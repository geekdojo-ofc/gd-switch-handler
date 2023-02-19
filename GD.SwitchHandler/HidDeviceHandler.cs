using HidSharp;
using HidSharp.Reports;
using HidSharp.Reports.Encodings;
using HidSharp.Reports.Input;
using Microsoft.Extensions.Logging;

namespace GD.SwitchHandler 
{
    public class HidDeviceHandler
    {
        private readonly ILogger _logger;
        private readonly ISwitchFactory _switchFactory;
        private ManualResetEvent _manual = new ManualResetEvent(false);

        public HidDeviceHandler(ILogger logger, ISwitchFactory switchFactory)
        {
            Stopping = false;
            _logger = logger;
            _switchFactory = switchFactory;
        }

        public bool Stopping { get; set; } 

        public void Load(int? vendorId, int? productId)
        {
            var deviceList = DeviceList.Local;
            var allDevices = deviceList.GetHidDevices();
            if(allDevices == null)
            {
                _logger.LogDebug("Unable to find any HID devices.");
                return;
            }
            HidDevice? dev = allDevices.FirstOrDefault(d => d.MaxInputReportLength > 0 && d.VendorID == vendorId && d.ProductID == productId);
            
            //var dev = deviceList.GetHidDeviceOrNull(vendorId, productId);            
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

                var reportDescriptor = dev.GetReportDescriptor();

                var autos = new AutoResetEvent[reportDescriptor.DeviceItems.Count];// [reportDescriptor.Reports.Count];
                var i = 0;

                if(reportDescriptor.DeviceItems.Count > 1)
                {
                    _logger.LogWarning("HID Device has multiple report descriptors. Picking the first one but this may cause unexpected behavior.");
                }
                var inputParser = reportDescriptor.DeviceItems.FirstOrDefault()?.CreateDeviceItemInputParser();

                var auto = new AutoResetEvent(false);
                autos[i] = auto;
                i++;

                var t = new Thread(() => Listen(auto, dev, reportDescriptor, inputParser));
                t.Start();

                WaitHandle.WaitAll(autos);
                _manual.Reset();
            }
            catch(Exception ex)
            {
                _logger.LogError(null, ex);
                return;
            }
        }

        private void Listen(AutoResetEvent auto, HidDevice dev, ReportDescriptor reportDescriptor, DeviceItemInputParser inputParser)
        {
            _manual.Set();

            try
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
                        inputReceiver.Start(hidStream);

                        while (true && !Stopping)
                        {
                            if (!inputReceiver.IsRunning) { break; } // Disconnected?

                            Report report; // Periodically check if the receiver has any reports.
                            while (inputReceiver.TryRead(inputReportBuffer, 0, out report))
                            {
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
            catch (Exception ex)
            {
            }
            finally
            {
                auto.Set();
            }
        }
    }
}