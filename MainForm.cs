using Reminder.Models;
using Reminder.Services;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using Microsoft.Win32;

namespace Reminder;

public partial class MainForm : Form
{
    private readonly ConfigService _configService;
    private readonly VPNSubscriptionService _vpnService;
    private List<SubscriptionItem> _subscriptions = new();
    private List<SubscriptionPanel> _subscriptionPanels = new();
    private Point _mouseOffset;
    private bool _isMinimized;
    private bool _sortAscending = true; // 默认从小到大
    private System.ComponentModel.IContainer? components = null;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr GetWindowDC(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

    private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    private const int DWMWCP_DEFAULT = 0;
    private const int DWMWCP_DONOTROUND = 1;
    private const int DWMWCP_ROUND = 2;
    private const int DWMWCP_ROUNDSMALL = 3;

    [DllImport("dwmapi.dll", CharSet = CharSet.Auto)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const string RunKeyName = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "Reminder";

    public MainForm()
    {
        InitializeComponent();
        _configService = new ConfigService();
        _vpnService = new VPNSubscriptionService();

        // 设置自定义托盘图标
        try
        {
            string iconPath = Path.Combine(Application.StartupPath, "t1b8q-xnjs2-001.ico");
            if (File.Exists(iconPath))
            {
                notifyIcon.Icon = new Icon(iconPath);
            }
            else
            {
                // 如果图标文件不存在，使用备用图标
                notifyIcon.Icon = CreateCustomIcon();
            }
        }
        catch
        {
            // 如果加载失败，使用备用图标
            notifyIcon.Icon = CreateCustomIcon();
        }

        // 设置圆角窗口
        SetRoundedCorners();

        // 加载自启动状态
        LoadAutoStartState();
    }

    private void LoadAutoStartState()
    {
        menuItemAutoStart.Checked = IsAutoStartEnabled();
    }

    private bool IsAutoStartEnabled()
    {
        try
        {
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyName, false))
            {
                string? value = key?.GetValue(AppName) as string;
                return !string.IsNullOrEmpty(value) && File.Exists(value);
            }
        }
        catch
        {
            return false;
        }
    }

