using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Serilog;
namespace ZapretMod.Core;

public class ZapretEngine : IDisposable {
    private Process? _p;
    private bool _disposed;
    private readonly string _bin;
    public event EventHandler<LogEventArgs>? LogOutput;
    public event EventHandler<StateChangedEventArgs>? StateChanged;
    public bool Running => _p != null && !_p.HasExited;
    public ZapretEngine() => _bin = Path.Combine(AppContext.BaseDirectory, "bin");
    public bool CheckFiles() => File.Exists(Path.Combine(_bin, "winws.exe")) && File.Exists(Path.Combine(_bin, "WinDivert64.sys"));
    public void Start(StrategyConfig cfg) {
        var exe = Path.Combine(_bin, "winws.exe");
        if (!File.Exists(exe)) throw new FileNotFoundException("winws.exe не найден! Скачайте файлы.", exe);
        Stop();
        var args = BuildArgs(cfg);
        Log.Information("Start: {Args}", args);
        var si = new ProcessStartInfo { FileName = exe, Arguments = args, UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true, WorkingDirectory = _bin };
        _p = new Process { StartInfo = si };
        _p.OutputDataReceived += (s, e) => { if (e.Data != null) LogOutput?.Invoke(this, new LogEventArgs(e.Data, LogType.Info)); };
        _p.ErrorDataReceived += (s, e) => { if (e.Data != null) LogOutput?.Invoke(this, new LogEventArgs(e.Data, LogType.Error)); };
        _p.Exited += (s, e) => { _p = null; StateChanged?.Invoke(this, new StateChangedEventArgs(false, null)); };
        _p.EnableRaisingEvents = true;
        _p.Start();
        _p.BeginOutputReadLine();
        _p.BeginErrorReadLine();
        StateChanged?.Invoke(this, new StateChangedEventArgs(true, cfg.Name));
    }
    public void Stop() { if (_p != null && !_p.HasExited) { try { _p.Kill(); _p.WaitForExit(3000); } catch {} _p = null; StateChanged?.Invoke(this, new StateChangedEventArgs(false, null)); } }
    private string BuildArgs(StrategyConfig c) {
        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(c.Wf)) sb.Append($"--wf={c.Wf} ");
        if (!string.IsNullOrEmpty(c.Dpi)) sb.Append($"--dpi-desync={c.Dpi} ");
        if (c.Oob) sb.Append("--dpi-desync-fake-tls=oob ");
        if (c.Autottls) sb.Append("--dpi-desync-autottls=1 ");
        if (!string.IsNullOrEmpty(c.Ports)) sb.Append($"--ports={c.Ports} ");
        if (!string.IsNullOrEmpty(c.Extra)) sb.Append(c.Extra);
        return sb.ToString().Trim();
    }
    public void Dispose() { if (!_disposed) { Stop(); _disposed = true; GC.SuppressFinalize(this); } }
    public static StrategyConfig[] GetStrategies() => new[] {
        new StrategyConfig { Name = "Discord + YouTube + Telegram", Description = "Основной профиль для всех сервисов. Рекомендуется для большинства пользователей.", Wf = "l3", Dpi = "fake", Autottls = true, Ports = "443,80,8443", Extra = "--dpi-desync-fake-tls=oob" },
        new StrategyConfig { Name = "Discord Only", Description = "Только для Discord (включая голосовые каналы)", Wf = "l3", Dpi = "fake", Autottls = true, Ports = "443,80" },
        new StrategyConfig { Name = "YouTube Only", Description = "Только для YouTube (4K стриминг)", Wf = "l3", Dpi = "fake", Autottls = true, Ports = "443,80" },
        new StrategyConfig { Name = "Telegram Only", Description = "Только для Telegram (мессенджер + proxy)", Wf = "l3", Dpi = "fake", Ports = "443,80,8443" },
        new StrategyConfig { Name = "FAKE TLS AUTO", Description = "Автоматический выбор fake TLS параметров", Wf = "l3", Dpi = "fake", Autottls = true, Oob = true, Ports = "443,80" },
        new StrategyConfig { Name = "SIMPLE FAKE", Description = "Простой fake метод (минимальные задержки)", Wf = "l3", Dpi = "simple-fake", Ports = "443,80" }
    };
}
public class StrategyConfig { public string Name { get; set; } = ""; public string Description { get; set; } = ""; public string Wf { get; set; } = ""; public string Dpi { get; set; } = ""; public bool Oob { get; set; } public bool Autottls { get; set; } public string Ports { get; set; } = ""; public string Extra { get; set; } = ""; public override string ToString() => Name; }
public class LogEventArgs(string msg, LogType t) : EventArgs { public string Message => msg; public LogType Type => t; }
public class StateChangedEventArgs(bool r, string? s) : EventArgs { public bool IsRunning => r; public string? Strategy => s; }
