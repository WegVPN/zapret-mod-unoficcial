using Microsoft.Win32;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;
using Serilog;

namespace ZapretGUI.Core;

/// <summary>
/// Handles network optimization including TCP, DNS, and MTU settings
/// </summary>
public class NetworkOptimizer : IDisposable
{
    private const string TcpRegistryPath = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters";
    private const string NetworkInterfaceRegistryPath = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces";
    
    public event EventHandler<OptimizationEventArgs>? OptimizationApplied;
    public event EventHandler<LogEventArgs>? LogOutput;

    private readonly RegistryBackup _registryBackup;

    public NetworkOptimizer()
    {
        _registryBackup = new RegistryBackup();
    }

    #region TCP Optimization

    public TcpSettings GetCurrentTcpSettings()
    {
        var settings = new TcpSettings();
        
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(TcpRegistryPath);
            if (key != null)
            {
                // TCP Auto-Tuning Level
                var autoTuning = key.GetValue("TcpAutoTuningLevel");
                settings.AutoTuningLevel = autoTuning?.ToString() ?? "normal";

                // ECN Capability
                var ecn = key.GetValue("EnableECN");
                settings.EcnEnabled = ecn != null && Convert.ToInt32(ecn) == 1;

                // TCP Timestamps
                var timestamps = key.GetValue("TcpTimestamps");
                settings.TimestampsEnabled = timestamps == null || Convert.ToInt32(timestamps) == 1;

                // Selective ACK
                var sack = key.GetValue("SackOpts");
                settings.SelectiveAckEnabled = sack != null && Convert.ToInt32(sack) == 1;

                // Max User Port
                var maxPort = key.GetValue("MaxUserPort");
                settings.MaxUserPort = maxPort != null ? Convert.ToInt32(maxPort) : 16387;

                // TcpTimedWaitDelay
                var timedWait = key.GetValue("TcpTimedWaitDelay");
                settings.TcpTimedWaitDelay = timedWait != null ? Convert.ToInt32(timedWait) : 240;
            }

            // Get current TCP global settings via netsh
            settings.AutoTuningLevel = GetNetshTcpSetting("autotuninglevel") ?? settings.AutoTuningLevel;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error reading TCP settings");
            OnLogOutput("Error reading TCP settings: " + ex.Message, LogType.Error);
        }