    private Icon CreateCustomIcon()
    {
        // 创建一个简单的绿色圆角图标
        using (Bitmap bitmap = new Bitmap(16, 16))
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // 绘制圆角矩形背景
            using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddLines(new Point[]
                {
                    new Point(3, 0),
                    new Point(13, 0),
                    new Point(16, 3),
                    new Point(16, 13),
                    new Point(13, 16),
                    new Point(3, 16),
                    new Point(0, 13),
                    new Point(0, 3)
                });
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(76, 175, 80)))
                {
                    g.FillPath(brush, path);
                }
            }

            // 绘制勾选标记
            using (Pen pen = new Pen(Color.White, 2))
            {
                g.DrawLine(pen, 5, 8, 7, 10);
                g.DrawLine(pen, 7, 10, 11, 6);
            }

            // 转换为 Icon
            IntPtr hIcon = bitmap.GetHicon();
            return Icon.FromHandle(hIcon);
        }
    }

    private void SetRoundedCorners()
    {
        try
        {
            // 使用 DWM API 设置圆角 (Windows 11)
            int preference = DWMWCP_ROUND;
            DwmSetWindowAttribute(this.Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));
        }
        catch
        {
            // 如果 DWM 失败，使用传统方法
            int radius = 12;
            IntPtr hRgn = CreateRoundRectRgn(0, 0, this.Width + 1, this.Height + 1, radius, radius);
            SetWindowRgn(this.Handle, hRgn, true);
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        // 窗口大小改变时重新应用圆角
        if (this.IsHandleCreated && !this.IsDisposed)
        {
            try
            {
                int preference = DWMWCP_ROUND;
                DwmSetWindowAttribute(this.Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));
            }
            catch { }
        }
    }

    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams cp = base.CreateParams;
            cp.Style &= ~0x00040000; // 移除 WS_THICKFRAME，禁用大小调整
            return cp;
        }
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        // 设置窗口位置到屏幕右上角
        var workingArea = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1920, 1080);
        this.Location = new Point(workingArea.Right - this.Width - 40, workingArea.Top + 20);

        LoadSubscriptions();

        // 启动时自动刷新VPN数据
        _ = RefreshVPNDataAsync();
    }

    private async System.Threading.Tasks.Task RefreshVPNDataAsync()
    {
        var vpnSubscriptions = _subscriptions.OfType<VPNSubscription>().Where(s => !string.IsNullOrEmpty(s.Url)).ToList();

        if (!vpnSubscriptions.Any())
        {
            return;
        }

        for (int i = 0; i < _subscriptions.Count; i++)
        {
            if (_subscriptions[i] is VPNSubscription vpn && !string.IsNullOrEmpty(vpn.Url))
            {
                try
                {
                    var updated = await _vpnService.FetchSubscriptionDataAsync(vpn.Url, vpn.Name);
                    updated.Id = vpn.Id;
                    _subscriptions[i] = updated;
                }
                catch
                {
                    // 静默失败，不显示错误
                }
            }
        }

        // 保存更新后的数据
        _configService.SaveSubscriptions(_subscriptions);

        // 更新显示
        this.BeginInvoke(() =>
        {
            foreach (var panel in _subscriptionPanels)
            {
                panel.UpdateDisplay();
            }
        });
    }

    private void LoadSubscriptions()
    {
        _subscriptions = _configService.LoadSubscriptions();
        SortSubscriptions(); // 应用默认排序
        CreateSubscriptionPanels();
        UpdateWindowSize();
    }

    private void SortSubscriptions()
    {
        _subscriptions = _subscriptions.OrderBy(s => GetRemainingDays(s)).ToList();
        if (!_sortAscending)
        {
            _subscriptions.Reverse();
        }
    }

    private int GetRemainingDays(SubscriptionItem subscription)
    {
        DateTime? expiryDate = subscription switch
        {
            RegularSubscription regular => regular.ExpiryDate,
            VPNSubscription vpn => vpn.ExpiryDate,
            _ => null
        };

        if (!expiryDate.HasValue)
        {
            return int.MaxValue; // 没有到期时间的排在最后
        }

        var remaining = expiryDate.Value - DateTime.Now;
        return (int)remaining.TotalDays;
    }

    private void CreateSubscriptionPanels()
    {
        const int margin = 10; // 统一边距

        // 清除现有面板
        foreach (var panel in _subscriptionPanels)
        {
            panel.Dispose();
        }
        _subscriptionPanels.Clear();
        panelSubscriptions.Controls.Clear();

        // 创建新面板
        int yOffset = margin;
        foreach (var sub in _subscriptions)
        {
            var panel = new SubscriptionPanel(sub);
            panel.Location = new Point(margin, yOffset);
            panelSubscriptions.Controls.Add(panel);
            _subscriptionPanels.Add(panel);
            yOffset += panel.Height + margin;
        }
    }

    private void UpdateWindowSize()
    {
        const int margin = 10; // 统一边距
        int totalHeight = _subscriptionPanels.Sum(p => p.Height) + (_subscriptionPanels.Count + 1) * margin;
        int maxWidth = _subscriptionPanels.Any() ? _subscriptionPanels.Max(p => p.Width) + margin * 2 : 320 + margin * 2;

        this.ClientSize = new Size(maxWidth, Math.Max(totalHeight, 100));
        panelSubscriptions.ClientSize = new Size(maxWidth, totalHeight);
    }

    private void PanelSubscriptions_MouseDown(object? sender, MouseEventArgs e)
    {
        _mouseOffset = new Point(e.X, e.Y);

        if (e.Button == MouseButtons.Right)
        {
            UpdateContextMenu();
            contextMenu.Show(this, e.Location);
        }
    }

    private void PanelSubscriptions_MouseMove(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            this.Location = new Point(this.Left + e.X - _mouseOffset.X,
                                      this.Top + e.Y - _mouseOffset.Y);
        }
    }

    private void PanelSubscriptions_MouseDoubleClick(object? sender, MouseEventArgs e)
    {
        OpenManageForm();
    }

    // 供 SubscriptionPanel 调用的公共方法
    public void OnPanelDrag(MouseEventArgs e)
    {
        PanelSubscriptions_MouseMove(null, e);
    }

    public void OnPanelDoubleClick()
    {
        OpenManageForm();
    }

    public void InvokeDrag(int deltaX, int deltaY)
    {
        this.Location = new Point(this.Left + deltaX, this.Top + deltaY);
    }

    public void InvokeMouseDown(MouseButtons button, int clicks, int x, int y)
    {
        _mouseOffset = new Point(x, y);
        if (button == MouseButtons.Right)
        {
            UpdateContextMenu();
            contextMenu.Show(this, new Point(x, y));
        }
    }

    private void UpdateContextMenu()
    {
        menuItemMinimize.Visible = !_isMinimized;
        menuItemRestore.Visible = _isMinimized;
    }

    private void MainForm_Resize(object sender, EventArgs e)
    {
        // 不再自动隐藏到托盘，因为程序已经在托盘中
        UpdateContextMenu();
    }

    private void MinimizeToTray()
    {
        // 隐藏窗口
        _isMinimized = true;
        this.Hide();
        UpdateContextMenu();
    }

    private void RestoreFromTray()
    {
        // 显示窗口
        _isMinimized = false;
        this.Show();
        this.WindowState = FormWindowState.Normal;
        UpdateContextMenu();
    }

    private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
    {
        RestoreFromTray();
    }

    private void MenuItemManage_Click(object? sender, EventArgs e)
    {
        OpenManageForm();
    }

    private void MenuItemRefresh_Click(object? sender, EventArgs e)
    {
        _ = RefreshVPNDataAsync();
    }

    private void MenuItemAutoStart_Click(object? sender, EventArgs e)
    {
        SetAutoStart(menuItemAutoStart.Checked);
    }

    private void MenuItemSort_Click(object? sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem item)
        {
            _sortAscending = item == menuItemSortAsc;
            SortSubscriptions();
            CreateSubscriptionPanels();
            UpdateWindowSize();
        }
    }

    private void SetAutoStart(bool enable)
    {
        try
        {
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyName, true))
            {
                if (enable)
                {
                    // 添加到自启动
                    string exePath = Application.ExecutablePath;
                    key?.SetValue(AppName, exePath);
                }
                else
                {
                    // 从自启动中移除
                    key?.DeleteValue(AppName, false);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"设置自启动失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            menuItemAutoStart.Checked = !enable; // 恢复状态
        }
    }

    private void OpenManageForm()
    {
        using var manageForm = new Forms.SubscriptionManagerForm(_configService);
        if (manageForm.ShowDialog() == DialogResult.OK)
        {
            LoadSubscriptions();
        }
    }

    private void MenuItemMinimize_Click(object? sender, EventArgs e)
    {
        MinimizeToTray();
    }

    private void MenuItemRestore_Click(object? sender, EventArgs e)
    {
        RestoreFromTray();
    }

    private void MenuItemExit_Click(object? sender, EventArgs e)
    {
        // TimerService 是单例，不在此处释放
        notifyIcon?.Dispose();
        Application.Exit();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
            // TimerService 是单例，不在此处释放
            notifyIcon?.Dispose();
            foreach (var panel in _subscriptionPanels)
            {
                panel.Dispose();
            }
        }
        base.Dispose(disposing);
    }
}

