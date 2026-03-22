using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ZapretMod.Core;

namespace ZapretMod;

public class SettingsWindow : Window
{
    public SettingsWindow()
    {
        Title = "Настройки - ZapretMod";
        Width = 600;
        Height = 500;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;
        Background = (Brush)new BrushConverter().ConvertFrom("#202020");

        var mainGrid = new Grid { Margin = new Thickness(25) };
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var title = new TextBlock
        {
            Text = "⚙ Настройки",
            FontSize = 22,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White,
            Margin = new Thickness(0, 0, 0, 25)
        };
        Grid.SetRow(title, 0);
        mainGrid.Children.Add(title);

        var scrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var contentPanel = new StackPanel();

        contentPanel.Children.Add(CreateSection(
            "📁 Бинарные файлы",
            "winws.exe и WinDivert должны находиться в папке bin\\",
            () =>
            {
                var engine = new ZapretEngine();
                if (engine.CheckBinaries())
                    MessageBox.Show("✓ Все файлы найдены в папке bin\\", "Проверка", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("✗ Файлы не найдены.\n\nСкачайте с:\nhttps://github.com/bol-van/zapret-win-bundle/releases", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
            },
            "Проверить"));

        contentPanel.Children.Add(CreateSection(
            "📦 zapret-win-bundle",
            "Используется оригинальный zapret-win-bundle от bol-van",
            () => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/bol-van/zapret-win-bundle",
                UseShellExecute = true
            }),
            "Открыть GitHub"));

        contentPanel.Children.Add(CreateSection(
            "ℹ О программе",
            "ZapretMod v2.0.0\n\n" +
            "Графическая оболочка для zapret (winws.exe)\n" +
            "Вдохновлено flowseal/zapret-discord-youtube\n\n" +
            "Требуется: Windows 10/11 x64, .NET 8",
            null, null));

        scrollViewer.Content = contentPanel;
        Grid.SetRow(scrollViewer, 1);
        mainGrid.Children.Add(scrollViewer);

        var closeBtn = new Button
        {
            Content = "Закрыть",
            Width = 120,
            Height = 36,
            Background = (Brush)new BrushConverter().ConvertFrom("#0078D4"),
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 25, 0, 0)
        };
        closeBtn.Click += (s, e) => Close();
        Grid.SetRow(closeBtn, 2);
        mainGrid.Children.Add(closeBtn);

        Content = mainGrid;
    }

    private Border CreateSection(string title, string description, System.Action? buttonAction, string? buttonText)
    {
        var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };
        
        panel.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 15,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brushes.White,
            Margin = new Thickness(0, 0, 0, 8)
        });
        
        panel.Children.Add(new TextBlock
        {
            Text = description,
            FontSize = 13,
            Foreground = (Brush)new BrushConverter().ConvertFrom("#B0B0B0"),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 12)
        });

        if (buttonAction != null && !string.IsNullOrEmpty(buttonText))
        {
            var btn = new Button
            {
                Content = buttonText,
                Width = 140,
                Height = 34,
                Background = (Brush)new BrushConverter().ConvertFrom("#2D2D30"),
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            btn.Click += (s, e) => buttonAction();
            panel.Children.Add(btn);
        }

        var border = new Border
        {
            Child = panel,
            Padding = new Thickness(18),
            Background = (Brush)new BrushConverter().ConvertFrom("#2D2D30"),
            CornerRadius = new CornerRadius(6)
        };

        return border;
    }
}
