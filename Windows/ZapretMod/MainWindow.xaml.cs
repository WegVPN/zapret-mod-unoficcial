using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ZapretMod.Core;
using Serilog;
using Hardcodet.Wpf.TaskbarNotification;

namespace ZapretMod;

public partial class MainWindow : Window
{
    private readonly ZapretEngine _zapretEngine;
    private TaskbarIcon? _notifyIcon;
    private bool _isClosing;

    public MainWindow()
    {
        InitializeComponent();
        
        _zapretEngine = new ZapretEngine();
        _zapretEngine.LogOutput += ZapretEngine_LogOutput;
        _zapretEngine.StateChanged += ZapretEngine_StateChanged;
        
        InitializeNotifyIcon();
        LoadStrategies();
        
        Log.Information("ZapretMod GUI started");
    }

    #region UI Elements

    private Grid _mainGrid = null!;
    private StackPanel _headerPanel = null!;
    private TextBlock _titleText = null!;
    private TextBlock _statusText = null!;
    private Button _toggleButton = null!;
    private ComboBox _strategyCombo = null!;
    private TextBlock _strategyDesc = null!;
    private CheckBox _gameFilterCheck = null!;
    private CheckBox _autoStartCheck = null!;
    private TextBox _logBox = null!;
    private Button _clearLogBtn = null!;
    private Button _saveLogBtn = null!;
    private Button _settingsBtn = null!;
    private Button _diagnosticsBtn = null!;

    private void InitializeComponent()
    {
        // Window settings
        Title = "ZapretMod - DPI Bypass for Discord, YouTube, Telegram";
        Width = 900;
        Height = 650;
        MinWidth = 800;
        MinHeight = 600;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        
        // Modern dark theme colors
        var bgColor = new SolidColorBrush(Color.FromRgb(32, 32, 32));
        var panelColor = new SolidColorBrush(Color.FromRgb(45, 45, 45));
        var accentColor = new SolidColorBrush(Color.FromRgb(0, 120, 215));
        var textColor = new SolidColorBrush(Colors.White);
        var successColor = new SolidColorBrush(Color.FromRgb(76, 209, 55));
        var errorColor = new SolidColorBrush(Color.FromRgb(255, 59, 48));

        // Main grid
        _mainGrid = new Grid
        {
            Background = bgColor
        };

        var mainRows = new RowDefinitionCollection
        {
            new RowDefinition { Height = GridLength.Auto },      // Header
            new RowDefinition { Height = GridLength.Auto },      // Strategy
            new RowDefinition { Height = GridLength.Auto },      // Options
            new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Logs
            new RowDefinition { Height = GridLength.Auto }       // Footer
        };

        for (int i = 0; i < 5; i++)
            _mainGrid.RowDefinitions.Add(mainRows[i]);

        // Header
        _headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(20, 15, 20, 15),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        _titleText = new TextBlock
        {
            Text = "🛡 ZapretMod",
            FontSize = 28,
            FontWeight = FontWeights.Bold,
            Foreground = accentColor,
            VerticalAlignment = VerticalAlignment.Center
        };

        _statusText = new TextBlock
        {
            Text = "● Остановлено",
            FontSize = 16,
            Margin = new Thickness(30, 0, 0, 0),
            Foreground = errorColor,
            VerticalAlignment = VerticalAlignment.Center
        };

        _headerPanel.Children.Add(_titleText);
        _headerPanel.Children.Add(_statusText);
        Grid.SetRow(_headerPanel, 0);
        _mainGrid.Children.Add(_headerPanel);

        // Strategy panel
        var strategyPanel = new StackPanel
        {
            Margin = new Thickness(20, 0, 20, 15)
        };

        var strategyLabel = new TextBlock
        {
            Text = "Стратегия обхода",
            FontSize = 14,
            Foreground = textColor,
            Margin = new Thickness(0, 0, 0, 8)
        };

        _strategyCombo = new ComboBox
        {
            Width = 400,
            HorizontalAlignment = HorizontalAlignment.Left,
            FontSize = 14,
            Background = panelColor,
            Foreground = textColor
        };
        _strategyCombo.SelectionChanged += StrategyCombo_SelectionChanged;

        _strategyDesc = new TextBlock
        {
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
            Margin = new Thickness(0, 8, 0, 0),
            TextWrapping = TextWrapping.Wrap,
            Width = 500
        };

        strategyPanel.Children.Add(strategyLabel);
        strategyPanel.Children.Add(_strategyCombo);
        strategyPanel.Children.Add(_strategyDesc);
        Grid.SetRow(strategyPanel, 1);
        _mainGrid.Children.Add(strategyPanel);

        // Options panel
        var optionsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(20, 0, 20, 15)
        };