/// <summary>
/// 订阅显示面板
/// </summary>
internal class SubscriptionPanel : Panel
{
    private readonly SubscriptionItem _subscription;
    private readonly Label _lblName;
    private readonly Label _lblInfo1;
    private readonly Label _lblInfo2;
    private readonly Label _lblCountdown;
    private Point _dragStartPoint;
    private bool _isDragging;

    public SubscriptionPanel(SubscriptionItem subscription)
    {
        _subscription = subscription;
        this.BackColor = Color.FromArgb(40, 40, 40);
        this.Size = new Size(320, 90);
        this.Padding = new Padding(10);

        // 名称：左上角，加粗白色
        _lblName = CreateLabel(subscription.Name, 12F, FontStyle.Bold, Color.White, new Point(10, 10));
        _lblName.AutoSize = true;

        // 剩余时间：最右侧右对齐，绿色加粗（与名称在同一行）
        _lblCountdown = CreateLabel("", 10F, FontStyle.Bold, Color.FromArgb(0, 255, 128), new Point(10, 12));
        _lblCountdown.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _lblCountdown.AutoSize = false;
        _lblCountdown.TextAlign = ContentAlignment.MiddleRight;
        _lblCountdown.Width = 280;
        _lblCountdown.Location = new Point(30, 10);

        // 信息1：第二行左侧
        _lblInfo1 = CreateLabel("", 9F, FontStyle.Regular, Color.FromArgb(200, 200, 200), new Point(10, 38));

        // 信息2：第三行左侧（VPN剩余量或空）
        _lblInfo2 = CreateLabel("", 9F, FontStyle.Regular, Color.FromArgb(200, 200, 200), new Point(10, 60));

        this.Controls.Add(_lblName);
        this.Controls.Add(_lblCountdown);
        this.Controls.Add(_lblInfo1);
        this.Controls.Add(_lblInfo2);

        UpdateDisplay();
        AdjustHeight();
    }

