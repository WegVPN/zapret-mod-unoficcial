using System.Diagnostics;
using System.Text;
using Serilog;

namespace ZapretMod.Core;

/// <summary>
/// Manages winws.exe (zapret) process - similar to flowseal/zapret-discord-youtube
/// </summary>
public class ZapretEngine : IDisposable
{
    private Process? _process;
    private readonly object _lock = new();
    private bool _disposed;
    private readonly string _binPath;

    public event EventHandler<LogEventArgs>? LogOutput;
    public event EventHandler<StateChangedEventArgs>? StateChanged;

    public bool IsRunning => _process != null && !_process.HasExited;
    public string? CurrentStrategy { get; private set; }
    public StrategyConfig? CurrentConfig { get; private set; }

    public ZapretEngine(string? binPath = null)
    {
        _binPath = binPath ?? Path.Combine(AppContext.BaseDirectory, "bin");
    }

    public void Initialize()
    {
        if (!Directory.Exists(_binPath))
            Directory.CreateDirectory(_binPath);
        
        Log.Information("ZapretEngine initialized. Bin path: {Path}", _binPath);
    }

    public bool CheckBinaries()
    {
        var winws = Path.Combine(_binPath, "winws.exe");
        var windivert = Path.Combine(_binPath, "WinDivert64.sys");
        
        if (!File.Exists(winws))
        {
            Log.Error("winws.exe not found in {Path}", _binPath);
            return false;
        }
        
        if (!File.Exists(windivert))
        {
            Log.Warning("WinDivert64.sys not found - may need installation");
        }
        
        return true;
    }