        _gameFilterCheck = new CheckBox
        {
            Content = "🎮 Game Filter (UDP >1023)",
            Foreground = textColor,
            Margin = new Thickness(0, 0, 20, 0),
            FontSize = 13
        };

        _autoStartCheck = new CheckBox
        {
            Content = "🚀 Автозапуск с Windows",
            Foreground = textColor,
            FontSize = 13
        };
        _autoStartCheck.Checked += AutoStartCheck_Checked;
        _autoStartCheck.Unchecked += AutoStartCheck_Unchecked;

        _toggleButton = new Button
        {
            Content = "▶ Запустить",
            Width = 140,
            Height = 36,
            Margin = new Thickness(50, 0, 0, 0),
            Background = accentColor,
            Foreground = textColor,
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            BorderThickness = new Thickness(0)
        };
        _toggleButton.Click += ToggleButton_Click;

        _settingsBtn = new Button
        {
            Content = "⚙ Настройки",
            Width = 120,
            Height = 36,
            Margin = new Thickness(20, 0, 0, 0),
            Background = panelColor,
            Foreground = textColor,
            FontSize = 13,
            BorderThickness = new Thickness(0)
        };
        _settingsBtn.Click += SettingsBtn_Click;

        _diagnosticsBtn = new Button
        {
            Content = "🔍 Диагностика",
            Width = 120,
            Height = 36,
            Margin = new Thickness(10, 0, 0, 0),
            Background = panelColor,
            Foreground = textColor,
            FontSize = 13,
            BorderThickness = new Thickness(0)
        };
        _diagnosticsBtn.Click += DiagnosticsBtn_Click;

        optionsPanel.Children.Add(_gameFilterCheck);
        optionsPanel.Children.Add(_autoStartCheck);
        optionsPanel.Children.Add(_toggleButton);
        optionsPanel.Children.Add(_settingsBtn);
        optionsPanel.Children.Add(_diagnosticsBtn);
        Grid.SetRow(optionsPanel, 2);
        _mainGrid.Children.Add(optionsPanel);

