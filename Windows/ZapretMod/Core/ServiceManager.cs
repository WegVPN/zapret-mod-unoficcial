using System.ServiceProcess;
using Microsoft.Win32;
using Serilog;

namespace ZapretMod.Core;

/// <summary>
/// Manages Windows service installation (similar to flowseal service.bat)
/// </summary>
public static class ServiceManager
{
    private const string ServiceName = "ZapretMod";
    private const string ServiceDisplayName = "ZapretMod DPI Bypass Service";
    private const string ServiceDescription = "Automatically starts zapret DPI bypass on system startup";

    public static bool IsServiceInstalled()
    {
        try
        {
            using var sc = new ServiceController(ServiceName);
            return sc != null;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsServiceRunning()
    {
        try
        {
            using var sc = new ServiceController(ServiceName);
            return sc.Status == ServiceControllerStatus.Running;
        }
        catch
        {
            return false;
        }
    }

    public static void InstallService()
    {
        try
        {
            var exePath = Environment.ProcessPath ?? 
                Path.Combine(AppContext.BaseDirectory, "ZapretMod.Service.exe");

            // Use sc.exe to install service
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = $"create \"{ServiceName}\" binPath= \"{exePath}\" start= auto DisplayName= \"{ServiceDisplayName}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = new System.Diagnostics.Process { StartInfo = startInfo };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Set service description
            startInfo.Arguments = $"description \"{ServiceName}\" \"{ServiceDescription}\"";
            startInfo.RedirectStandardOutput = true;
            
            using var descProcess = new System.Diagnostics.Process { StartInfo = startInfo };
            descProcess.Start();
            descProcess.WaitForExit();

            // Start the service
            StartService();

            Log.Information("Service installed successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to install service");
            throw;
        }
    }

    public static void RemoveService()
    {
        try
        {
            StopService();

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = $"delete \"{ServiceName}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new System.Diagnostics.Process { StartInfo = startInfo };
            process.Start();
            process.WaitForExit();

            Log.Information("Service removed successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to remove service");
        }
    }

    public static void StartService()
    {
        try
        {
            using var sc = new ServiceController(ServiceName);
            if (sc.Status != ServiceControllerStatus.Running)
            {
                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
            }
            Log.Information("Service started");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to start service");
        }
    }

    public static void StopService()
    {
        try
        {
            using var sc = new ServiceController(ServiceName);
            if (sc.Status == ServiceControllerStatus.Running)
            {
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
            }
            Log.Information("Service stopped");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to stop service");
        }
    }

    public static void ToggleService()
    {
        if (IsServiceRunning())
            StopService();
        else
            StartService();
    }

    /// <summary>
    /// Check if Secure DNS is enabled (requirement from flowseal/zapret-discord-youtube)
    /// </summary>
    public static bool IsSecureDnsEnabled()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Services\Dnscache\Parameters");
            
            if (key != null)
            {
                var enableDoh = key.GetValue("EnableAutoDoh");
                return enableDoh != null && Convert.ToInt32(enableDoh) == 2;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error checking Secure DNS");
        }
        
        return false;
    }

    /// <summary>
    /// Enable Secure DNS (DoH)
    /// </summary>
    public static void EnableSecureDns()
    {
        try
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "reg.exe",
                Arguments = @"add HKLM\SYSTEM\CurrentControlSet\Services\Dnscache\Parameters /v EnableAutoDoh /t REG_DWORD /d 2 /f",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new System.Diagnostics.Process { StartInfo = startInfo };
            process.Start();
            process.WaitForExit();

            Log.Information("Secure DNS enabled");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to enable Secure DNS");
        }
    }
}
