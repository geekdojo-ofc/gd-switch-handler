using HidSharp;
using HidSharp.Reports;
using HidSharp.Reports.Encodings;
using Microsoft.Extensions.Logging;

namespace GD.SwitchHandler 
{
    public class HidDeviceHandler
    {
        private readonly ILogger _logger;
        private readonly ISwitchFactory _switchFactory;

        public HidDeviceHandler(ILogger logger, ISwitchFactory switchFactory)
        {
            _logger = logger;
            _switchFactory = switchFactory;
        }

        public void Load(int? vendorId, int? productId)
        {
            var deviceList = DeviceList.Local;
            var dev = deviceList.GetHidDeviceOrNull(vendorId, productId);            
            _switchFactory.LoadDevice(vendorId, productId);

            if(dev == null)
            {
                _logger.LogWarning($"Unable to locate device with vendorId {vendorId} and productId {productId}.");
                return;
            }

            _logger.LogDebug(dev.DevicePath);
            _logger.LogDebug(dev.ToString());

            try
            {
                _logger.LogDebug(string.Format("Max Lengths: Input {0}, Output {1}, Feature {2}",
                    dev.GetMaxInputReportLength(),
                    dev.GetMaxOutputReportLength(),
                    dev.GetMaxFeatureReportLength()));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(null, ex);
                return;
            }
            
            try
            {

                var rawReportDescriptor = dev.GetRawReportDescriptor();
                _logger.LogDebug(string.Format("Report Descriptor:\n  {0} ({1} bytes)"
                    , string.Join(" ", rawReportDescriptor.Select(d => d.ToString("X2")))
                    , rawReportDescriptor.Length));

                int indent = 0;
                foreach (var element in EncodedItem.DecodeItems(rawReportDescriptor, 0, rawReportDescriptor.Length))
                {
                    if (element.ItemType == ItemType.Main && element.TagForMain == MainItemTag.EndCollection) { indent -= 2; }

                    _logger.LogDebug("  {0}{1}", new string(' ', indent), element);

                    if (element.ItemType == ItemType.Main && element.TagForMain == MainItemTag.Collection) { indent += 2; }
                }

                var reportDescriptor = dev.GetReportDescriptor();
                foreach (var deviceItem in reportDescriptor.DeviceItems)
                {
                    foreach (var usage in deviceItem.Usages.GetAllValues())
                    {
                        _logger.LogDebug(string.Format("Usage: {0:X4} {1}", usage, (Usage)usage));
                    }
                    foreach (var report in deviceItem.Reports)
                    {
                        _logger.LogDebug(string.Format("{0}: ReportID={1}, Length={2}, Items={3}",
                                            report.ReportType, report.ReportID, report.Length, report.DataItems.Count));
                        foreach (var dataItem in report.DataItems)
                        {
                            _logger.LogDebug(string.Format("  {0} Elements x {1} Bits, Units: {2}, Expected Usage Type: {3}, Flags: {4}, Usages: {5}",
                                dataItem.ElementCount, dataItem.ElementBits, dataItem.Unit.System, dataItem.ExpectedUsageType, dataItem.Flags,
                                string.Join(", ", dataItem.Usages.GetAllValues().Select(usage => usage.ToString("X4") + " " + ((Usage)usage).ToString()))));

                            if(dataItem.ExpectedUsageType == ExpectedUsageType.PushButton)
                            {
                                Task.Run(()=> {
                                    Run(dev, reportDescriptor, deviceItem);
                                }).Wait();
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(null, ex);
                return;
            }
        }

        public void Run(HidDevice dev, ReportDescriptor reportDescriptor, DeviceItem deviceItem)
        {
            
            _logger.LogDebug("Opening device...");

            HidStream hidStream;
            if (dev.TryOpen(out hidStream))
            {
                _logger.LogDebug("Opened device.");
                hidStream.ReadTimeout = Timeout.Infinite;

                using (hidStream)
                {
                    var inputReportBuffer = new byte[dev.GetMaxInputReportLength()];
                    var inputReceiver = reportDescriptor.CreateHidDeviceInputReceiver();
                    var inputParser = deviceItem.CreateDeviceItemInputParser();

                    inputReceiver.Start(hidStream);
                    while (true)
                    {
                        if (!inputReceiver.IsRunning) { break; } // Disconnected?

                        Report report; // Periodically check if the receiver has any reports.
                        while (inputReceiver.TryRead(inputReportBuffer, 0, out report))
                        {
                            // Parse the report if possible.
                            // This will return false if (for example) the report applies to a different DeviceItem.
                            if (inputParser.TryParseReport(inputReportBuffer, 0, report))
                            {
                                while (inputParser.HasChanged)
                                {
                                    _switchFactory.Update(inputParser);
                                }
                            }
                        }
                    }
                }

                _logger.LogDebug("Closed device.");
            }
            else
            {
                _logger.LogDebug("Failed to open device.");
            }
        }
    }
}