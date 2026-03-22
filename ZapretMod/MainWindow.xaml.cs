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
        CheckAndDownloadBinaries();
        
        Log.Information("ZapretMod GUI started");
        AppendLog("=== ZapretMod запущен ===", LogType.Info);
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
        
        // Event handlers
        ClearLogBtn.Click += (s, e) => LogBox.Clear();
        SaveLogBtn.Click += SaveLogBtn_Click;
        SettingsBtn.Click += (s, e) => new SettingsWindow().ShowDialog();
        DiagnosticsBtn.Click += (s, e) => new DiagnosticsWindow().ShowDialog();
        ServiceBtn.Click += (s, e) => OpenServiceManager();
        
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
        else
        {
            MessageBox.Show("Файл service.bat не найден", "ZapretMod", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void CheckAndDownloadBinaries()
    {
        var binPath = Path.Combine(AppContext.BaseDirectory, "bin");
        var winwsPath = Path.Combine(binPath, "winws.exe");
        
        if (!File.Exists(winwsPath))
        {
            AppendLog("⚠ winws.exe не найден. Скачивание...", LogType.Warning);
            
            try
            {
                Directory.CreateDirectory(binPath);
                
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(30);
                
                var files = new Dictionary<string, string>
                {
                    { "winws.exe", "https://github.com/bol-van/zapret-win-bundle/releases/latest/download/winws.exe" },
                    { "WinDivert64.sys", "https://github.com/bol-van/zapret-win-bundle/releases/latest/download/WinDivert64.sys" },
                    { "WinDivert64.dll", "https://github.com/bol-van/zapret-win-bundle/releases/latest/download/WinDivert64.dll" }
                };
                
                foreach (var file in files)
                {
                    try
                    {
                        var data = await client.GetByteArrayAsync(file.Value);
                        await File.WriteAllBytesAsync(Path.Combine(binPath, file.Key), data);
                        AppendLog($"✓ Скачан {file.Key}", LogType.Info);
                    }
                    catch (Exception ex)
                    {
                        AppendLog($"⚠ Не удалось скачать {file.Key}: {ex.Message}", LogType.Warning);
                    }
                }
                
                AppendLog("Скачивание завершено!", LogType.Info);
                MessageBox.Show(
                    "Бинарные файлы успешно скачаны!\n\nТеперь можно запустить защиту.",
                    "ZapretMod",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                AppendLog($"✗ Ошибка скачивания: {ex.Message}", LogType.Error);
                MessageBox.Show(
                    $"Не удалось скачать файлы автоматически.\n\nСкачайте вручную:\nhttps://github.com/bol-van/zapret-win-bundle/releases\n\nОшибка: {ex.Message}",
                    "ZapretMod",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
        else
        {
            AppendLog("✓ winws.exe найден", LogType.Info);
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
                StatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(16, 124, 16));
                ToggleButton.Content = "⏹ ОСТАНОВИТЬ";
                ToggleButton.Background = new SolidColorBrush(Color.FromRgb(209, 52, 56));
                AppendLog($"✓ Стратегия '{e.StrategyName}' запущена", LogType.Info);
            }
            else
            {
                StatusText.Text = "Остановлено";
                StatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(209, 52, 56));
                ToggleButton.Content = "▶ ЗАПУСТИТЬ";
                ToggleButton.Background = new SolidColorBrush(Color.FromRgb(0, 120, 212));
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
