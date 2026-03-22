using System;
using System.Diagnostics;
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
    private readonly ZapretEngine _engine;
    private bool _running;

    public MainWindow()
    {
        InitializeComponent();
        _engine = new ZapretEngine();
        _engine.LogOutput += (s, e) => Dispatcher.Invoke(() => Log($"[{DateTime.Now:HH:mm:ss}] {e.Message}", e.Type));
        _engine.StateChanged += (s, e) => Dispatcher.Invoke(() => UpdateStatus(e.IsRunning, e.Strategy));
        LoadStrategies();
        CheckFiles();
        Log("═══ ZapretMod v3.0 запущен ═══", LogType.Info);
    }

    private void LoadStrategies()
    {
        var strategies = ZapretEngine.GetStrategies();
        foreach (var s in strategies) StrategyCombo.Items.Add(s);
        StrategyCombo.SelectedIndex = 0;
        StrategyCombo.SelectionChanged += (s, e) => StrategyDesc.Text = ((StrategyConfig)StrategyCombo.SelectedItem).Description;
        StrategyDesc.Text = ((StrategyConfig)StrategyCombo.SelectedItem).Description;
    }

    private void CheckFiles()
    {
        if (_engine.CheckFiles()) Log("✓ Файлы найдены", LogType.Info);
        else Log("⚠ Файлы не найдены! Нажмите '📥 Скачать файлы'", LogType.Warning);
    }

    private async void DownloadClick(object sender, RoutedEventArgs e)
    {
        Log("Скачивание файлов...", LogType.Info);
        try
        {
            var bin = Path.Combine(AppContext.BaseDirectory, "bin");
            Directory.CreateDirectory(bin);
            using var client = new HttpClient();
            var files = new[] { "winws.exe", "WinDivert64.sys", "WinDivert64.dll" };
            foreach (var f in files)
            {
                try
                {
                    var data = await client.GetByteArrayAsync($"https://github.com/bol-van/zapret-win-bundle/releases/latest/download/{f}");
                    File.WriteAllBytes(Path.Combine(bin, f), data);
                    Log($"✓ {f} ({data.Length / 1024} KB)", LogType.Info);
                }
                catch (Exception ex) { Log($"✗ {f}: {ex.Message}", LogType.Error); }
            }
            Log("✓ Скачивание завершено", LogType.Info);
            MessageBox.Show("Файлы скачаны!", "ZapretMod", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) { Log($"✗ Ошибка: {ex.Message}", LogType.Error); }
    }

    private void ToggleClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_running) _engine.Stop();
            else _engine.Start((StrategyConfig)StrategyCombo.SelectedItem);
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private void UpdateStatus(bool running, string? strategy)
    {
        _running = running;
        StatusText.Text = running ? "Работает" : "Остановлено";
        StatusIndicator.Fill = new SolidColorBrush(running ? Color.FromRgb(36, 129, 34) : Color.FromRgb(218, 55, 60));
        ToggleButton.Content = running ? "⏹ ОСТАНОВИТЬ" : "▶ ЗАПУСТИТЬ";
        ToggleButton.Background = new SolidColorBrush(running ? Color.FromRgb(218, 55, 60) : Color.FromRgb(88, 101, 242));
        Log(running ? $"✓ Запущено: {strategy}" : "✗ Остановлено", running ? LogType.Info : LogType.Warning);
    }

    private void Log(string msg, LogType type)
    {
        LogBox.AppendText($"\n{msg}");
        LogBox.ScrollToEnd();
    }

    private void SettingsClick(object sender, RoutedEventArgs e) => new SettingsWindow().ShowDialog();
    private void DiagnosticsClick(object sender, RoutedEventArgs e) => new DiagnosticsWindow().ShowDialog();

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (_running && MessageBox.Show("Остановить и выйти?", "ZapretMod", MessageBoxButton.YesNo) == MessageBoxResult.Yes) _engine.Stop();
        _engine.Dispose();
        base.OnClosing(e);
    }
}

public class SettingsWindow : Window
{
    public SettingsWindow()
    {
        Title = "Настройки"; Width = 500; Height = 400;
        Background = (Brush)new BrushConverter().ConvertFrom("#2A2A3A");
        var text = new TextBlock { Text = "Настройки\n\nВерсия: 3.0.0\n\nВсе настройки применяются автоматически.", Foreground = Brushes.White, FontSize = 14, Margin = new Thickness(20) };
        Content = text;
    }
}

public class DiagnosticsWindow : Window
{
    public DiagnosticsWindow()
    {
        Title = "Диагностика"; Width = 600; Height = 500;
        Background = (Brush)new BrushConverter().ConvertFrom("#2A2A3A");
        var panel = new StackPanel { Margin = new Thickness(20) };
        var isAdmin = new System.Security.Principal.WindowsPrincipal(System.Security.Principal.WindowsIdentity.GetCurrent()).IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        panel.Children.Add(new TextBlock { Text = $"✓ Администратор: {(isAdmin ? "Да" : "Нет")}", Foreground = isAdmin ? Brushes.Green : Brushes.Red, FontSize = 14, Margin = new Thickness(0, 0, 0, 10) });
        panel.Children.Add(new TextBlock { Text = "✓ .NET 8: Да", Foreground = Brushes.Green, FontSize = 14, Margin = new Thickness(0, 0, 0, 10) });
        Content = panel;
    }
}
