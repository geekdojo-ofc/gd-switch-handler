// See https://aka.ms/new-console-template for more information
using GD.SwitchHandler;
using Microsoft.Extensions.Logging;

namespace GD.SwitchHandler.Functional.Tests;

public static class Program
{
    private static HidDeviceHandler _hidDeviceHandler;

    public static void Main()
    {

        Console.WriteLine("Start...");


        // create a logger factory
        var loggerFactory = LoggerFactory.Create(
            builder => builder
                        // add console as logging target
                        .AddConsole()
                        // add debug output as logging target
                        //.AddDebug()
                        // set minimum level to log
                        .SetMinimumLevel(LogLevel.Debug)
        );

        var logger = loggerFactory.CreateLogger("GD.SwitchHandler.Functional.Tests");

        var switchFactory = new SwitchFactory(logger);
        switchFactory.StateChanged += (s, e) =>
        {
            logger.LogInformation($"{s.Id}: {s.State}");
        };

        var hidDeviceHandler = new HidDeviceHandler(logger, switchFactory);
        hidDeviceHandler.Load(0x3344, 0x0259);
    }
}
