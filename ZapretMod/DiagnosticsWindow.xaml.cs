using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ZapretMod.Core;

namespace ZapretMod;

public class DiagnosticsWindow : Window
{
    public DiagnosticsWindow()
    {
        Title = "Диагностика - ZapretMod";
        Width = 700;
        Height = 600;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = (Brush)new BrushConverter().ConvertFrom("#202020");

        var mainGrid = new Grid { Margin = new Thickness(25) };
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var title = new TextBlock
        {
            Text = "🔍 Диагностика",
            FontSize = 22,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White,
            Margin = new Thickness(0, 0, 0, 20)
        };
        Grid.SetRow(title, 0);
        mainGrid.Children.Add(title);

        var scrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var diagnosticsPanel = new StackPanel();

        var binariesOk = CheckBinariesDetailed(out var binariesMsg);
        diagnosticsPanel.Children.Add(CreateDiagnosticItem(
            "📁 Бинарные файлы",
            binariesMsg,
            binariesOk,
            "Скачать",
            () => Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/bol-van/zapret-win-bundle/releases",
                UseShellExecute = true
            })));

        var isAdmin = new System.Security.Principal.WindowsPrincipal(
            System.Security.Principal.WindowsIdentity.GetCurrent())
            .IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        diagnosticsPanel.Children.Add(CreateDiagnosticItem(
            "🔐 Права администратора",
            isAdmin ? "Приложение запущено от имени администратора" : "Требуется запуск от администратора",
            isAdmin,
            null, null));

        var serviceInstalled = ServiceManager.IsServiceInstalled();
        diagnosticsPanel.Children.Add(CreateDiagnosticItem(
            "🔧 Служба Windows",
            serviceInstalled ? "Служба ZapretMod установлена" : "Служба не установлена",
            serviceInstalled,
            serviceInstalled ? "Удалить" : "Установить",
            () =>
            {
                if (serviceInstalled)
                    ServiceManager.RemoveService();
                else
                    ServiceManager.InstallService();
                RefreshDiagnostics();
            }));

        var dnsOk = ServiceManager.IsSecureDnsEnabled();
        diagnosticsPanel.Children.Add(CreateDiagnosticItem(
            "🌐 Secure DNS (DoH)",
            dnsOk ? "Secure DNS включён" : "Рекомендуется включить Secure DNS",
            dnsOk,
            dnsOk ? null : "Включить",
            dnsOk ? null : () => ServiceManager.EnableSecureDns()));

        var windivertPath = Path.Combine(AppContext.BaseDirectory, "bin", "WinDivert64.sys");
        var windivertExists = File.Exists(windivertPath);
        diagnosticsPanel.Children.Add(CreateDiagnosticItem(
            "🔌 WinDivert драйвер",
            windivertExists ? "Драйвер найден" : "Драйвер не найден (скачается автоматически)",
            windivertExists,
            null, null));

        scrollViewer.Content = diagnosticsPanel;
        Grid.SetRow(scrollViewer, 1);
        mainGrid.Children.Add(scrollViewer);

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var refreshBtn = new Button
        {
            Content = "🔄 Обновить",
            Width = 110,
            Height = 36,
            Background = (Brush)new BrushConverter().ConvertFrom("#2D2D30"),
            Foreground = Brushes.White,
            Margin = new Thickness(0, 0, 10, 0)
        };
        refreshBtn.Click += (s, e) => RefreshDiagnostics();

        var closeBtn = new Button
        {
            Content = "Закрыть",
            Width = 110,
            Height = 36,
            Background = (Brush)new BrushConverter().ConvertFrom("#0078D4"),
            Foreground = Brushes.White
        };
        closeBtn.Click += (s, e) => Close();

        buttonPanel.Children.Add(refreshBtn);
        buttonPanel.Children.Add(closeBtn);
        Grid.SetRow(buttonPanel, 2);
        mainGrid.Children.Add(buttonPanel);

        Content = mainGrid;
    }

    private bool CheckBinariesDetailed(out string message)
    {
        var binPath = Path.Combine(AppContext.BaseDirectory, "bin");
        var winws = Path.Combine(binPath, "winws.exe");
        var windivertSys = Path.Combine(binPath, "WinDivert64.sys");
        var windivertDll = Path.Combine(binPath, "WinDivert64.dll");

        var missing = new List<string>();
        if (!File.Exists(winws)) missing.Add("winws.exe");
        if (!File.Exists(windivertSys)) missing.Add("WinDivert64.sys");
        if (!File.Exists(windivertDll)) missing.Add("WinDivert64.dll");

        if (missing.Count == 0)
        {
            message = "Все файлы найдены в папке bin\\";
            return true;
        }
        else
        {
            message = $"Отсутствуют: {string.Join(", ", missing)}";
            return false;
        }
    }

    private void RefreshDiagnostics()
    {
        Close();
        new DiagnosticsWindow().ShowDialog();
    }

    private Border CreateDiagnosticItem(string title, string description, bool isOk, 
        string? buttonText, Action? buttonAction)
    {
        var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };

        var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };

        headerPanel.Children.Add(new TextBlock
        {
            Text = isOk ? "✓" : "✗",
            FontSize = 20,
            Foreground = isOk ? (Brush)new BrushConverter().ConvertFrom("#107C10") 
                              : (Brush)new BrushConverter().ConvertFrom("#D13438"),
            Margin = new Thickness(0, 0, 12, 0),
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeights.Bold
        });

        headerPanel.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 15,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center
        });

        if (!string.IsNullOrEmpty(buttonText))
        {
            var btn = new Button
            {
                Content = buttonText,
                Width = 110,
                Height = 32,
                Margin = new Thickness(20, 0, 0, 0),
                Background = (Brush)new BrushConverter().ConvertFrom("#0078D4"),
                Foreground = Brushes.White
            };
            btn.Click += (s, e) =>
            {
                try
                {
                    buttonAction?.Invoke();
                    MessageBox.Show("Действие выполнено", "Диагностика", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Диагностика", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
            headerPanel.Children.Add(btn);
        }

        panel.Children.Add(headerPanel);
        panel.Children.Add(new TextBlock
        {
            Text = description,
            FontSize = 13,
            Foreground = (Brush)new BrushConverter().ConvertFrom("#B0B0B0"),
            Margin = new Thickness(32, 6, 0, 0),
            TextWrapping = TextWrapping.Wrap
        });

        var border = new Border
        {
            Child = panel,
            Padding = new Thickness(18, 14, 18, 14),
            Background = (Brush)new BrushConverter().ConvertFrom("#2D2D30"),
            CornerRadius = new CornerRadius(6),
            Margin = new Thickness(0, 6, 0, 6)
        };

        return border;
    }
}
