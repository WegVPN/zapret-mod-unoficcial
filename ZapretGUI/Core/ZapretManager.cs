using System.Diagnostics;
using System.Text;
using Serilog;

namespace ZapretGUI.Core;

/// <summary>
/// Manages the zapret process lifecycle and configuration
/// </summary>
public class ZapretManager : IDisposable
{
    private Process? _zapretProcess;
    private readonly object _lock = new();
    private bool _disposed;

    public event EventHandler<LogEventArgs>? LogOutput;
    public event EventHandler<StateChangedEventArgs>? StateChanged;

    public bool IsRunning => _zapretProcess != null && !_zapretProcess.HasExited;
    public string? ZapretPath { get; set; }
    public string? CurrentProfile { get; private set; }
    public ZapretConfig? CurrentConfig { get; private set; }

    public void Initialize(string zapretPath)
    {
        ZapretPath = zapretPath;
        Log.Information("Zapret initialized with path: {Path}", zapretPath);
    }

    public void Start(ZapretConfig config)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ZapretManager));
        
        if (string.IsNullOrEmpty(ZapretPath))
            throw new InvalidOperationException("Zapret path not initialized");

        if (!File.Exists(ZapretPath))
            throw new FileNotFoundException("Zapret executable not found", ZapretPath);

        lock (_lock)
        {
            Stop();

            var arguments = BuildArguments(config);
            Log.Information("Starting zapret with arguments: {Args}", arguments);

            var startInfo = new ProcessStartInfo
            {
                FileName = ZapretPath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            _zapretProcess = new Process { StartInfo = startInfo };
            _zapretProcess.OutputDataReceived += OnOutputDataReceived;
            _zapretProcess.ErrorDataReceived += OnErrorDataReceived;
            _zapretProcess.Exited += OnProcessExited;
            _zapretProcess.EnableRaisingEvents = true;

            _zapretProcess.Start();
            _zapretProcess.BeginOutputReadLine();
            _zapretProcess.BeginErrorReadLine();

            CurrentConfig = config;
            CurrentProfile = config.ProfileName;

            Log.Information("Zapret started successfully with PID: {Pid}", _zapretProcess.Id);
            StateChanged?.Invoke(this, new StateChangedEventArgs(true, config.ProfileName));
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            if (_zapretProcess != null && !_zapretProcess.HasExited)
            {
                Log.Information("Stopping zapret process (PID: {Pid})", _zapretProcess.Id);
                
                try
                {
                    // Try graceful shutdown first
                    if (!_zapretProcess.CloseMainWindow())
                    {
                        _zapretProcess.Kill();
                    }
                    _zapretProcess.WaitForExit(5000);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error stopping zapret process");
                }
                finally
                {
                    _zapretProcess.OutputDataReceived -= OnOutputDataReceived;
                    _zapretProcess.ErrorDataReceived -= OnErrorDataReceived;
                    _zapretProcess.Exited -= OnProcessExited;
                    _zapretProcess.Dispose();
                    _zapretProcess = null;
                }

                CurrentConfig = null;
                CurrentProfile = null;
                StateChanged?.Invoke(this, new StateChangedEventArgs(false, null));
                Log.Information("Zapret stopped");
            }
        }
    }

    public void Restart(ZapretConfig config)
    {
        Stop();
        Thread.Sleep(500);
        Start(config);
    }

    private string BuildArguments(ZapretConfig config)
    {
        var args = new StringBuilder();

        // Add preset based on profile
        args.Append(config.Preset switch
        {
            ZapretPreset.Discord => "--preset=discord",
            ZapretPreset.YouTube => "--preset=youtube",
            ZapretPreset.Telegram => "--preset=telegram",
            ZapretPreset.All => "--preset=all",
            ZapretPreset.Custom => "",
            _ => ""
        });

        if (config.Preset == ZapretPreset.Custom)
        {
            // Build custom arguments
            if (!string.IsNullOrEmpty(config.Domains))
                args.Append($" --dpi-list=\"{config.Domains}\"");

            if (!string.IsNullOrEmpty(config.Ips))
                args.Append($" --ip-list=\"{config.Ips}\"");

            if (!string.IsNullOrEmpty(config.Ports))
                args.Append($" --ports={config.Ports}");
        }

        // Add method
        if (!string.IsNullOrEmpty(config.Method))
            args.Append($" --method={config.Method}");

        // Add interface if specified
        if (!string.IsNullOrEmpty(config.Interface))
            args.Append($" --iface={config.Interface}");

        // Add additional custom arguments
        if (!string.IsNullOrEmpty(config.AdditionalArgs))
            args.Append($" {config.AdditionalArgs}");

        return args.ToString().Trim();
    }

    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            Log.Information("Zapret: {Message}", e.Data);
            LogOutput?.Invoke(this, new LogEventArgs(e.Data, LogType.Info));
        }
    }

    private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            Log.Error("Zapret Error: {Message}", e.Data);
            LogOutput?.Invoke(this, new LogEventArgs(e.Data, LogType.Error));
        }
    }

    private void OnProcessExited(object? sender, EventArgs e)
    {
        Log.Warning("Zapret process exited unexpectedly");
        _zapretProcess?.Dispose();
        _zapretProcess = null;
        StateChanged?.Invoke(this, new StateChangedEventArgs(false, CurrentProfile));
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
}

/// <summary>
/// Configuration for zapret process
/// </summary>
public class ZapretConfig
{
    public string ProfileName { get; set; } = "Default";
    public ZapretPreset Preset { get; set; } = ZapretPreset.Custom;
    public string Method { get; set; } = "fake";
    public string Domains { get; set; } = "";
    public string Ips { get; set; } = "";
    public string Ports { get; set; } = "443,80";
    public string Interface { get; set; } = "";
    public string AdditionalArgs { get; set; } = "";
}

public enum ZapretPreset
{
    Discord,
    YouTube,
    Telegram,
    All,
    Custom
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
    public string? ProfileName { get; }

    public StateChangedEventArgs(bool isRunning, string? profileName)
    {
        IsRunning = isRunning;
        ProfileName = profileName;
    }
}
