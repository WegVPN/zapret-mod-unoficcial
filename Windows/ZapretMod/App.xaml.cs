using System.IO;
using System.Windows;
using Serilog;

namespace ZapretMod;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ZapretMod",
            "Logs",
            "app-.log");

        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateLogger();

        Log.Information("=== ZapretMod Starting ===");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("=== ZapretMod Exiting ===");
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
