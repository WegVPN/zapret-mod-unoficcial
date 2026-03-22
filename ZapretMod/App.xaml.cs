using System.IO;
using System.Windows;
using Serilog;
namespace ZapretMod;
public partial class App : Application {
    protected override void OnStartup(StartupEventArgs e) {
        base.OnStartup(e);
        var log = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ZapretMod", "Logs", "app-.log");
        Directory.CreateDirectory(Path.GetDirectoryName(log)!);
        Log.Logger = new LoggerConfiguration().MinimumLevel.Information().WriteTo.File(log, rollingInterval: RollingInterval.Day).CreateLogger();
        Log.Information("=== ZapretMod v3.0 Starting ===");
    }
    protected override void OnExit(ExitEventArgs e) { Log.Information("=== ZapretMod Exiting ==="); Log.CloseAndFlush(); base.OnExit(e); }
}