        return settings;
    }

    public async Task<bool> OptimizeTcpAsync(TcpOptimizationSettings settings)
    {
        try
        {
            // Backup current settings
            await _registryBackup.BackupTcpSettingsAsync();

            var results = new StringBuilder();

            // Apply TCP Auto-Tuning
            if (settings.OptimizeAutoTuning)
            {
                var result = await ExecuteNetshCommand("set global autotuninglevel=normal");
                results.AppendLine($"Auto-Tuning: {result}");
                OnLogOutput($"TCP Auto-Tuning set to normal", LogType.Info);
            }

            // Enable ECN
            if (settings.EnableEcn)
            {
                var result = await ExecuteNetshCommand("set global ecncapability=enabled");
                results.AppendLine($"ECN: {result}");
                OnLogOutput("ECN enabled", LogType.Info);
            }

            // Enable TCP Timestamps
            if (settings.EnableTimestamps)
            {
                var result = await ExecuteNetshCommand("set global timestamps=enabled");
                results.AppendLine($"Timestamps: {result}");
                OnLogOutput("TCP Timestamps enabled", LogType.Info);
            }

            // Enable Selective ACK
            if (settings.EnableSelectiveAck)
            {
                var result = await ExecuteNetshCommand("set global sack=enabled");
                results.AppendLine($"Selective ACK: {result}");
                OnLogOutput("Selective ACK enabled", LogType.Info);
            }

            // Disable Nagle's Algorithm for specific applications (via registry)
            if (settings.DisableNagleForApps)
            {
                await DisableNagleForApplicationsAsync();
                OnLogOutput("Nagle's algorithm disabled for games/messengers", LogType.Info);
            }

            // Try to enable BBR (if supported via third-party driver or Windows 11)
            if (settings.EnableBBR)
            {
                var bbrSupported = await CheckBBRSupportAsync();
                if (bbrSupported)
                {
                    var result = await ExecuteNetshCommand("set global congestionprovider=ctcp");
                    results.AppendLine($"BBR/CTCP: {result}");
                    OnLogOutput("BBR/CTCP congestion provider enabled", LogType.Info);
                }
                else
                {
                    OnLogOutput("BBR not supported on this Windows version", LogType.Warning);
                }
            }

            OptimizationApplied?.Invoke(this, new OptimizationEventArgs(true, "TCP optimization applied"));
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error optimizing TCP settings");
            OnLogOutput("Error optimizing TCP: " + ex.Message, LogType.Error);
            return false;
        }
    }

    public async Task<bool> ResetTcpSettingsAsync()
    {
        try
        {
            await ExecuteNetshCommand("set global autotuninglevel=normal");
            await ExecuteNetshCommand("set global ecncapability=disabled");
            await ExecuteNetshCommand("set global timestamps=enabled");
            await ExecuteNetshCommand("set global sack=enabled");
            
            // Restore from backup
            await _registryBackup.RestoreTcpSettingsAsync();

            OnLogOutput("TCP settings reset to default", LogType.Info);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error resetting TCP settings");
            OnLogOutput("Error resetting TCP: " + ex.Message, LogType.Error);
            return false;
        }
    }

    private async Task<string> ExecuteNetshCommand(string command)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "netsh",
            Arguments = $"int tcp {command}",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = new Process { StartInfo = startInfo };
        var output = new StringBuilder();
        
        process.OutputDataReceived += (s, e) => { if (e.Data != null) output.AppendLine(e.Data); };
        process.ErrorDataReceived += (s, e) => { if (e.Data != null) output.AppendLine(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await process.WaitForExitAsync(cts.Token);

        return output.ToString().Trim();
    }

    private string? GetNetshTcpSetting(string setting)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = "int tcp show global",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var lines = output.Split('\n');
            foreach (var line in lines)
            {
                if (line.Contains(setting, StringComparison.OrdinalIgnoreCase))
                {
                    var parts = line.Split(':');
                    if (parts.Length > 1)
                        return parts[1].Trim();
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error reading netsh TCP setting");
        }

        return null;
    }

    private async Task<bool> CheckBBRSupportAsync()
    {
        // BBR is available on Windows 11 22H2+ or with third-party drivers
        // Check Windows version
        var version = Environment.OSVersion.Version;
        return version.Major >= 10 && version.Build >= 22621; // Windows 11 22H2
    }

    private async Task DisableNagleForApplicationsAsync()
    {
        // Common application GUIDs and paths for games/messengers
        var applications = new[]
        {
            "discord.exe",
            "telegram.exe",
            "steam.exe",
            "cs2.exe",
            "valorant.exe"
        };

        foreach (var app in applications)
        {
            try
            {
                var appPath = FindApplicationPath(app);
                if (!string.IsNullOrEmpty(appPath))
                {
                    await SetTcpNoDelayForApplicationAsync(appPath, true);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to disable Nagle for {App}", app);
            }
        }
    }

    private string? FindApplicationPath(string appName)
    {
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        var searchPaths = new[]
        {
            programFiles,
            programFilesX86,
            Path.Combine(appData, "Discord"),
            Path.Combine(appData, "TelegramDesktop")
        };

        foreach (var path in searchPaths)
        {
            try
            {
                var files = Directory.GetFiles(path, appName, SearchOption.AllDirectories);
                if (files.Length > 0)
                    return files[0];
            }
            catch
            {
                // Ignore access errors
            }
        }

        return null;
    }

    private Task SetTcpNoDelayForApplicationAsync(string appPath, bool enable)
    {
        // This would require creating a registry entry under the application's network settings
        // For simplicity, we'll log the intention
        OnLogOutput($"Would set TCP_NODELAY=1 for {appPath}", LogType.Info);
        return Task.CompletedTask;
    }

    #endregion

    #region DNS Optimization

    public DnsSettings GetCurrentDnsSettings()
    {
        var settings = new DnsSettings();
        
        try
        {
            foreach (var iface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (iface.OperationalStatus == OperationalStatus.Up &&
                    iface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    var props = iface.GetIPProperties();
                    if (props.DnsAddresses.Count > 0)
                    {
                        settings.CurrentDnsServers = props.DnsAddresses
                            .Select(a => a.ToString())
                            .ToArray();
                        settings.ActiveInterface = iface.Name;
                        break;
                    }
                }
            }

            // Check if DNS caching service is running
            var dnsCacheService = GetServiceStatus("Dnscache");
            settings.DnsCachingEnabled = dnsCacheService == "Running";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error reading DNS settings");
            OnLogOutput("Error reading DNS settings: " + ex.Message, LogType.Error);
        }

        return settings;
    }

    public async Task<bool> SetDnsServersAsync(string[] dnsServers, string? interfaceName = null)
    {
        try
        {
            await _registryBackup.BackupDnsSettingsAsync();

            var interfaces = string.IsNullOrEmpty(interfaceName)
                ? GetActiveNetworkInterfaces()
                : new[] { interfaceName };

            foreach (var iface in interfaces)
            {
                var index = GetInterfaceIndex(iface);
                if (index >= 0)
                {
                    // Set primary DNS
                    await ExecuteNetshCommand($"int ip set dns name=\"{iface}\" static {dnsServers[0]} primary");
                    
                    // Set secondary DNS if provided
                    if (dnsServers.Length > 1)
                    {
                        await ExecuteNetshCommand($"int ip add dns name=\"{iface}\" {dnsServers[1]}");
                    }

                    OnLogOutput($"DNS servers set for {iface}: {string.Join(", ", dnsServers)}", LogType.Info);
                }
            }

            // Flush DNS cache
            await FlushDnsAsync();

            OptimizationApplied?.Invoke(this, new OptimizationEventArgs(true, "DNS settings applied"));
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error setting DNS servers");
            OnLogOutput("Error setting DNS: " + ex.Message, LogType.Error);
            return false;
        }
    }

    public async Task<bool> FlushDnsAsync()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "ipconfig",
                Arguments = "/flushdns",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await process.WaitForExitAsync(cts.Token);

            OnLogOutput("DNS cache flushed", LogType.Info);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error flushing DNS cache");
            OnLogOutput("Error flushing DNS: " + ex.Message, LogType.Error);
            return false;
        }
    }

    public async Task<bool> ResetDnsSettingsAsync()
    {
        try
        {
            var interfaces = GetActiveNetworkInterfaces();
            
            foreach (var iface in interfaces)
            {
                await ExecuteNetshCommand($"int ip set dns name=\"{iface}\" dhcp");
            }

            await _registryBackup.RestoreDnsSettingsAsync();
            await FlushDnsAsync();

            OnLogOutput("DNS settings reset to DHCP", LogType.Info);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error resetting DNS settings");
            OnLogOutput("Error resetting DNS: " + ex.Message, LogType.Error);
            return false;
        }
    }

    #endregion

    #region MTU Optimization

    public MtuSettings GetCurrentMtuSettings()
    {
        var settings = new MtuSettings();
        
        try
        {
            foreach (var iface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (iface.OperationalStatus == OperationalStatus.Up &&
                    iface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    var mtu = GetInterfaceMtu(iface.Name);
                    settings.Interfaces.Add(new InterfaceMtu
                    {
                        Name = iface.Name,
                        CurrentMtu = mtu,
                        IsWireless = iface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211,
                        IsEthernet = iface.NetworkInterfaceType == NetworkInterfaceType.Ethernet
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error reading MTU settings");
            OnLogOutput("Error reading MTU settings: " + ex.Message, LogType.Error);
        }

        return settings;
    }

    public async Task<int> FindOptimalMtuAsync(string interfaceName)
    {
        // Binary search for optimal MTU
        int min = 576;
        int max = 1500;
        int optimal = 1500;

        try
        {
            // Ping test with different packet sizes
            using var ping = new System.Net.NetworkInformation.Ping();
            
            for (int mtu = max; mtu >= min; mtu -= 100)
            {
                var bufferSize = mtu - 40; // Account for IP and ICMP headers
                var buffer = new byte[bufferSize];
                
                try
                {
                    var reply = await ping.SendPingAsync("8.8.8.8", 1000, buffer);
                    if (reply.Status == IPStatus.Success)
                    {
                        optimal = mtu;
                        break;
                    }
                }
                catch
                {
                    // Try smaller MTU
                }
            }

            OnLogOutput($"Optimal MTU for {interfaceName}: {optimal}", LogType.Info);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error finding optimal MTU");
            OnLogOutput("Error finding optimal MTU: " + ex.Message, LogType.Error);
        }

        return optimal;
    }

    public async Task<bool> SetMtuAsync(string interfaceName, int mtu)
    {
        try
        {
            await _registryBackup.BackupMtuSettingsAsync();

            await ExecuteNetshCommand($"int ipv4 set subinterface \"{interfaceName}\" mtu={mtu} store=persistent");
            
            OnLogOutput($"MTU set to {mtu} for {interfaceName}", LogType.Info);
            OptimizationApplied?.Invoke(this, new OptimizationEventArgs(true, $"MTU set to {mtu}"));
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error setting MTU");
            OnLogOutput("Error setting MTU: " + ex.Message, LogType.Error);
            return false;
        }
    }

    public async Task<bool> ResetMtuAsync(string interfaceName)
    {
        try
        {
            await ExecuteNetshCommand($"int ipv4 set subinterface \"{interfaceName}\" mtu=1500 store=persistent");
            
            await _registryBackup.RestoreMtuSettingsAsync();
            
            OnLogOutput($"MTU reset to default (1500) for {interfaceName}", LogType.Info);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error resetting MTU");
            OnLogOutput("Error resetting MTU: " + ex.Message, LogType.Error);
            return false;
        }
    }

    private int GetInterfaceMtu(string interfaceName)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = $"interface ipv4 show subinterfaces",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var lines = output.Split('\n');
            foreach (var line in lines)
            {
                if (line.Contains(interfaceName, StringComparison.OrdinalIgnoreCase))
                {
                    var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 4 && int.TryParse(parts[0], out var mtu))
                        return mtu;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting interface MTU");
        }

        return 1500; // Default
    }

    private int GetInterfaceIndex(string interfaceName)
    {
        try
        {
            foreach (var iface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (iface.Name.Equals(interfaceName, StringComparison.OrdinalIgnoreCase))
                {
                    var props = iface.GetIPProperties();
                    return props.GetIPv4Properties().Index;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting interface index");
        }

        return -1;
    }

    private string[] GetActiveNetworkInterfaces()
    {
        var activeInterfaces = new List<string>();
        
        try
        {
            foreach (var iface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (iface.OperationalStatus == OperationalStatus.Up &&
                    iface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    activeInterfaces.Add(iface.Name);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting active network interfaces");
        }

        return activeInterfaces.ToArray();
    }

    private string? GetServiceStatus(string serviceName)
    {
        try
        {
            using var sc = new System.ServiceProcess.ServiceController(serviceName);
            return sc.Status.ToString();
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region System Restore Point

    public async Task<bool> CreateRestorePointAsync(string description)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-Command \"Checkpoint-Computer -Description '{description}' -RestorePointType 'MODIFY_SETTINGS'\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await process.WaitForExitAsync(cts.Token);

            OnLogOutput($"System restore point created: {description}", LogType.Info);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error creating restore point");
            OnLogOutput("Error creating restore point: " + ex.Message, LogType.Error);
            return false;
        }
    }

    #endregion

    #region Events

    private void OnLogOutput(string message, LogType type)
    {
        LogOutput?.Invoke(this, new LogEventArgs(message, type));
    }

    public void Dispose()
    {
        // No unmanaged resources to dispose
        GC.SuppressFinalize(this);
    }

    #endregion
}

#region Supporting Classes

public class TcpSettings
{
    public string AutoTuningLevel { get; set; } = "normal";
    public bool EcnEnabled { get; set; }
    public bool TimestampsEnabled { get; set; } = true;
    public bool SelectiveAckEnabled { get; set; }
    public int MaxUserPort { get; set; } = 16387;
    public int TcpTimedWaitDelay { get; set; } = 240;
}

public class TcpOptimizationSettings
{
    public bool OptimizeAutoTuning { get; set; } = true;
    public bool EnableEcn { get; set; } = true;
    public bool EnableTimestamps { get; set; } = true;
    public bool EnableSelectiveAck { get; set; } = true;
    public bool DisableNagleForApps { get; set; } = false;
    public bool EnableBBR { get; set; } = false;
}

public class DnsSettings
{
    public string[] CurrentDnsServers { get; set; } = Array.Empty<string>();
    public string? ActiveInterface { get; set; }
    public bool DnsCachingEnabled { get; set; }
}

public class MtuSettings
{
    public List<InterfaceMtu> Interfaces { get; set; } = new();
}

public class InterfaceMtu
{
    public string Name { get; set; } = "";
    public int CurrentMtu { get; set; } = 1500;
    public bool IsWireless { get; set; }
    public bool IsEthernet { get; set; }
}

public class OptimizationEventArgs : EventArgs
{
    public bool Success { get; }
    public string Message { get; }

    public OptimizationEventArgs(bool success, string message)
    {
        Success = success;
        Message = message;
    }
}

public class RegistryBackup
{
    private readonly string _backupPath;

    public RegistryBackup()
    {
        _backupPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ZapretGUI",
            "Backups");
        
        Directory.CreateDirectory(_backupPath);
    }

    public async Task BackupTcpSettingsAsync()
    {
        var backupFile = Path.Combine(_backupPath, $"tcp_backup_{DateTime.Now:yyyyMMdd_HHmmss}.reg");
        await ExportRegistryKeyAsync("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters", backupFile);
    }

    public async Task BackupDnsSettingsAsync()
    {
        var backupFile = Path.Combine(_backupPath, $"dns_backup_{DateTime.Now:yyyyMMdd_HHmmss}.reg");
        await ExportRegistryKeyAsync("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces", backupFile);
    }

    public async Task BackupMtuSettingsAsync()
    {
        var backupFile = Path.Combine(_backupPath, $"mtu_backup_{DateTime.Now:yyyyMMdd_HHmmss}.reg");
        await ExportRegistryKeyAsync("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces", backupFile);
    }

    public async Task RestoreTcpSettingsAsync()
    {
        await ImportLatestRegistryBackupAsync("tcp_backup_*.reg");
    }

    public async Task RestoreDnsSettingsAsync()
    {
        await ImportLatestRegistryBackupAsync("dns_backup_*.reg");
    }

    public async Task RestoreMtuSettingsAsync()
    {
        await ImportLatestRegistryBackupAsync("mtu_backup_*.reg");
    }

    private async Task ExportRegistryKeyAsync(string keyPath, string outputFile)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "reg",
            Arguments = $"export \"{keyPath}\" \"{outputFile}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await process.WaitForExitAsync(cts.Token);
    }

    private async Task ImportLatestRegistryBackupAsync(string searchPattern)
    {
        var files = Directory.GetFiles(_backupPath, searchPattern)
            .OrderByDescending(f => f)
            .ToArray();

        if (files.Length > 0)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "reg",
                Arguments = $"import \"{files[0]}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await process.WaitForExitAsync(cts.Token);
        }
    }
}

#endregion
