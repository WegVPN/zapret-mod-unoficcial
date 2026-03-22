using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZapretMod.Core;
using Serilog;

namespace ZapretMod.Service;

public class ZapretModWorker : BackgroundService
{
    private readonly ILogger<ZapretModWorker> _logger;
    private readonly ZapretEngine _zapretEngine;

    public ZapretModWorker(ILogger<ZapretModWorker> logger)
    {
        _logger = logger;
        _zapretEngine = new ZapretEngine();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ZapretMod Service starting...");
        
        _zapretEngine.Initialize();
        
        if (!_zapretEngine.CheckBinaries())
        {
            _logger.LogError("Required binaries not found. Service will not start.");
            return;
        }

        // Load default strategy
        var strategies = ZapretEngine.GetBuiltInStrategies();
        var defaultStrategy = strategies.FirstOrDefault(s => s.Name == "Discord + YouTube + Telegram") 
            ?? strategies.First();

        _logger.LogInformation("Starting strategy: {Strategy}", defaultStrategy.Name);
        _zapretEngine.Start(defaultStrategy);

        await Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ZapretMod Service stopping...");
        _zapretEngine.Stop();
        await base.StopAsync(cancellationToken);
    }
}