    private void AdjustHeight()
    {
        // 如果信息2为空，缩小面板高度
        if (string.IsNullOrEmpty(_lblInfo2.Text))
        {
            this.Height = 70;
        }
        else
        {
            this.Height = 90;
        }
    }

    private Label CreateLabel(string text, float fontSize, FontStyle style, Color color, Point location)
    {
        var label = new Label
        {
            Text = text,
            Font = new Font("Microsoft YaHei UI", fontSize, style),
            ForeColor = color,
            Location = location,
            AutoSize = true,
            BackColor = Color.Transparent
        };

        // 让标签将鼠标事件转发给父面板
        label.MouseDown += (s, e) =>
        {
            // 将标签的坐标转换为面板坐标
            var panelLocation = label.PointToScreen(e.Location);
            var panelArgs = new MouseEventArgs(e.Button, e.Clicks, panelLocation.X - this.PointToScreen(Point.Empty).X, panelLocation.Y - this.PointToScreen(Point.Empty).Y, e.Delta);
            this.OnMouseDown(panelArgs);
        };
        label.MouseMove += (s, e) =>
        {
            var panelLocation = label.PointToScreen(e.Location);
            var panelArgs = new MouseEventArgs(e.Button, e.Clicks, panelLocation.X - this.PointToScreen(Point.Empty).X, panelLocation.Y - this.PointToScreen(Point.Empty).Y, e.Delta);
            this.OnMouseMove(panelArgs);
        };
        label.MouseUp += (s, e) =>
        {
            var panelLocation = label.PointToScreen(e.Location);
            var panelArgs = new MouseEventArgs(e.Button, e.Clicks, panelLocation.X - this.PointToScreen(Point.Empty).X, panelLocation.Y - this.PointToScreen(Point.Empty).Y, e.Delta);
            this.OnMouseUp(panelArgs);
        };
        label.MouseDoubleClick += (s, e) =>
        {
            var panelLocation = label.PointToScreen(e.Location);
            var panelArgs = new MouseEventArgs(e.Button, e.Clicks, panelLocation.X - this.PointToScreen(Point.Empty).X, panelLocation.Y - this.PointToScreen(Point.Empty).Y, e.Delta);
            this.OnMouseDoubleClick(panelArgs);
        };

        return label;
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _dragStartPoint = e.Location;
            _isDragging = true;
        }
        else if (e.Button == MouseButtons.Right)
        {
            // 右键点击传递给父控件
            if (Parent?.Parent is MainForm mainForm)
            {
                var screenPos = this.PointToScreen(e.Location);
                var formPos = mainForm.PointToClient(screenPos);
                mainForm.InvokeMouseDown(e.Button, e.Clicks, formPos.X, formPos.Y);
            }
        }
        base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_isDragging && e.Button == MouseButtons.Left)
        {
            if (Parent?.Parent is MainForm mainForm)
            {
                var deltaX = e.X - _dragStartPoint.X;
                var deltaY = e.Y - _dragStartPoint.Y;
                mainForm.InvokeDrag(deltaX, deltaY);
            }
        }
        base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _isDragging = false;
        }
        base.OnMouseUp(e);
    }

    protected override void OnMouseDoubleClick(MouseEventArgs e)
    {
        if (Parent?.Parent is MainForm mainForm)
        {
            mainForm.OnPanelDoubleClick();
        }
        base.OnMouseDoubleClick(e);
    }

    public void UpdateDisplay()
    {
        _lblName.Text = _subscription.Name;

        if (_subscription is RegularSubscription regular)
        {
            _lblInfo1.Text = $"到期: {regular.ExpiryDate:yyyy-MM-dd}";
            _lblInfo2.Text = "";
        }
        else if (_subscription is VPNSubscription vpn)
        {
            // 优先使用新的 Usage 对象
            if (vpn.Usage != null)
            {
                _lblInfo1.Text = $"已用: {vpn.Usage.UsedGB} GB / 总量: {vpn.Usage.TotalGB} GB ({vpn.Usage.UsagePercentage}%)";
                _lblInfo2.Text = $"剩余: {vpn.Usage.RemainingGB} GB";
                if (vpn.Usage.ExpireTime.HasValue)
                {
                    _lblInfo2.Text += $" | 到期: {vpn.Usage.ExpireTime:yyyy-MM-dd}";
                }
            }
            else if (vpn.CurrentUsed.HasValue && vpn.TotalQuota.HasValue)
            {
                // 兼容旧数据
                double percentage = vpn.TotalQuota.Value > 0
                    ? Math.Round((double)vpn.CurrentUsed.Value / vpn.TotalQuota.Value * 100, 1)
                    : 0;
                double remaining = vpn.TotalQuota.Value - vpn.CurrentUsed.Value;
                _lblInfo1.Text = $"已用: {vpn.CurrentUsed} GB / 总量: {vpn.TotalQuota} GB ({percentage}%)";
                _lblInfo2.Text = $"剩余: {remaining:F2} GB";
                if (vpn.ExpiryDate.HasValue)
                {
                    _lblInfo2.Text += $" | 到期: {vpn.ExpiryDate:yyyy-MM-dd}";
                }
            }
            else
            {
                _lblInfo1.Text = "点击刷新获取数据";
                _lblInfo2.Text = vpn.ExpiryDate.HasValue
                    ? $"到期: {vpn.ExpiryDate:yyyy-MM-dd}"
                    : "未设置到期时间";
            }

            if (!string.IsNullOrEmpty(vpn.ErrorMessage))
            {
                _lblInfo2.Text = $"错误: {vpn.ErrorMessage}";
                _lblInfo2.ForeColor = Color.OrangeRed;
            }
            else
            {
                _lblInfo2.ForeColor = Color.FromArgb(200, 200, 200);
            }
        }

        UpdateCountdown();
        AdjustHeight();
    }

    public void UpdateCountdown()
    {
        DateTime? expiryDate = _subscription switch
        {
            RegularSubscription regular => regular.ExpiryDate,
            VPNSubscription vpn => vpn.ExpiryDate,
            _ => null
        };

        if (!expiryDate.HasValue)
        {
            _lblCountdown.Text = "";
            return;
        }

        var remaining = expiryDate.Value - DateTime.Now;
        if (remaining.TotalSeconds <= 0)
        {
            _lblCountdown.Text = "已到期";
            _lblCountdown.ForeColor = Color.Red;
            return;
        }

        _lblCountdown.Text = $"剩余: {remaining.Days}天";
        _lblCountdown.ForeColor = Color.FromArgb(0, 255, 128);
    }
}
