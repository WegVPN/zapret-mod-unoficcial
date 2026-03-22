using Microsoft.Win32.TaskScheduler;
using Serilog;

namespace ZapretGUI.Core;

/// <summary>
/// Manages application auto-start via Windows Task Scheduler
/// </summary>
public class AutoStartManager
{
    private const string TaskName = "ZapretGUI";
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
                using var taskService = new TaskService();
                var task = taskService.GetTask(TaskName);
                return task != null;
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
            using var taskService = new TaskService();
            
            // Remove existing task if present
            if (taskService.GetTask(TaskName) != null)
            {
                taskService.RootFolder.DeleteTask(TaskName, false);
            }

            // Create new task
            var taskDefinition = taskService.NewTask();
            
            // General settings
            taskDefinition.RegistrationInfo.Description = "ZapretGUI - Auto-start on login";
            taskDefinition.Settings.AllowStartIfOnBatteries = true;
            taskDefinition.Settings.StopIfGoingOnBatteries = false;
            taskDefinition.Settings.RunOnlyIfIdle = false;
            taskDefinition.Settings.IdleSettings.StopOnIdleEnd = false;
            taskDefinition.Settings.ExecutionTimeLimit = TimeSpan.Zero; // No limit
            taskDefinition.Settings.Priority = System.Diagnostics.ProcessPriorityClass.Normal;

            // Trigger - at logon
            var trigger = new LogonTrigger { Delay = TimeSpan.FromSeconds(5) };
            taskDefinition.Triggers.Add(trigger);

            // Action - run application
            taskDefinition.Actions.Add(new ExecAction(_applicationPath, "", Path.GetDirectoryName(_applicationPath)));

            // Principal - run with highest privileges
            taskDefinition.Principal.RunLevel = TaskRunLevel.Highest;

            // Register task
            taskService.RootFolder.RegisterTaskDefinition(TaskName, taskDefinition);

            Log.Information("Auto-start enabled");
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
            using var taskService = new TaskService();
            var task = taskService.GetTask(TaskName);
            
            if (task != null)
            {
                taskService.RootFolder.DeleteTask(TaskName, false);
                Log.Information("Auto-start disabled");
                return true;
            }

            return false;
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

/// <summary>
/// Manages Windows Registry run key for auto-start (alternative method)
/// </summary>
public class RegistryAutoStart
{
    private const string RunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "ZapretGUI";

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
                Log.Error(ex, "Error checking registry auto-start");
                return false;
            }
        }
    }

    public bool Enable()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, true);
            var appPath = Environment.ProcessPath ?? 
                Path.Combine(AppContext.BaseDirectory, "ZapretGUI.exe");
            
            key?.SetValue(ValueName, $"\"{appPath}\"", RegistryValueKind.String);
            Log.Information("Registry auto-start enabled");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error enabling registry auto-start");
            return false;
        }
    }

    public bool Disable()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, true);
            key?.DeleteValue(ValueName, false);
            Log.Information("Registry auto-start disabled");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error disabling registry auto-start");
            return false;
        }
    }
}
