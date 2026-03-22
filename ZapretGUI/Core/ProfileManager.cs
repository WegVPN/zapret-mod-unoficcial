using Newtonsoft.Json;
using Serilog;

namespace ZapretGUI.Core;

/// <summary>
/// Manages configuration profiles for zapret
/// </summary>
public class ProfileManager
{
    private readonly string _profilesPath;
    private readonly string _settingsPath;
    private readonly Dictionary<string, ZapretProfile> _profiles;

    public event EventHandler<ProfileChangedEventArgs>? ProfileChanged;

    public IReadOnlyCollection<ZapretProfile> Profiles => _profiles.Values;
    public ZapretProfile? ActiveProfile { get; private set; }
    public AppSettings Settings { get; private set; }

    public ProfileManager()
    {
        _profilesPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ZapretGUI",
            "Profiles");

        _settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ZapretGUI",
            "settings.json");

        Directory.CreateDirectory(_profilesPath);
        
        _profiles = new Dictionary<string, ZapretProfile>(StringComparer.OrdinalIgnoreCase);
        Settings = new AppSettings();

        LoadProfiles();
        LoadSettings();
    }

    public void LoadProfiles()
    {
        _profiles.Clear();

        // Load built-in profiles
        LoadBuiltInProfiles();

        // Load user profiles
        try
        {
            var profileFiles = Directory.GetFiles(_profilesPath, "*.json");
            foreach (var file in profileFiles)
            {
                var json = File.ReadAllText(file);
                var profile = JsonConvert.DeserializeObject<ZapretProfile>(json);
                if (profile != null && !profile.IsBuiltIn)
                {
                    _profiles[profile.Name] = profile;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading user profiles");
        }

        Log.Information("Loaded {Count} profiles", _profiles.Count);
    }

    private void LoadBuiltInProfiles()
    {
        var builtInProfiles = new[]
        {
            new ZapretProfile
            {
                Name = "Discord",
                IsBuiltIn = true,
                Config = new ZapretConfig
                {
                    ProfileName = "Discord",
                    Preset = ZapretPreset.Discord,
                    Method = "fake",
                    Domains = "discord.com,discord.gg,discordapp.com,discord.media",
                    Ports = "443,80",
                    AdditionalArgs = "--dpi-desync=fake --dpi-desync-autottls=1"
                }
            },
            new ZapretProfile
            {
                Name = "YouTube",
                IsBuiltIn = true,
                Config = new ZapretConfig
                {
                    ProfileName = "YouTube",
                    Preset = ZapretPreset.YouTube,
                    Method = "fake",
                    Domains = "youtube.com,ytimg.com,googlevideo.com,youtube-nocookie.com",
                    Ports = "443,80",
                    AdditionalArgs = "--dpi-desync=fake --dpi-desync-autottls=1 --host-mismatch=1"
                }
            },
            new ZapretProfile
            {
                Name = "Telegram",
                IsBuiltIn = true,
                Config = new ZapretConfig
                {
                    ProfileName = "Telegram",
                    Preset = ZapretPreset.Telegram,
                    Method = "fake",
                    Domains = "telegram.org,telegram.me,t.me",
                    Ips = "149.154.160.0/20,91.108.4.0/22",
                    Ports = "443,80,8443",
                    AdditionalArgs = "--dpi-desync=fake"
                }
            },
            new ZapretProfile
            {
                Name = "All Services",
                IsBuiltIn = true,
                Config = new ZapretConfig
                {
                    ProfileName = "All Services",
                    Preset = ZapretPreset.All,
                    Method = "fake",
                    Domains = "discord.com,discord.gg,youtube.com,ytimg.com,googlevideo.com,telegram.org,t.me",
                    Ports = "443,80,8443",
                    AdditionalArgs = "--dpi-desync=fake --dpi-desync-autottls=1"
                }
            },
            new ZapretProfile
            {
                Name = "Custom",
                IsBuiltIn = true,
                Config = new ZapretConfig
                {
                    ProfileName = "Custom",
                    Preset = ZapretPreset.Custom,
                    Method = "fake"
                }
            }
        };

        foreach (var profile in builtInProfiles)
        {
            _profiles[profile.Name] = profile;
        }
    }

    public void LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                Settings = JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
                Log.Information("Settings loaded");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading settings");
            Settings = new AppSettings();
        }
    }

    public void SaveSettings()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
            var json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
            File.WriteAllText(_settingsPath, json);
            Log.Information("Settings saved");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saving settings");
        }
    }

    public ZapretProfile? GetProfile(string name)
    {
        return _profiles.TryGetValue(name, out var profile) ? profile : null;
    }

    public void SetActiveProfile(string name)
    {
        ActiveProfile = GetProfile(name);
        ProfileChanged?.Invoke(this, new ProfileChangedEventArgs(ActiveProfile));
        Log.Information("Active profile set to: {Profile}", name);
    }

    public bool SaveUserProfile(ZapretProfile profile)
    {
        try
        {
            if (profile.IsBuiltIn)
            {
                Log.Warning("Cannot modify built-in profile: {Profile}", profile.Name);
                return false;
            }

            var fileName = Path.Combine(_profilesPath, $"{profile.Name}.json");
            var json = JsonConvert.SerializeObject(profile, Formatting.Indented);
            File.WriteAllText(fileName, json);
            
            _profiles[profile.Name] = profile;
            Log.Information("User profile saved: {Profile}", profile.Name);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saving user profile");
            return false;
        }
    }

    public bool DeleteUserProfile(string name)
    {
        try
        {
            var profile = GetProfile(name);
            if (profile == null || profile.IsBuiltIn)
            {
                Log.Warning("Cannot delete profile: {Profile}", name);
                return false;
            }

            var fileName = Path.Combine(_profilesPath, $"{name}.json");
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            _profiles.Remove(name);
            Log.Information("User profile deleted: {Profile}", name);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting user profile");
            return false;
        }
    }

    public List<string> GetAvailableInterfaces()
    {
        var interfaces = new List<string> { "Auto" };
        
        try
        {
            foreach (var iface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                if (iface.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up &&
                    iface.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
                {
                    interfaces.Add(iface.Name);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting network interfaces");
        }

        return interfaces;
    }

    public void ExportProfiles(string filePath)
    {
        try
        {
            var exportData = new
            {
                Profiles = _profiles.Values.Where(p => !p.IsBuiltIn).ToList(),
                Settings = Settings
            };
            
            var json = JsonConvert.SerializeObject(exportData, Formatting.Indented);
            File.WriteAllText(filePath, json);
            Log.Information("Profiles exported to: {Path}", filePath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error exporting profiles");
        }
    }

    public void ImportProfiles(string filePath)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            var importData = JsonConvert.DeserializeObject<ImportData>(json);
            
            if (importData != null)
            {
                if (importData.Profiles != null)
                {
                    foreach (var profile in importData.Profiles)
                    {
                        profile.IsBuiltIn = false;
                        _profiles[profile.Name] = profile;
                        SaveUserProfile(profile);
                    }
                }

                if (importData.Settings != null)
                {
                    Settings = importData.Settings;
                    SaveSettings();
                }

                Log.Information("Profiles imported from: {Path}", filePath);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error importing profiles");
        }
    }

    private class ImportData
    {
        public List<ZapretProfile>? Profiles { get; set; }
        public AppSettings? Settings { get; set; }
    }
}

/// <summary>
/// Configuration profile for zapret
/// </summary>
public class ZapretProfile
{
    [JsonProperty("name")]
    public string Name { get; set; } = "";

    [JsonProperty("isBuiltIn")]
    public bool IsBuiltIn { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; } = "";

    [JsonProperty("config")]
    public ZapretConfig Config { get; set; } = new();

    [JsonProperty("networkSettings")]
    public NetworkOptimizationSettings NetworkSettings { get; set; } = new();

    public override string ToString() => Name;
}

/// <summary>
/// Network optimization settings for a profile
/// </summary>
public class NetworkOptimizationSettings
{
    [JsonProperty("optimizeTcp")]
    public bool OptimizeTcp { get; set; }

    [JsonProperty("enableBBR")]
    public bool EnableBBR { get; set; }

    [JsonProperty("customDns")]
    public bool CustomDns { get; set; }

    [JsonProperty("dnsServers")]
    public string[] DnsServers { get; set; } = { "1.1.1.1", "1.0.0.1" };

    [JsonProperty("customMtu")]
    public bool CustomMtu { get; set; }

    [JsonProperty("mtuValue")]
    public int MtuValue { get; set; } = 1500;
}

/// <summary>
/// Application-wide settings
/// </summary>
public class AppSettings
{
    [JsonProperty("autoStart")]
    public bool AutoStart { get; set; }

    [JsonProperty("startMinimized")]
    public bool StartMinimized { get; set; }

    [JsonProperty("minimizeToTray")]
    public bool MinimizeToTray { get; set; } = true;

    [JsonProperty("checkUpdates")]
    public bool CheckUpdates { get; set; } = true;

    [JsonProperty("darkTheme")]
    public bool DarkTheme { get; set; }

    [JsonProperty("zapretPath")]
    public string ZapretPath { get; set; } = "";

    [JsonProperty("activeProfile")]
    public string ActiveProfile { get; set; } = "Discord";

    [JsonProperty("language")]
    public string Language { get; set; } = "en";

    [JsonProperty("logLevel")]
    public string LogLevel { get; set; } = "Information";
}

public class ProfileChangedEventArgs : EventArgs
{
    public ZapretProfile? Profile { get; }

    public ProfileChangedEventArgs(ZapretProfile? profile)
    {
        Profile = profile;
    }
}
