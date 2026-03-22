using ZapretGUI.Core;
using Serilog;
using System.Drawing;

namespace ZapretGUI;

public partial class MainForm : Form
{
    private readonly ZapretManager _zapretManager;
    private readonly NetworkOptimizer _networkOptimizer;
    private readonly ProfileManager _profileManager;
    private readonly AutoStartManager _autoStartManager;
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _trayMenu;
    private bool _isClosing;

    public MainForm()
    {
        InitializeComponent();
        
        // Initialize managers
        _zapretManager = new ZapretManager();
        _networkOptimizer = new NetworkOptimizer();
        _profileManager = new ProfileManager();
        _autoStartManager = new AutoStartManager();

        // Initialize tray icon
        _trayMenu = CreateTrayMenu();
        _notifyIcon = CreateNotifyIcon();

        // Subscribe to events
        SubscribeToEvents();

        // Load initial state
        LoadSettings();
        PopulateUI();
    }

    #region Initialization

    private void InitializeComponent()
    {
        this.SuspendLayout();
        
        // Form settings
        this.AutoScaleDimensions = new SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(900, 650);
        this.MinimumSize = new Size(800, 600);
        this.Text = "ZapretGUI - DPI Circumvention & Network Optimizer";
        this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormClosing += MainForm_FormClosing;
        this.Resize += MainForm_Resize;

        // Create main tab control
        var tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9F)
        };

        // Create tabs
        var tabZapret = new TabPage("Обход блокировок");
        var tabOptimizer = new TabPage("Ускорение интернета");
        var tabLogs = new TabPage("Логи");
        var tabSettings = new TabPage("Настройки");

        tabZapret.Controls.Add(CreateZapretPanel());
        tabOptimizer.Controls.Add(CreateOptimizerPanel());
        tabLogs.Controls.Add(CreateLogsPanel());
        tabSettings.Controls.Add(CreateSettingsPanel());

        tabControl.TabPages.AddRange(new[] { tabZapret, tabOptimizer, tabLogs, tabSettings });

        this.Controls.Add(tabControl);
        
        // Apply theme
        ApplyTheme(_profileManager.Settings.DarkTheme);

        this.ResumeLayout(false);
    }

    private Panel CreateZapretPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(15)
        };

        var tableLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            AutoSize = true
        };

        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        // Status group
        var statusGroup = CreateStatusGroupBox();
        tableLayout.Controls.Add(statusGroup, 0, 0);

        // Profile group
        var profileGroup = CreateProfileGroupBox();
        tableLayout.Controls.Add(profileGroup, 0, 1);

        // Domains/IPs group
        var domainsGroup = CreateDomainsGroupBox();
        tableLayout.Controls.Add(domainsGroup, 0, 2);

        // Advanced group
        var advancedGroup = CreateAdvancedGroupBox();
        tableLayout.Controls.Add(advancedGroup, 0, 3);

        // Log preview
        var logPreviewGroup = CreateLogPreviewGroupBox();
        tableLayout.Controls.Add(logPreviewGroup, 0, 4);

        panel.Controls.Add(tableLayout);
        return panel;
    }

    private GroupBox CreateStatusGroupBox()
    {
        var group = new GroupBox
        {
            Text = "Статус zapret",
            Dock = DockStyle.Top,
            Height = 70,
            Font = new Font("Segoe UI", 9F)
        };

        var flowPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true
        };

        _lblStatus = new Label
        {
            Text = "Остановлено",
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.Red,
            Margin = new Padding(5, 10, 5, 5)
        };

        _btnToggleZapret = new Button
        {
            Text = "Запустить",
            Size = new Size(100, 30),
            Margin = new Padding(10, 5, 5, 5),
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        _btnToggleZapret.Click += BtnToggleZapret_Click;

        flowPanel.Controls.AddRange(new Control[] { _lblStatus, _btnToggleZapret });
        group.Controls.Add(flowPanel);

        return group;
    }

    private GroupBox CreateProfileGroupBox()
    {
        var group = new GroupBox
        {
            Text = "Профиль",
            Dock = DockStyle.Top,
            Height = 120,
            Font = new Font("Segoe UI", 9F)
        };

        var tableLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(10)
        };

        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // Profile selection
        tableLayout.Controls.Add(new Label { Text = "Профиль:", AutoSize = true, Margin = new Padding(3) }, 0, 0);
        _cmbProfiles = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(3)
        };
        _cmbProfiles.SelectedIndexChanged += CmbProfiles_SelectedIndexChanged;
        tableLayout.Controls.Add(_cmbProfiles, 1, 0);

        // Interface selection
        tableLayout.Controls.Add(new Label { Text = "Интерфейс:", AutoSize = true, Margin = new Padding(3) }, 0, 1);
        _cmbInterfaces = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(3)
        };
        tableLayout.Controls.Add(_cmbInterfaces, 1, 1);

        // Apply button
        var applyPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, AutoSize = true };
        _btnApplyProfile = new Button
        {
            Text = "Применить профиль",
            Size = new Size(140, 30),
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        _btnApplyProfile.Click += BtnApplyProfile_Click;
        applyPanel.Controls.Add(_btnApplyProfile);
        tableLayout.Controls.Add(applyPanel, 1, 2);

        group.Controls.Add(tableLayout);
        return group;
    }

    private GroupBox CreateDomainsGroupBox()
    {
        var group = new GroupBox
        {
            Text = "Домены и IP для обхода",
            Dock = DockStyle.Top,
            Height = 150,
            Font = new Font("Segoe UI", 9F)
        };

        var tableLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(10)
        };

        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        tableLayout.Controls.Add(new Label { Text = "Домены (через запятую):", AutoSize = true }, 0, 0);
        tableLayout.Controls.Add(new Label { Text = "IP адреса/сети:", AutoSize = true }, 1, 0);

        _txtDomains = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Margin = new Padding(3)
        };
        tableLayout.Controls.Add(_txtDomains, 0, 1);

        _txtIps = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Margin = new Padding(3)
        };
        tableLayout.Controls.Add(_txtIps, 1, 1);

        group.Controls.Add(tableLayout);
        return group;
    }

    private GroupBox CreateAdvancedGroupBox()
    {
        var group = new GroupBox
        {
            Text = "Дополнительные параметры",
            Dock = DockStyle.Top,
            Height = 80,
            Font = new Font("Segoe UI", 9F)
        };

        var tableLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(10)
        };

        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        tableLayout.Controls.Add(new Label { Text = "Метод:", AutoSize = true, Margin = new Padding(3) }, 0, 0);
        _cmbMethod = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(3)
        };
        _cmbMethod.Items.AddRange(new object[] { "fake", "http", "tls", "mix" });
        _cmbMethod.SelectedIndex = 0;
        tableLayout.Controls.Add(_cmbMethod, 1, 0);

        tableLayout.Controls.Add(new Label { Text = "Аргументы:", AutoSize = true, Margin = new Padding(3) }, 0, 1);
        _txtAdditionalArgs = new TextBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(3)
        };
        tableLayout.Controls.Add(_txtAdditionalArgs, 1, 1);

        group.Controls.Add(tableLayout);
        return group;
    }

    private GroupBox CreateLogPreviewGroupBox()
    {
        var group = new GroupBox
        {
            Text = "Предпросмотр логов",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9F)
        };

        _txtLogPreview = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            ReadOnly = true,
            Font = new Font("Consolas", 8.5F),
            BackColor = Color.FromArgb(30, 30, 30),
            ForeColor = Color.FromArgb(200, 200, 200)
        };

        group.Controls.Add(_txtLogPreview);
        return group;
    }

    private Panel CreateOptimizerPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(15),
            AutoScroll = true
        };

        var flowLayout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            WrapContents = false
        };

        // TCP Optimization
        flowLayout.Controls.Add(CreateTcpOptimizationGroup());
        
        // DNS Optimization
        flowLayout.Controls.Add(CreateDnsOptimizationGroup());
        
        // MTU Optimization
        flowLayout.Controls.Add(CreateMtuOptimizationGroup());

        // Apply button
        var applyPanel = new Panel
        {
            Height = 50,
            Dock = DockStyle.Top
        };
        
        _btnApplyOptimizations = new Button
        {
            Text = "Применить оптимизации",
            Size = new Size(180, 35),
            Location = new Point(10, 5),
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };
        _btnApplyOptimizations.Click += BtnApplyOptimizations_Click;
        applyPanel.Controls.Add(_btnApplyOptimizations);
        flowLayout.Controls.Add(applyPanel);

        panel.Controls.Add(flowLayout);
        return panel;
    }

    private GroupBox CreateTcpOptimizationGroup()
    {
        var group = new GroupBox
        {
            Text = "Оптимизация TCP",
            Width = 800,
            Height = 180,
            Font = new Font("Segoe UI", 9F)
        };

        var tableLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(10)
        };

        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _chkOptimizeAutoTuning = new CheckBox { Text = "Оптимизировать TCP Auto-Tuning", AutoSize = true, Checked = true };
        _chkEnableEcn = new CheckBox { Text = "Включить ECN", AutoSize = true, Checked = true };
        _chkEnableTimestamps = new CheckBox { Text = "Включить TCP Timestamps", AutoSize = true, Checked = true };
        _chkEnableSelectiveAck = new CheckBox { Text = "Включить Selective ACK", AutoSize = true, Checked = true };
        _chkDisableNagle = new CheckBox { Text = "Отключить Nagle для игр/мессенджеров", AutoSize = true };
        _chkEnableBBR = new CheckBox { Text = "Включить BBR (если поддерживается)", AutoSize = true };

        tableLayout.Controls.Add(_chkOptimizeAutoTuning, 0, 0);
        tableLayout.Controls.Add(_chkEnableEcn, 1, 0);
        tableLayout.Controls.Add(_chkEnableTimestamps, 0, 1);
        tableLayout.Controls.Add(_chkEnableSelectiveAck, 1, 1);
        tableLayout.Controls.Add(_chkDisableNagle, 0, 2);
        tableLayout.Controls.Add(_chkEnableBBR, 1, 2);

        var resetPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, AutoSize = true };
        _btnResetTcp = new Button
        {
            Text = "Сбросить TCP настройки",
            Size = new Size(150, 28),
            FlatStyle = FlatStyle.Flat
        };
        _btnResetTcp.Click += BtnResetTcp_Click;
        resetPanel.Controls.Add(_btnResetTcp);
        tableLayout.Controls.Add(resetPanel, 1, 4);

        group.Controls.Add(tableLayout);
        return group;
    }

    private GroupBox CreateDnsOptimizationGroup()
    {
        var group = new GroupBox
        {
            Text = "DNS настройки",
            Width = 800,
            Height = 140,
            Font = new Font("Segoe UI", 9F)
        };

        var tableLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 3,
            Padding = new Padding(10)
        };

        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        tableLayout.Controls.Add(new Label { Text = "Первичный DNS:", AutoSize = true, Margin = new Padding(3) }, 0, 0);
        _txtDnsPrimary = new TextBox { Text = "1.1.1.1", Margin = new Padding(3) };
        tableLayout.Controls.Add(_txtDnsPrimary, 1, 0);

        tableLayout.Controls.Add(new Label { Text = "Вторичный DNS:", AutoSize = true, Margin = new Padding(3) }, 0, 1);
        _txtDnsSecondary = new TextBox { Text = "1.0.0.1", Margin = new Padding(3) };
        tableLayout.Controls.Add(_txtDnsSecondary, 1, 1);

        var dnsButtonsPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
        _btnSetDns = new Button
        {
            Text = "Установить DNS",
            Size = new Size(120, 28),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White
        };
        _btnSetDns.Click += BtnSetDns_Click;
        
        _btnFlushDns = new Button
        {
            Text = "Очистить кэш DNS",
            Size = new Size(130, 28),
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(5, 0, 0, 0)
        };
        _btnFlushDns.Click += BtnFlushDns_Click;
        
        _btnResetDns = new Button
        {
            Text = "Сбросить DNS",
            Size = new Size(100, 28),
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(5, 0, 0, 0)
        };
        _btnResetDns.Click += BtnResetDns_Click;

        dnsButtonsPanel.Controls.AddRange(new Control[] { _btnSetDns, _btnFlushDns, _btnResetDns });
        tableLayout.Controls.Add(dnsButtonsPanel, 2, 0);

        group.Controls.Add(tableLayout);
        return group;
    }

    private GroupBox CreateMtuOptimizationGroup()
    {
        var group = new GroupBox
        {
            Text = "MTU настройки",
            Width = 800,
            Height = 120,
            Font = new Font("Segoe UI", 9F)
        };

        var tableLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 2,
            Padding = new Padding(10)
        };

        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        tableLayout.Controls.Add(new Label { Text = "Сетевой интерфейс:", AutoSize = true, Margin = new Padding(3) }, 0, 0);
        _cmbMtuInterfaces = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(3)
        };
        tableLayout.Controls.Add(_cmbMtuInterfaces, 1, 0);

        tableLayout.Controls.Add(new Label { Text = "MTU:", AutoSize = true, Margin = new Padding(3) }, 0, 1);
        _numMtuValue = new NumericUpDown
        {
            Minimum = 576,
            Maximum = 9000,
            Value = 1500,
            Increment = 100,
            Margin = new Padding(3)
        };
        tableLayout.Controls.Add(_numMtuValue, 1, 1);

        var mtuButtonsPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
        _btnFindOptimalMtu = new Button
        {
            Text = "Найти оптимальный",
            Size = new Size(130, 28),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White
        };
        _btnFindOptimalMtu.Click += BtnFindOptimalMtu_Click;
        
        _btnSetMtu = new Button
        {
            Text = "Установить MTU",
            Size = new Size(120, 28),
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(5, 0, 0, 0)
        };
        _btnSetMtu.Click += BtnSetMtu_Click;
        
        _btnResetMtu = new Button
        {
            Text = "Сбросить MTU",
            Size = new Size(100, 28),
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(5, 0, 0, 0)
        };
        _btnResetMtu.Click += BtnResetMtu_Click;

        mtuButtonsPanel.Controls.AddRange(new Control[] { _btnFindOptimalMtu, _btnSetMtu, _btnResetMtu });
        tableLayout.Controls.Add(mtuButtonsPanel, 2, 1);

        group.Controls.Add(tableLayout);
        return group;
    }

    private Panel CreateLogsPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(15)
        };

        var tableLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };

        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _txtFullLog = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            ReadOnly = true,
            Font = new Font("Consolas", 9F),
            BackColor = Color.FromArgb(30, 30, 30),
            ForeColor = Color.FromArgb(200, 200, 200)
        };
        tableLayout.Controls.Add(_txtFullLog, 0, 0);

        var buttonsPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Padding = new Padding(0, 10, 0, 0)
        };

        _btnSaveLog = new Button
        {
            Text = "Сохранить в файл",
            Size = new Size(130, 30),
            FlatStyle = FlatStyle.Flat
        };
        _btnSaveLog.Click += BtnSaveLog_Click;

        _btnClearLog = new Button
        {
            Text = "Очистить лог",
            Size = new Size(100, 30),
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(5, 0, 0, 0)
        };
        _btnClearLog.Click += BtnClearLog_Click;

        buttonsPanel.Controls.AddRange(new Control[] { _btnSaveLog, _btnClearLog });
        tableLayout.Controls.Add(buttonsPanel, 0, 1);

        panel.Controls.Add(tableLayout);
        return panel;
    }

    private Panel CreateSettingsPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(15),
            AutoScroll = true
        };

        var flowLayout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            WrapContents = false
        };

        // General settings
        flowLayout.Controls.Add(CreateGeneralSettingsGroup());
        
        // Zapret path settings
        flowLayout.Controls.Add(CreateZapretPathGroup());
        
        // Backup settings
        flowLayout.Controls.Add(CreateBackupSettingsGroup());

        panel.Controls.Add(flowLayout);
        return panel;
    }

    private GroupBox CreateGeneralSettingsGroup()
    {
        var group = new GroupBox
        {
            Text = "Общие настройки",
            Width = 800,
            Height = 150,
            Font = new Font("Segoe UI", 9F)
        };

        var tableLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(10)
        };

        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _chkAutoStart = new CheckBox { Text = "Автозагрузка при старте Windows", AutoSize = true };
        _chkAutoStart.CheckedChanged += ChkAutoStart_CheckedChanged;
        
        _chkStartMinimized = new CheckBox { Text = "Запускать свёрнутым", AutoSize = true };
        _chkMinimizeToTray = new CheckBox { Text = "Сворачивать в трей", AutoSize = true, Checked = true };
        _chkCheckUpdates = new CheckBox { Text = "Проверять обновления", AutoSize = true, Checked = true };
        _chkDarkTheme = new CheckBox { Text = "Тёмная тема", AutoSize = true };
        _chkDarkTheme.CheckedChanged += ChkDarkTheme_CheckedChanged;

        tableLayout.Controls.Add(_chkAutoStart, 0, 0);
        tableLayout.Controls.Add(_chkStartMinimized, 1, 0);
        tableLayout.Controls.Add(_chkMinimizeToTray, 0, 1);
        tableLayout.Controls.Add(_chkCheckUpdates, 1, 1);
        tableLayout.Controls.Add(_chkDarkTheme, 0, 2);

        group.Controls.Add(tableLayout);
        return group;
    }

    private GroupBox CreateZapretPathGroup()
    {
        var group = new GroupBox
        {
            Text = "Путь к zapret",
            Width = 800,
            Height = 80,
            Font = new Font("Segoe UI", 9F)
        };

        var tableLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(10)
        };

        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        _txtZapretPath = new TextBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(3)
        };
        tableLayout.Controls.Add(_txtZapretPath, 0, 0);

        _btnBrowseZapret = new Button
        {
            Text = "Обзор...",
            Size = new Size(90, 28),
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(3)
        };
        _btnBrowseZapret.Click += BtnBrowseZapret_Click;
        tableLayout.Controls.Add(_btnBrowseZapret, 1, 0);

        group.Controls.Add(tableLayout);
        return group;
    }

    private GroupBox CreateBackupSettingsGroup()
    {
        var group = new GroupBox
        {
            Text = "Резервное копирование",
            Width = 800,
            Height = 100,
            Font = new Font("Segoe UI", 9F)
        };

        var flowLayout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(10),
            AutoSize = true
        };

        _btnCreateRestorePoint = new Button
        {
            Text = "Создать точку восстановления",
            Size = new Size(180, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White
        };
        _btnCreateRestorePoint.Click += BtnCreateRestorePoint_Click;

        _btnExportSettings = new Button
        {
            Text = "Экспорт настроек",
            Size = new Size(130, 35),
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(5, 0, 0, 0)
        };
        _btnExportSettings.Click += BtnExportSettings_Click;

        _btnImportSettings = new Button
        {
            Text = "Импорт настроек",
            Size = new Size(130, 35),
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(5, 0, 0, 0)
        };
        _btnImportSettings.Click += BtnImportSettings_Click;

        flowLayout.Controls.AddRange(new Control[] { _btnCreateRestorePoint, _btnExportSettings, _btnImportSettings });
        group.Controls.Add(flowLayout);

        return group;
    }

    private NotifyIcon CreateNotifyIcon()
    {
        return new NotifyIcon
        {
            Icon = this.Icon,
            Text = "ZapretGUI",
            Visible = true,
            ContextMenuStrip = _trayMenu
        };
    }

    private ContextMenuStrip CreateTrayMenu()
    {
        var menu = new ContextMenuStrip();

        var toggleItem = new ToolStripMenuItem("Вкл/Выкл обход");
        toggleItem.Click += (s, e) => ToggleZapretFromTray();
        menu.Items.Add(toggleItem);

        var profilesItem = new ToolStripMenuItem("Сменить профиль");
        foreach (var profile in _profileManager.Profiles)
        {
            var profileItem = new ToolStripMenuItem(profile.Name);
            profileItem.Click += (s, e) => ChangeProfileFromTray(profile.Name);
            profilesItem.DropDownItems.Add(profileItem);
        }
        menu.Items.Add(profilesItem);

        menu.Items.Add(new ToolStripSeparator());

        var showItem = new ToolStripMenuItem("Показать окно");
        showItem.Click += (s, e) => ShowWindow();
        menu.Items.Add(showItem);

        menu.Items.Add(new ToolStripSeparator());

        var exitItem = new ToolStripMenuItem("Выход");
        exitItem.Click += (s, e) => CloseApplication();
        menu.Items.Add(exitItem);

        return menu;
    }

    #endregion

    #region Event Handlers

    private void SubscribeToEvents()
    {
        _zapretManager.LogOutput += ZapretManager_LogOutput;
        _zapretManager.StateChanged += ZapretManager_StateChanged;
        _networkOptimizer.LogOutput += NetworkOptimizer_LogOutput;
        _networkOptimizer.OptimizationApplied += NetworkOptimizer_OptimizationApplied;
        _notifyIcon.DoubleClick += (s, e) => ShowWindow();
    }

    private void LoadSettings()
    {
        var settings = _profileManager.Settings;
        
        _chkAutoStart.Checked = settings.AutoStart;
        _chkStartMinimized.Checked = settings.StartMinimized;
        _chkMinimizeToTray.Checked = settings.MinimizeToTray;
        _chkCheckUpdates.Checked = settings.CheckUpdates;
        _chkDarkTheme.Checked = settings.DarkTheme;
        _txtZapretPath.Text = settings.ZapretPath;

        if (!string.IsNullOrEmpty(settings.ZapretPath))
        {
            _zapretManager.Initialize(settings.ZapretPath);
        }
    }

    private void PopulateUI()
    {
        // Populate profiles
        _cmbProfiles.Items.Clear();
        foreach (var profile in _profileManager.Profiles)
        {
            _cmbProfiles.Items.Add(profile);
        }
        _cmbProfiles.SelectedItem = _profileManager.GetProfile(_profileManager.Settings.ActiveProfile);

        // Populate interfaces
        _cmbInterfaces.Items.Clear();
        _cmbInterfaces.Items.AddRange(_profileManager.GetAvailableInterfaces().ToArray());
        _cmbInterfaces.SelectedIndex = 0;

        // Populate MTU interfaces
        _cmbMtuInterfaces.Items.Clear();
        foreach (var iface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
        {
            if (iface.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
            {
                _cmbMtuInterfaces.Items.Add(iface.Name);
            }
        }
        if (_cmbMtuInterfaces.Items.Count > 0)
            _cmbMtuInterfaces.SelectedIndex = 0;
    }

    private void ApplyTheme(bool darkTheme)
    {
        if (darkTheme)
        {
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
        }
        else
        {
            this.BackColor = SystemColors.Control;
            this.ForeColor = SystemColors.ControlText;
        }
    }

    #endregion

    #region Zapret Controls

    private Label _lblStatus = null!;
    private Button _btnToggleZapret = null!;
    private ComboBox _cmbProfiles = null!;
    private ComboBox _cmbInterfaces = null!;
    private Button _btnApplyProfile = null!;
    private TextBox _txtDomains = null!;
    private TextBox _txtIps = null!;
    private ComboBox _cmbMethod = null!;
    private TextBox _txtAdditionalArgs = null!;
    private TextBox _txtLogPreview = null!;

    private void BtnToggleZapret_Click(object? sender, EventArgs e)
    {
        if (_zapretManager.IsRunning)
        {
            _zapretManager.Stop();
        }
        else
        {
            var config = GetCurrentConfig();
            _zapretManager.Start(config);
        }
    }

    private void CmbProfiles_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_cmbProfiles.SelectedItem is ZapretProfile profile)
        {
            _txtDomains.Text = profile.Config.Domains;
            _txtIps.Text = profile.Config.Ips;
            _cmbMethod.Text = profile.Config.Method;
            _txtAdditionalArgs.Text = profile.Config.AdditionalArgs;
        }
    }

    private void BtnApplyProfile_Click(object? sender, EventArgs e)
    {
        var config = GetCurrentConfig();
        _zapretManager.Restart(config);
    }

    private ZapretConfig GetCurrentConfig()
    {
        return new ZapretConfig
        {
            ProfileName = _cmbProfiles.SelectedItem?.ToString() ?? "Custom",
            Preset = _cmbProfiles.SelectedItem is ZapretProfile p ? p.Config.Preset : ZapretPreset.Custom,
            Method = _cmbMethod.Text,
            Domains = _txtDomains.Text,
            Ips = _txtIps.Text,
            Ports = "443,80",
            Interface = _cmbInterfaces.SelectedItem?.ToString() ?? "",
            AdditionalArgs = _txtAdditionalArgs.Text
        };
    }

    private void ZapretManager_LogOutput(object? sender, LogEventArgs e)
    {
        if (InvokeRequired)
        {
            Invoke(() => ZapretManager_LogOutput(sender, e));
            return;
        }

        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var logLine = $"[{timestamp}] {e.Message}";
        
        _txtLogPreview.AppendText(logLine + Environment.NewLine);
        _txtFullLog.AppendText(logLine + Environment.NewLine);
        
        // Scroll to bottom
        _txtLogPreview.SelectionStart = _txtLogPreview.Text.Length;
        _txtLogPreview.ScrollToCaret();
        _txtFullLog.SelectionStart = _txtFullLog.Text.Length;
        _txtFullLog.ScrollToCaret();
    }

    private void ZapretManager_StateChanged(object? sender, StateChangedEventArgs e)
    {
        if (InvokeRequired)
        {
            Invoke(() => ZapretManager_StateChanged(sender, e));
            return;
        }

        if (e.IsRunning)
        {
            _lblStatus.Text = "Работает";
            _lblStatus.ForeColor = Color.Green;
            _btnToggleZapret.Text = "Остановить";
        }
        else
        {
            _lblStatus.Text = "Остановлено";
            _lblStatus.ForeColor = Color.Red;
            _btnToggleZapret.Text = "Запустить";
        }
    }

    private void ToggleZapretFromTray()
    {
        if (_zapretManager.IsRunning)
            _zapretManager.Stop();
        else
        {
            var config = GetCurrentConfig();
            _zapretManager.Start(config);
        }
    }

    private void ChangeProfileFromTray(string profileName)
    {
        _profileManager.SetActiveProfile(profileName);
        var profile = _profileManager.GetProfile(profileName);
        if (profile != null && _zapretManager.IsRunning)
        {
            _zapretManager.Restart(profile.Config);
        }
    }

    #endregion

    #region Optimizer Controls

    private CheckBox _chkOptimizeAutoTuning = null!;
    private CheckBox _chkEnableEcn = null!;
    private CheckBox _chkEnableTimestamps = null!;
    private CheckBox _chkEnableSelectiveAck = null!;
    private CheckBox _chkDisableNagle = null!;
    private CheckBox _chkEnableBBR = null!;
    private Button _btnResetTcp = null!;
    private TextBox _txtDnsPrimary = null!;
    private TextBox _txtDnsSecondary = null!;
    private Button _btnSetDns = null!;
    private Button _btnFlushDns = null!;
    private Button _btnResetDns = null!;
    private ComboBox _cmbMtuInterfaces = null!;
    private NumericUpDown _numMtuValue = null!;
    private Button _btnFindOptimalMtu = null!;
    private Button _btnSetMtu = null!;
    private Button _btnResetMtu = null!;
    private Button _btnApplyOptimizations = null!;

    private async void BtnApplyOptimizations_Click(object? sender, EventArgs e)
    {
        var result = MessageBox.Show(
            "Применение оптимизаций может потребовать перезагрузки.\nПродолжить?",
            "Подтверждение",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result != DialogResult.Yes)
            return;

        // Create restore point
        await _networkOptimizer.CreateRestorePointAsync("ZapretGUI Optimization");

        var tcpSettings = new TcpOptimizationSettings
        {
            OptimizeAutoTuning = _chkOptimizeAutoTuning.Checked,
            EnableEcn = _chkEnableEcn.Checked,
            EnableTimestamps = _chkEnableTimestamps.Checked,
            EnableSelectiveAck = _chkEnableSelectiveAck.Checked,
            DisableNagleForApps = _chkDisableNagle.Checked,
            EnableBBR = _chkEnableBBR.Checked
        };

        await _networkOptimizer.OptimizeTcpAsync(tcpSettings);
    }

    private async void BtnResetTcp_Click(object? sender, EventArgs e)
    {
        await _networkOptimizer.ResetTcpSettingsAsync();
    }

    private async void BtnSetDns_Click(object? sender, EventArgs e)
    {
        var dnsServers = new[] { _txtDnsPrimary.Text, _txtDnsSecondary.Text };
        await _networkOptimizer.SetDnsServersAsync(dnsServers);
    }

    private async void BtnFlushDns_Click(object? sender, EventArgs e)
    {
        await _networkOptimizer.FlushDnsAsync();
    }

    private async void BtnResetDns_Click(object? sender, EventArgs e)
    {
        await _networkOptimizer.ResetDnsSettingsAsync();
    }

    private async void BtnFindOptimalMtu_Click(object? sender, EventArgs e)
    {
        if (_cmbMtuInterfaces.SelectedItem != null)
        {
            var interfaceName = _cmbMtuInterfaces.SelectedItem.ToString()!;
            var optimalMtu = await _networkOptimizer.FindOptimalMtuAsync(interfaceName);
            _numMtuValue.Value = optimalMtu;
        }
    }

    private async void BtnSetMtu_Click(object? sender, EventArgs e)
    {
        if (_cmbMtuInterfaces.SelectedItem != null)
        {
            var interfaceName = _cmbMtuInterfaces.SelectedItem.ToString()!;
            await _networkOptimizer.SetMtuAsync(interfaceName, (int)_numMtuValue.Value);
        }
    }

    private async void BtnResetMtu_Click(object? sender, EventArgs e)
    {
        if (_cmbMtuInterfaces.SelectedItem != null)
        {
            var interfaceName = _cmbMtuInterfaces.SelectedItem.ToString()!;
            await _networkOptimizer.ResetMtuAsync(interfaceName);
        }
    }

    private void NetworkOptimizer_LogOutput(object? sender, LogEventArgs e)
    {
        if (InvokeRequired)
        {
            Invoke(() => NetworkOptimizer_LogOutput(sender, e));
            return;
        }

        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        _txtFullLog.AppendText($"[{timestamp}] {e.Message}" + Environment.NewLine);
    }

    private void NetworkOptimizer_OptimizationApplied(object? sender, OptimizationEventArgs e)
    {
        if (InvokeRequired)
        {
            Invoke(() => NetworkOptimizer_OptimizationApplied(sender, e));
            return;
        }

        MessageBox.Show(e.Message, "Оптимизация", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    #endregion

    #region Settings Controls

    private CheckBox _chkAutoStart = null!;
    private CheckBox _chkStartMinimized = null!;
    private CheckBox _chkMinimizeToTray = null!;
    private CheckBox _chkCheckUpdates = null!;
    private CheckBox _chkDarkTheme = null!;
    private TextBox _txtZapretPath = null!;
    private Button _btnBrowseZapret = null!;
    private Button _btnCreateRestorePoint = null!;
    private Button _btnExportSettings = null!;
    private Button _btnImportSettings = null!;
    private TextBox _txtFullLog = null!;
    private Button _btnSaveLog = null!;
    private Button _btnClearLog = null!;

    private void ChkAutoStart_CheckedChanged(object? sender, EventArgs e)
    {
        if (_chkAutoStart.Checked)
            _autoStartManager.Enable();
        else
            _autoStartManager.Disable();

        _profileManager.Settings.AutoStart = _chkAutoStart.Checked;
        _profileManager.SaveSettings();
    }

    private void ChkDarkTheme_CheckedChanged(object? sender, EventArgs e)
    {
        ApplyTheme(_chkDarkTheme.Checked);
        _profileManager.Settings.DarkTheme = _chkDarkTheme.Checked;
        _profileManager.SaveSettings();
    }

    private void BtnBrowseZapret_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Executable Files|*.exe|All Files|*.*",
            Title = "Выберите файл zapret"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            _txtZapretPath.Text = dialog.FileName;
            _profileManager.Settings.ZapretPath = dialog.FileName;
            _profileManager.SaveSettings();
            _zapretManager.Initialize(dialog.FileName);
        }
    }

    private async void BtnCreateRestorePoint_Click(object? sender, EventArgs e)
    {
        await _networkOptimizer.CreateRestorePointAsync("ZapretGUI Manual Backup");
    }

    private void BtnExportSettings_Click(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "JSON Files|*.json|All Files|*.*",
            Title = "Экспорт настроек"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            _profileManager.ExportProfiles(dialog.FileName);
        }
    }

    private void BtnImportSettings_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "JSON Files|*.json|All Files|*.*",
            Title = "Импорт настроек"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            _profileManager.ImportProfiles(dialog.FileName);
            PopulateUI();
        }
    }

    private void BtnSaveLog_Click(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "Text Files|*.txt|All Files|*.*",
            Title = "Сохранить лог"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            File.WriteAllText(dialog.FileName, _txtFullLog.Text);
        }
    }

    private void BtnClearLog_Click(object? sender, EventArgs e)
    {
        _txtFullLog.Clear();
        _txtLogPreview.Clear();
    }

    #endregion

    #region Form Events

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!_isClosing && _profileManager.Settings.MinimizeToTray)
        {
            e.Cancel = true;
            this.Hide();
            _notifyIcon.Visible = true;
        }
        else
        {
            _zapretManager.Dispose();
            _notifyIcon.Dispose();
        }
    }

    private void MainForm_Resize(object? sender, EventArgs e)
    {
        if (this.WindowState == FormWindowState.Minimized && _profileManager.Settings.MinimizeToTray)
        {
            this.Hide();
        }
    }

    private void ShowWindow()
    {
        this.Show();
        this.WindowState = FormWindowState.Normal;
        this.Activate();
    }

    private void CloseApplication()
    {
        _isClosing = true;
        _zapretManager.Stop();
        this.Close();
    }

    #endregion

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _zapretManager.Dispose();
        _networkOptimizer.Dispose();
        base.OnFormClosing(e);
    }
}