        // Log box
        var logBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(25, 25, 25)),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(10),
            Margin = new Thickness(20, 0, 20, 10)
        };

        var logHeader = new TextBlock
        {
            Text = "📋 Логи",
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
            Margin = new Thickness(0, 0, 0, 8)
        };

        _logBox = new TextBox
        {
            IsReadOnly = true,
            IsReadOnlyCaretVisible = true,
            FontFamily = new FontFamily("Consolas"),
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            TextWrapping = TextWrapping.Wrap,
            AcceptsReturn = true
        };

        var logStack = new StackPanel();
        logStack.Children.Add(logHeader);
        logStack.Children.Add(_logBox);
        logBorder.Child = logStack;

        Grid.SetRow(logBorder, 3);
        _mainGrid.Children.Add(logBorder);

        // Footer buttons
        var footerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 0, 20, 15)
        };

        _clearLogBtn = new Button
        {
            Content = "🗑 Очистить лог",
            Width = 110,
            Height = 32,
            Background = panelColor,
            Foreground = textColor,
            FontSize = 12,
            BorderThickness = new Thickness(0)
        };
        _clearLogBtn.Click += ClearLogBtn_Click;

        _saveLogBtn = new Button
        {
            Content = "💾 Сохранить лог",
            Width = 110,
            Height = 32,
            Margin = new Thickness(10, 0, 0, 0),
            Background = panelColor,
            Foreground = textColor,
            FontSize = 12,
            BorderThickness = new Thickness(0)
        };
        _saveLogBtn.Click += SaveLogBtn_Click;

        footerPanel.Children.Add(_clearLogBtn);
        footerPanel.Children.Add(_saveLogBtn);
        Grid.SetRow(footerPanel, 4);
        _mainGrid.Children.Add(footerPanel);

        Content = _mainGrid;

        // Handle minimize to tray
        StateChanged += MainWindow_StateChanged;
        Closing += MainWindow_Closing;
    }

    #endregion

    #region Event Handlers

    private void InitializeNotifyIcon()
    {
        _notifyIcon = new TaskbarIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            ToolTipText = "ZapretMod"
        };

        var contextMenu = new ContextMenu();
        
        var toggleItem = new MenuItem { Header = "Запустить/Остановить" };
        toggleItem.Click += (s, e) => ToggleFromTray();
        
        var showItem = new MenuItem { Header = "Показать окно" };
        showItem.Click += (s, e) => ShowWindow();
        
        var exitItem = new MenuItem { Header = "Выход" };
        exitItem.Click += (s, e) => CloseApplication();

        contextMenu.Items.Add(toggleItem);
        contextMenu.Items.Add(showItem);
        contextMenu.Items.Add(exitItem);
        
        _notifyIcon.ContextMenu = contextMenu;
        _notifyIcon.TrayMouseDoubleClick += (s, e) => ShowWindow();
    }

    private void LoadStrategies()
    {
        var strategies = ZapretEngine.GetBuiltInStrategies();
        foreach (var strategy in strategies)
        {
            _strategyCombo.Items.Add(strategy);
        }
        _strategyCombo.SelectedIndex = 0;
    }

    private void StrategyCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_strategyCombo.SelectedItem is StrategyConfig config)
        {
            _strategyDesc.Text = config.Description;
        }
    }

    private void ToggleButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (_zapretEngine.IsRunning)
            {
                _zapretEngine.Stop();
            }
            else
            {
                if (_strategyCombo.SelectedItem is StrategyConfig config)
                {
                    _zapretEngine.Start(config);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "ZapretMod", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ZapretEngine_StateChanged(object? sender, StateChangedEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            if (e.IsRunning)
            {
                _statusText.Text = "● Работает";
                _statusText.Foreground = new SolidColorBrush(Color.FromRgb(76, 209, 55));
                _toggleButton.Content = "⏹ Остановить";
                _toggleButton.Background = new SolidColorBrush(Color.FromRgb(255, 59, 48));
                AppendLog($"✓ Стратегия '{e.StrategyName}' запущена", LogType.Info);
            }
            else
            {
                _statusText.Text = "● Остановлено";
                _statusText.Foreground = new SolidColorBrush(Color.FromRgb(255, 59, 48));
                _toggleButton.Content = "▶ Запустить";
                _toggleButton.Background = new SolidColorBrush(Color.FromRgb(0, 120, 215));
                AppendLog($"✗ Остановлено", LogType.Warning);
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
        _logBox.AppendText(message + "\n");
        _logBox.ScrollToEnd();
    }

    private void ClearLogBtn_Click(object? sender, RoutedEventArgs e)
    {
        _logBox.Clear();
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
            File.WriteAllText(dialog.FileName, _logBox.Text);
        }
    }

    private void SettingsBtn_Click(object? sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow();
        settingsWindow.Owner = this;
        settingsWindow.ShowDialog();
    }

    private void DiagnosticsBtn_Click(object? sender, RoutedEventArgs e)
    {
        var diagnosticsWindow = new DiagnosticsWindow();
        diagnosticsWindow.Owner = this;
        diagnosticsWindow.ShowDialog();
    }

    private void AutoStartCheck_Checked(object? sender, RoutedEventArgs e)
    {
        ServiceManager.InstallService();
        AppendLog("✓ Автозапуск включен", LogType.Info);
    }

    private void AutoStartCheck_Unchecked(object? sender, RoutedEventArgs e)
    {
        ServiceManager.RemoveService();
        AppendLog("✗ Автозапуск выключен", LogType.Warning);
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            Hide();
        }
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (!_isClosing)
        {
            e.Cancel = true;
            Hide();
        }
        else
        {
            _zapretEngine.Dispose();
            _notifyIcon?.Dispose();
        }
    }

    private void ToggleFromTray()
    {
        if (_zapretEngine.IsRunning)
            _zapretEngine.Stop();
        else if (_strategyCombo.SelectedItem is StrategyConfig config)
            _zapretEngine.Start(config);
    }

    private void ShowWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void CloseApplication()
    {
        _isClosing = true;
        _zapretEngine.Stop();
        Close();
    }

    #endregion
}
