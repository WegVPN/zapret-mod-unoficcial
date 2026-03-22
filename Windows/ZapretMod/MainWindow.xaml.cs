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
        ServiceBtn.Click += (s, e) => System.Diagnostics.Process.Start("service.bat");
        
        AutoStartCheck.Checked += (s, e) => {
            try { ServiceManager.InstallService(); AppendLog("✓ Автозапуск включен", LogType.Info); }
            catch (Exception ex) { AppendLog($"✗ Ошибка: {ex.Message}", LogType.Error); }
        };
        AutoStartCheck.Unchecked += (s, e) => {
            try { ServiceManager.RemoveService(); AppendLog("✗ Автозапуск выключен", LogType.Warning); }
            catch (Exception ex) { AppendLog($"✗ Ошибка: {ex.Message}", LogType.Error); }
        };
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
                var releases = new[]
                {
                    "https://github.com/bol-van/zapret-win-bundle/releases/latest/download/winws.exe",
                    "https://raw.githubusercontent.com/bol-van/zapret-win-bundle/master/WinDivert64.sys",
                    "https://raw.githubusercontent.com/bol-van/zapret-win-bundle/master/WinDivert64.dll"
                };
                
                foreach (var url in releases)
                {
                    var fileName = Path.GetFileName(url);
                    var destPath = Path.Combine(binPath, fileName);
                    
                    try
                    {
                        var data = await client.GetByteArrayAsync(url);
                        await File.WriteAllBytesAsync(destPath, data);
                        AppendLog($"✓ Скачан {fileName}", LogType.Info);
                    }
                    catch
                    {
                        AppendLog($"⚠ Не удалось скачать {fileName}", LogType.Warning);
                    }
                }
                
                AppendLog("Скачивание завершено. Перезапустите приложение.", LogType.Info);
                MessageBox.Show(
                    "Бинарные файлы скачаны в папку bin\\\n\nТеперь закройте приложение и запустите снова.\n\nЕсли скачивание не удалось - скачайте вручную с:\nhttps://github.com/bol-van/zapret-win-bundle/releases",
                    "ZapretMod",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                AppendLog($"✗ Ошибка скачивания: {ex.Message}", LogType.Error);
                MessageBox.Show(
                    $"Не удалось скачать файлы автоматически.\n\nСкачайте вручную:\n1. https://github.com/bol-van/zapret-win-bundle/releases\n2. Распакуйте в папку bin\\\n\nОшибка: {ex.Message}",
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
                StatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(76, 209, 55));
                ToggleButton.Content = "⏹ ОСТАНОВИТЬ";
                ToggleButton.Background = new SolidColorBrush(Color.FromRgb(255, 59, 48));
                AppendLog($"✓ Стратегия '{e.StrategyName}' запущена", LogType.Info);
            }
            else
            {
                StatusText.Text = "Остановлено";
                StatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(255, 59, 48));
                ToggleButton.Content = "▶ ЗАПУСТИТЬ";
                ToggleButton.Background = new SolidColorBrush(Color.FromRgb(0, 120, 215));
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
            LogType.Error => "#FF3B30",
            LogType.Warning => "#FFA500",
            LogType.Info => "#00FF00",
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
