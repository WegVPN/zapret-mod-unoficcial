using Microsoft.Win32;
using Serilog;

namespace ZapretGUI.Core;

/// <summary>
/// Manages application auto-start via Windows Registry Run key
/// </summary>
public class AutoStartManager
{
    private const string RunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "ZapretGUI";
    private readonly string _applicationPath;

    public AutoStartManager()
    {
        _applicationPath = Environment.ProcessPath ?? 
            Path.Combine(AppContext.BaseDirectory, "ZapretGUI.exe");
    }

    public bool IsEnabled
    {
        get
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKey, false);
                return key?.GetValue(ValueName) != null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error checking auto-start status");
                return false;
            }
        }
    }

    public bool Enable()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, true);
            key?.SetValue(ValueName, $"\"{_applicationPath}\"", RegistryValueKind.String);
            Log.Information("Auto-start enabled via registry");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error enabling auto-start");
            return false;
        }
    }

    public bool Disable()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, true);
            key?.DeleteValue(ValueName, false);
            Log.Information("Auto-start disabled via registry");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error disabling auto-start");
            return false;
        }
    }

    public void Toggle()
    {
        if (IsEnabled)
            Disable();
        else
            Enable();
    }
}
