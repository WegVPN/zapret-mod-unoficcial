using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ZapretMod.Core;
using Serilog;

namespace ZapretMod;

public partial class MainWindow : Window
{
    private readonly ZapretEngine _zapretEngine;
    private bool _isRunning;

    public MainWindow()
    {
        InitializeComponent();
        
        _zapretEngine = new ZapretEngine();
        _zapretEngine.LogOutput += ZapretEngine_LogOutput;
        _zapretEngine.StateChanged += ZapretEngine_StateChanged;
        
        LoadStrategies();
        CheckBinaries();
        
        Log.Information("ZapretMod GUI started");
        AppendLog("═══ ZapretMod запущен ═══", LogType.Info);
    }

    private void LoadStrategies()
    {
        var strategies = ZapretEngine.GetBuiltInStrategies();
        foreach (var strategy in strategies)
        {
            StrategyCombo.Items.Add(strategy);
        }
        StrategyCombo.SelectedIndex = 0;
        StrategyCombo.SelectionChanged += StrategyCombo_SelectionChanged;
        UpdateStrategyDescription();
        
        ClearLogBtn.Click += (s, e) => LogBox.Clear();
        SaveLogBtn.Click += SaveLogBtn_Click;
        SettingsBtn.Click += (s, e) => new SettingsWindow().ShowDialog();
        DiagnosticsBtn.Click += (s, e) => new DiagnosticsWindow().ShowDialog();
        ServiceBtn.Click += (s, e) => OpenServiceManager();
        DownloadBinariesBtn.Click += async (s, e) => await DownloadBinariesAsync();
        
        AutoStartCheck.Checked += (s, e) => {
            try { ServiceManager.InstallService(); AppendLog("✓ Автозапуск включен", LogType.Info); }
            catch (Exception ex) { AppendLog($"✗ Ошибка: {ex.Message}", LogType.Error); }
        };
        AutoStartCheck.Unchecked += (s, e) => {
            try { ServiceManager.RemoveService(); AppendLog("✗ Автозапуск выключен", LogType.Warning); }
            catch (Exception ex) { AppendLog($"✗ Ошибка: {ex.Message}", LogType.Error); }
        };
    }

    private void OpenServiceManager()
    {
        var servicePath = Path.Combine(AppContext.BaseDirectory, "service.bat");
        if (File.Exists(servicePath))
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = servicePath,
                UseShellExecute = true,
                Verb = "runas"
            });
        }
    }

    private void CheckBinaries()
    {
        var engine = new ZapretEngine();
        if (engine.CheckBinaries())
        {
            AppendLog("✓ Все файлы найдены", LogType.Info);
        }
        else
        {
            AppendLog("⚠ Файлы не найдены! Нажмите '📥 Скачать файлы'", LogType.Warning);
        }
    }

    private async System.Threading.Tasks.Task DownloadBinariesAsync()
    {
        AppendLog("Начинаю скачивание файлов...", LogType.Info);
        
        try
        {
            var binPath = Path.Combine(AppContext.BaseDirectory, "bin");
            Directory.CreateDirectory(binPath);
            
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(60);
            
            // Try multiple sources
            var sources = new[]
            {
                "https://raw.githubusercontent.com/bol-van/zapret-win-bundle/master/",
                "https://github.com/bol-van/zapret-win-bundle/releases/latest/download/"
            };
            
            var files = new[] { "winws.exe", "WinDivert64.sys", "WinDivert64.dll" };
            bool allDownloaded = true;
            
            foreach (var fileName in files)
            {
                bool downloaded = false;
                foreach (var source in sources)
                {
                    try
                    {
                        var url = source + fileName;
                        var data = await client.GetByteArrayAsync(url);
                        
                        if (data.Length > 100) // Validate file size
                        {
                            await File.WriteAllBytesAsync(Path.Combine(binPath, fileName), data);
                            AppendLog($"✓ Скачан {fileName} ({data.Length / 1024} KB)", LogType.Info);
                            downloaded = true;
                            break;
                        }
                    }
                    catch { }
                }
                
                if (!downloaded)
                {
                    AppendLog($"✗ Не удалось скачать {fileName}", LogType.Error);
                    allDownloaded = false;
                }
            }
            
            if (allDownloaded)
            {
                AppendLog("═══ Все файлы успешно скачаны! ═══", LogType.Info);
                MessageBox.Show("✓ Все файлы успешно скачаны!\n\nТеперь можно запустить защиту.", 
                    "ZapretMod", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("⚠ Не все файлы удалось скачать.\n\nПопробуйте скачать вручную:\nhttps://github.com/bol-van/zapret-win-bundle/releases", 
                    "ZapretMod", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            AppendLog($"✗ Ошибка: {ex.Message}", LogType.Error);
            MessageBox.Show($"Ошибка скачивания: {ex.Message}", "ZapretMod", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UpdateStrategyDescription()
    {
        if (StrategyCombo.SelectedItem is StrategyConfig config)
        {
            StrategyDescription.Text = config.Description;
        }
    }

    private void StrategyCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdateStrategyDescription();
    }

    private void ToggleButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (_isRunning)
            {
                _zapretEngine.Stop();
            }
            else
            {
                if (StrategyCombo.SelectedItem is StrategyConfig config)
                {
                    _zapretEngine.Start(config);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "ZapretMod", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            AppendLog($"✗ Ошибка: {ex.Message}", LogType.Error);
        }
    }

    private void ZapretEngine_StateChanged(object? sender, StateChangedEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            _isRunning = e.IsRunning;
            
            if (e.IsRunning)
            {
                StatusText.Text = "Работает";
                StatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(36, 129, 34));
                ToggleButton.Content = "⏹ ОСТАНОВИТЬ";
                ToggleButton.Background = new SolidColorBrush(Color.FromRgb(218, 55, 60));
                AppendLog($"✓ Стратегия '{e.StrategyName}' запущена", LogType.Info);
            }
            else
            {
                StatusText.Text = "Остановлено";
                StatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(218, 55, 60));
                ToggleButton.Content = "▶ ЗАПУСТИТЬ";
                ToggleButton.Background = new SolidColorBrush(Color.FromRgb(88, 101, 242));
                AppendLog("✗ Остановлено", LogType.Warning);
            }
        });
    }

    private void ZapretEngine_LogOutput(object? sender, LogEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            AppendLog($"[{timestamp}] {e.Message}", e.Type);
        });
    }

    private void AppendLog(string message, LogType type)
    {
        var color = type switch
        {
            LogType.Error => "#FF5555",
            LogType.Warning => "#FFA61A",
            LogType.Info => "#00FF88",
            LogType.Debug => "#888888",
            _ => "#AAAAAA"
        };
        
        LogBox.AppendText($"\n{message}");
        LogBox.ScrollToEnd();
    }

    private void SaveLogBtn_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Text Files|*.txt|All Files|*.*",
            FileName = $"ZapretMod_Log_{DateTime.Now:yyyyMMdd_HHmmss}"
        };

        if (dialog.ShowDialog() == true)
        {
            File.WriteAllText(dialog.FileName, LogBox.Text);
            AppendLog($"✓ Лог сохранён: {dialog.FileName}", LogType.Info);
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (_isRunning)
        {
            var result = MessageBox.Show(
                "Zapret работает. Остановить и выйти?",
                "ZapretMod",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                _zapretEngine.Stop();
            }
            else
            {
                e.Cancel = true;
                return;
            }
        }
        
        _zapretEngine.Dispose();
        base.OnClosing(e);
    }
}
