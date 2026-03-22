using Microsoft.Extensions.Hosting;
using Serilog;

namespace ZapretMod.Service;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "ZapretMod",
                    "Logs",
                    "service-.log"),
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            Log.Information("ZapretMod Service starting");
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Service terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseWindowsService()
            .ConfigureServices(services =>
            {
                services.AddHostedService<ZapretModWorker>();
            })
            .UseSerilog();
}
