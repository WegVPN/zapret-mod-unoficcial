using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using ZapretMod.Core;

namespace ZapretMod;

public partial class DiagnosticsWindow : Window
{
    public DiagnosticsWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Title = "Диагностика - ZapretMod";
        Width = 700;
        Height = 550;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        var bgColor = new SolidColorBrush(Color.FromRgb(32, 32, 32));
        var panelColor = new SolidColorBrush(Color.FromRgb(45, 45, 45));
        var accentColor = new SolidColorBrush(Color.FromRgb(0, 120, 215));
        var textColor = new SolidColorBrush(Colors.White);
        var successColor = new SolidColorBrush(Color.FromRgb(76, 209, 55));
        var errorColor = new SolidColorBrush(Color.FromRgb(255, 59, 48));

        var mainGrid = new Grid { Background = bgColor, Margin = new Thickness(20) };

        var rows = new[] {
            new RowDefinition { Height = GridLength.Auto },
            new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            new RowDefinition { Height = GridLength.Auto }
        };

        foreach (var row in rows)
            mainGrid.RowDefinitions.Add(row);

        // Title
        var title = new TextBlock
        {
            Text = "🔍 Диагностика",
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            Foreground = accentColor,
            Margin = new Thickness(0, 0, 0, 15)
        };
        Grid.SetRow(title, 0);
        mainGrid.Children.Add(title);

        // Diagnostics panel
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        var diagnosticsPanel = new StackPanel();

        // Check 1: Secure DNS
        diagnosticsPanel.Children.Add(CreateDiagnosticItem(
            "Secure DNS (DoH)",
            "Требуется для работы некоторых стратегий",
            ServiceManager.IsSecureDnsEnabled(),
            "Включить",
            () => ServiceManager.EnableSecureDns()));

        // Check 2: Service installed
        diagnosticsPanel.Children.Add(CreateDiagnosticItem(
            "Служба Windows",
            "Автозапуск zapret при старте системы",
            ServiceManager.IsServiceInstalled(),
            ServiceManager.IsServiceInstalled() ? "Удалить" : "Установить",
            () =>
            {
                if (ServiceManager.IsServiceInstalled())
                    ServiceManager.RemoveService();
                else
                    ServiceManager.InstallService();
            }));

        // Check 3: Binaries
        var engine = new ZapretEngine();
        var binariesOk = engine.CheckBinaries();
        diagnosticsPanel.Children.Add(CreateDiagnosticItem(
            "Бинарные файлы (winws.exe)",
            "Файлы из zapret-win-bundle",
            binariesOk,
            "Скачать",
            () =>
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/bol-van/zapret-win-bundle/releases",
                    UseShellExecute = true
                });
            }));

        // Check 4: Admin rights
        var isAdmin = new System.Security.Principal.WindowsPrincipal(
            System.Security.Principal.WindowsIdentity.GetCurrent())
            .IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        diagnosticsPanel.Children.Add(CreateDiagnosticItem(
            "Права администратора",
            "Требуются для установки службы и изменения сети",
            isAdmin,
            null,
            null));

        // Check 5: WinDivert driver
        var windivertPath = Path.Combine(AppContext.BaseDirectory, "bin", "WinDivert64.sys");
        var windivertExists = File.Exists(windivertPath);
        diagnosticsPanel.Children.Add(CreateDiagnosticItem(
            "WinDivert драйвер",
            "Необходим для перехвата трафика",
            windivertExists,
            null,
            null));

        scrollViewer.Content = diagnosticsPanel;
        Grid.SetRow(scrollViewer, 1);
        mainGrid.Children.Add(scrollViewer);

        // Buttons
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var refreshBtn = new Button
        {
            Content = "🔄 Обновить",
            Width = 100,
            Height = 32,
            Background = panelColor,
            Foreground = textColor,
            Margin = new Thickness(0, 0, 10, 0)
        };
        refreshBtn.Click += (s, e) =>
        {
            // Refresh diagnostics
            Close();
            new DiagnosticsWindow().ShowDialog();
        };

        var closeBtn = new Button
        {
            Content = "Закрыть",
            Width = 100,
            Height = 32,
            Background = accentColor,
            Foreground = textColor
        };
        closeBtn.Click += (s, e) => Close();

        buttonPanel.Children.Add(refreshBtn);
        buttonPanel.Children.Add(closeBtn);
        Grid.SetRow(buttonPanel, 2);
        mainGrid.Children.Add(buttonPanel);

        Content = mainGrid;
    }

    private Border CreateDiagnosticItem(string title, string description, bool isOk, 
        string? buttonText, Action? buttonAction)
    {
        var panel = new StackPanel { Margin = new Thickness(0, 10, 0, 10) };

        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        headerPanel.Children.Add(new TextBlock
        {
            Text = isOk ? "✓" : "✗",
            FontSize = 18,
            Foreground = isOk ? new SolidColorBrush(Color.FromRgb(76, 209, 55)) 
                              : new SolidColorBrush(Color.FromRgb(255, 59, 48)),
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Center
        });

        headerPanel.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center
        });

        if (!string.IsNullOrEmpty(buttonText))
        {
            var btn = new Button
            {
                Content = buttonText,
                Width = 90,
                Height = 28,
                Margin = new Thickness(20, 0, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(0, 120, 215)),
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
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
            Margin = new Thickness(28, 5, 0, 0)
        });

        var border = new Border
        {
            Child = panel,
            Padding = new Thickness(15, 10, 15, 10),
            Background = new SolidColorBrush(Color.FromRgb(45, 45, 45)),
            CornerRadius = new CornerRadius(6),
            Margin = new Thickness(0, 5, 0, 5)
        };

        return border;
    }
}
