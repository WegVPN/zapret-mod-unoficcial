using Serilog;
using ZapretGUI.Core;
using System.Runtime.InteropServices;

namespace ZapretGUI;

internal static class Program
{
    private static Mutex _mutex = null!;
    private const string MutexName = "ZapretGUI_SingleInstance";

    [STAThread]
    static void Main()
    {
        // Ensure single instance
        _mutex = new Mutex(true, MutexName, out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show(
                "ZapretGUI уже запущен.\nПожалуйста, используйте иконку в системном трее.",
                "ZapretGUI",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        // Configure logging
        ConfigureLogging();

        try
        {
            Log.Information("=== ZapretGUI Starting ===");
            Log.Information("Version: {Version}", typeof(Program).Assembly.GetName().Version);
            Log.Information("OS Version: {OSVersion}", Environment.OSVersion);
            Log.Information("Framework: {Framework}", RuntimeInformation.FrameworkDescription);

            // Check for admin privileges
            if (!IsRunningAsAdmin())
            {
                Log.Warning("Application not running as administrator");
                // Restart as admin
                RestartAsAdmin();
                return;
            }

            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());

            Log.Information("=== ZapretGUI Exiting ===");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            MessageBox.Show(
                $"Произошла критическая ошибка:\n{ex.Message}\n\nЛог файл: {GetLogFilePath()}",
                "ZapretGUI - Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            _mutex.ReleaseMutex();
            _mutex.Dispose();
            Log.CloseAndFlush();
        }
    }

    private static void ConfigureLogging()
    {
        var logPath = GetLogFilePath();
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Console()
            .CreateLogger();
    }

    private static string GetLogFilePath()
    {
        var logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ZapretGUI",
            "Logs");
        
        Directory.CreateDirectory(logDir);
        return Path.Combine(logDir, "zapretgui_.log");
    }

    private static bool IsRunningAsAdmin()
    {
        using var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
        var principal = new System.Security.Principal.WindowsPrincipal(identity);
        return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
    }

    private static void RestartAsAdmin()
    {
        try
        {
            var exePath = Environment.ProcessPath ?? 
                Path.Combine(AppContext.BaseDirectory, "ZapretGUI.exe");
            
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = true,
                Verb = "runas"
            };

            System.Diagnostics.Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to restart as administrator");
            MessageBox.Show(
                "Для работы приложения требуются права администратора.\n" +
                "Пожалуйста, запустите программу от имени администратора.",
                "ZapretGUI - Требуется администратор",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
