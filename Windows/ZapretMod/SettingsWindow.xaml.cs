using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ZapretMod.Core;

namespace ZapretMod;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Title = "Настройки - ZapretMod";
        Width = 600;
        Height = 500;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;

        var bgColor = new SolidColorBrush(Color.FromRgb(32, 32, 32));
        var panelColor = new SolidColorBrush(Color.FromRgb(45, 45, 45));
        var accentColor = new SolidColorBrush(Color.FromRgb(0, 120, 215));
        var textColor = new SolidColorBrush(Colors.White);

        var mainGrid = new Grid { Background = bgColor, Margin = new Thickness(20) };

        var rows = new[] {
            new RowDefinition { Height = GridLength.Auto },
            new RowDefinition { Height = GridLength.Auto },
            new RowDefinition { Height = GridLength.Auto },
            new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            new RowDefinition { Height = GridLength.Auto }
        };

        foreach (var row in rows)
            mainGrid.RowDefinitions.Add(row);

        // Title
        var title = new TextBlock
        {
            Text = "⚙ Настройки",
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            Foreground = accentColor,
            Margin = new Thickness(0, 0, 0, 20)
        };
        Grid.SetRow(title, 0);
        mainGrid.Children.Add(title);

        // Zapret path
        var pathPanel = CreateSettingRow(
            "Путь к winws.exe:",
            "Укажите расположение winws.exe из zapret-win-bundle",
            1);
        Grid.SetRow(pathPanel, 1);
        mainGrid.Children.Add(pathPanel);

        // Binaries check
        var binPanel = CreateSettingRow(
            "Бинарные файлы:",
            "Проверка наличия winws.exe и WinDivert",
            2);
        
        var checkBtn = new Button
        {
            Content = "Проверить",
            Width = 100,
            Height = 28,
            Background = accentColor,
            Foreground = textColor,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        checkBtn.Click += (s, e) =>
        {
            var engine = new ZapretEngine();
            if (engine.CheckBinaries())
                MessageBox.Show("✓ Все файлы найдены", "Проверка", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show("✗ Файлы не найдены. Скачайте zapret-win-bundle.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
        };
        ((StackPanel)((Border)binPanel).Child).Children.Add(checkBtn);
        Grid.SetRow(binPanel, 2);
        mainGrid.Children.Add(binPanel);

        // About
        var aboutPanel = new StackPanel { Margin = new Thickness(0, 20, 0, 0) };
        aboutPanel.Children.Add(new TextBlock
        {
            Text = "О программе",
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Foreground = textColor,
            Margin = new Thickness(0, 0, 0, 10)
        });
        aboutPanel.Children.Add(new TextBlock
        {
            Text = "ZapretMod v2.0.0\n" +
                   "Графическая оболочка для zapret (winws.exe)\n" +
                   "Вдохновлено flowseal/zapret-discord-youtube\n\n" +
                   "Требуется: .NET 8 Desktop Runtime, Windows 10/11 x64",
            Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
            FontSize = 13,
            LineHeight = 4
        });
        Grid.SetRow(aboutPanel, 3);
        mainGrid.Children.Add(aboutPanel);

        // Close button
        var closeBtn = new Button
        {
            Content = "Закрыть",
            Width = 100,
            Height = 32,
            Background = accentColor,
            Foreground = textColor,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 20, 0, 0)
        };
        closeBtn.Click += (s, e) => Close();
        Grid.SetRow(closeBtn, 4);
        mainGrid.Children.Add(closeBtn);

        Content = mainGrid;
    }

    private Border CreateSettingRow(string title, string description, int row)
    {
        var panel = new StackPanel { Margin = new Thickness(0, 10, 0, 10) };
        
        panel.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brushes.White
        });
        
        panel.Children.Add(new TextBlock
        {
            Text = description,
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
            Margin = new Thickness(0, 5, 0, 10)
        });

        var border = new Border
        {
            Child = panel,
            Padding = new Thickness(10),
            Background = new SolidColorBrush(Color.FromRgb(45, 45, 45)),
            CornerRadius = new CornerRadius(6)
        };

        Grid.SetRow(border, row);
        return border;
    }
}