    public void Start(StrategyConfig config)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ZapretEngine));

        var winws = Path.Combine(_binPath, "winws.exe");
        
        if (!File.Exists(winws))
            throw new FileNotFoundException("winws.exe not found", winws);

        lock (_lock)
        {
            Stop();

            var arguments = BuildArguments(config);
            Log.Information("Starting winws with strategy: {Strategy}, args: {Args}", config.Name, arguments);

            var startInfo = new ProcessStartInfo
            {
                FileName = winws,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                WorkingDirectory = _binPath
            };

            _process = new Process { StartInfo = startInfo };
            _process.OutputDataReceived += OnOutputDataReceived;
            _process.ErrorDataReceived += OnErrorDataReceived;
            _process.Exited += OnProcessExited;
            _process.EnableRaisingEvents = true;

            try
            {
                _process.Start();
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
                
                CurrentConfig = config;
                CurrentStrategy = config.Name;
                
                Log.Information("winws started with PID: {Pid}", _process.Id);
                StateChanged?.Invoke(this, new StateChangedEventArgs(true, config.Name));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to start winws");
                throw;
            }
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            if (_process != null && !_process.HasExited)
            {
                Log.Information("Stopping winws (PID: {Pid})", _process.Id);
                
                try
                {
                    if (!_process.CloseMainWindow())
                    {
                        _process.Kill();
                    }
                    _process.WaitForExit(5000);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error stopping winws");
                }
                finally
                {
                    _process.OutputDataReceived -= OnOutputDataReceived;
                    _process.ErrorDataReceived -= OnErrorDataReceived;
                    _process.Exited -= OnProcessExited;
                    _process.Dispose();
                    _process = null;
                }

                CurrentConfig = null;
                CurrentStrategy = null;
                StateChanged?.Invoke(this, new StateChangedEventArgs(false, null));
                Log.Information("winws stopped");
            }
        }
    }

    public void Restart(StrategyConfig config)
    {
        Stop();
        Thread.Sleep(500);
        Start(config);
    }

    private string BuildArguments(StrategyConfig config)
    {
        var args = new StringBuilder();

        // Main strategy parameters (based on flowseal/zapret-discord-youtube)
        if (!string.IsNullOrEmpty(config.Wf))
            args.Append($"--wf={config.Wf} ");
        
        if (!string.IsNullOrEmpty(config.Dpi))
            args.Append($"--dpi-desync={config.Dpi} ");
        
        if (config.Oob)
            args.Append("--dpi-desync-fake-tls=oob ");
        
        if (!string.IsNullOrEmpty(config.FakeTls))
            args.Append($"--dpi-desync-fake-tls={config.FakeTls} ");
        
        if (config.Autottls)
            args.Append("--dpi-desync-autottls=1 ");
        
        if (config.Nat)
            args.Append("--nat ");
        
        if (!string.IsNullOrEmpty(config.Ports))
            args.Append($"--ports={config.Ports} ");
        
        if (!string.IsNullOrEmpty(config.IpList))
            args.Append($"--ip-list={config.IpList} ");
        
        if (!string.IsNullOrEmpty(config.DomainList))
            args.Append($"--domain-list={config.DomainList} ");

        // Additional custom arguments
        if (!string.IsNullOrEmpty(config.ExtraArgs))
            args.Append($" {config.ExtraArgs}");

        return args.ToString().Trim();
    }

    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            Log.Information("winws: {Message}", e.Data);
            LogOutput?.Invoke(this, new LogEventArgs(e.Data, LogType.Info));
        }
    }

    private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            Log.Error("winws error: {Message}", e.Data);
            LogOutput?.Invoke(this, new LogEventArgs(e.Data, LogType.Error));
        }
    }

    private void OnProcessExited(object? sender, EventArgs e)
    {
        Log.Warning("winws process exited unexpectedly");
        _process?.Dispose();
        _process = null;
        StateChanged?.Invoke(this, new StateChangedEventArgs(false, CurrentStrategy));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Stop();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Get available strategies (similar to flowseal general.bat variants)
    /// </summary>
    public static List<StrategyConfig> GetBuiltInStrategies()
    {
        return new List<StrategyConfig>
        {
            // Main strategy - Discord + YouTube + Telegram
            new StrategyConfig
            {
                Name = "Discord + YouTube + Telegram",
                Description = "Основной профиль для Discord, YouTube и Telegram",
                Wf = "l3",
                Dpi = "fake",
                Autottls = true,
                Ports = "443,80,8443",
                ExtraArgs = "--dpi-desync-fake-tls=oob"
            },
            // Discord only
            new StrategyConfig
            {
                Name = "Discord Only",
                Description = "Только для Discord (включая голосовые каналы)",
                Wf = "l3",
                Dpi = "fake",
                Autottls = true,
                Ports = "443,80",
                ExtraArgs = "--domain-list=discord.txt"
            },
            // YouTube only
            new StrategyConfig
            {
                Name = "YouTube Only",
                Description = "Только для YouTube",
                Wf = "l3",
                Dpi = "fake",
                Autottls = true,
                Ports = "443,80",
                ExtraArgs = "--domain-list=youtube.txt"
            },
            // Telegram only
            new StrategyConfig
            {
                Name = "Telegram Only",
                Description = "Только для Telegram",
                Wf = "l3",
                Dpi = "fake",
                Ports = "443,80,8443",
                IpList = "telegram.txt"
            },
            // FAKE TLS AUTO (like flowseal)
            new StrategyConfig
            {
                Name = "FAKE TLS AUTO",
                Description = "Автоматический выбор fake TLS параметров",
                Wf = "l3",
                Dpi = "fake",
                Autottls = true,
                Oob = true,
                Ports = "443,80"
            },
            // ALT Strategy 1
            new StrategyConfig
            {
                Name = "ALT Strategy 1",
                Description = "Альтернативный метод 1",
                Wf = "l3",
                Dpi = "fake",
                FakeTls = "fake.bin",
                Ports = "443,80"
            },
            // SIMPLE FAKE
            new StrategyConfig
            {
                Name = "SIMPLE FAKE",
                Description = "Простой fake метод",
                Wf = "l3",
                Dpi = "simple-fake",
                Ports = "443,80"
            },
            // Custom
            new StrategyConfig
            {
                Name = "Custom",
                Description = "Пользовательская конфигурация",
                Wf = "",
                Dpi = "",
                ExtraArgs = ""
            }
        };
    }
}

/// <summary>
/// Strategy configuration (similar to flowseal/zapret-discord-youtube .bat files)
/// </summary>
public class StrategyConfig
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Wf { get; set; } = "";          // --wf parameter (l3, l2, etc.)
    public string Dpi { get; set; } = "";         // --dpi-desync parameter
    public bool Oob { get; set; }                 // --dpi-desync-fake-tls=oob
    public string? FakeTls { get; set; }          // --dpi-desync-fake-tls=file.bin
    public bool Autottls { get; set; }            // --dpi-desync-autottls=1
    public bool Nat { get; set; }                 // --nat
    public string? Ports { get; set; }            // --ports
    public string? IpList { get; set; }           // --ip-list
    public string? DomainList { get; set; }       // --domain-list
    public string? ExtraArgs { get; set; }        // Additional arguments
    
    public override string ToString() => Name;
}

public class LogEventArgs : EventArgs
{
    public string Message { get; }
    public LogType Type { get; }

    public LogEventArgs(string message, LogType type)
    {
        Message = message;
        Type = type;
    }
}

public enum LogType
{
    Info,
    Error,
    Warning,
    Debug
}

public class StateChangedEventArgs : EventArgs
{
    public bool IsRunning { get; }
    public string? StrategyName { get; }

    public StateChangedEventArgs(bool isRunning, string? strategyName)
    {
        IsRunning = isRunning;
        StrategyName = strategyName;
    }
}
