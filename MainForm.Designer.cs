namespace Reminder;

partial class MainForm
{
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.panelSubscriptions = new System.Windows.Forms.Panel();
        this.contextMenu = new System.Windows.Forms.ContextMenuStrip();
        this.menuItemManage = new System.Windows.Forms.ToolStripMenuItem();
        this.menuItemRefresh = new System.Windows.Forms.ToolStripMenuItem();
        this.menuItemSort = new System.Windows.Forms.ToolStripMenuItem();
        this.menuItemSortAsc = new System.Windows.Forms.ToolStripMenuItem();
        this.menuItemSortDesc = new System.Windows.Forms.ToolStripMenuItem();
        this.menuItemAutoStart = new System.Windows.Forms.ToolStripMenuItem();
        this.menuItemSep2 = new System.Windows.Forms.ToolStripSeparator();
        this.menuItemMinimize = new System.Windows.Forms.ToolStripMenuItem();
        this.menuItemRestore = new System.Windows.Forms.ToolStripMenuItem();
        this.menuItemSep1 = new System.Windows.Forms.ToolStripSeparator();
        this.menuItemExit = new System.Windows.Forms.ToolStripMenuItem();
        this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
        this.contextMenu.SuspendLayout();
        this.panelSubscriptions.SuspendLayout();
        this.SuspendLayout();

        //
        // panelSubscriptions
        //
        this.panelSubscriptions.AutoScroll = false;
        this.panelSubscriptions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
        this.panelSubscriptions.Dock = System.Windows.Forms.DockStyle.Fill;
        this.panelSubscriptions.Location = new System.Drawing.Point(0, 0);
        this.panelSubscriptions.Name = "panelSubscriptions";
        this.panelSubscriptions.Size = new System.Drawing.Size(320, 200);
        this.panelSubscriptions.TabIndex = 0;
        //
        // contextMenu
        //
        this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.menuItemManage,
        this.menuItemRefresh,
        this.menuItemSort,
        this.menuItemAutoStart,
        this.menuItemSep2,
        this.menuItemMinimize,
        this.menuItemRestore,
        this.menuItemSep1,
        this.menuItemExit});
        this.contextMenu.Name = "contextMenu";
        this.contextMenu.Size = new System.Drawing.Size(137, 98);
        //
        // menuItemManage
        //
        this.menuItemManage.Name = "menuItemManage";
        this.menuItemManage.Size = new System.Drawing.Size(136, 22);
        this.menuItemManage.Text = "管理订阅";
        this.menuItemManage.Click += new System.EventHandler(this.MenuItemManage_Click);
        //
        // menuItemRefresh
        //
        this.menuItemRefresh.Name = "menuItemRefresh";
        this.menuItemRefresh.Size = new System.Drawing.Size(136, 22);
        this.menuItemRefresh.Text = "刷新数据";
        this.menuItemRefresh.Click += new System.EventHandler(this.MenuItemRefresh_Click);
        //
        // menuItemSort
        //
        this.menuItemSort.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.menuItemSortAsc,
        this.menuItemSortDesc});
        this.menuItemSort.Name = "menuItemSort";
        this.menuItemSort.Size = new System.Drawing.Size(136, 22);
        this.menuItemSort.Text = "剩余时间排序";
        //
        // menuItemSortAsc
        //
        this.menuItemSortAsc.Name = "menuItemSortAsc";
        this.menuItemSortAsc.Size = new System.Drawing.Size(124, 22);
        this.menuItemSortAsc.Text = "从小到大";
        this.menuItemSortAsc.Click += new System.EventHandler(this.MenuItemSort_Click);
        //
        // menuItemSortDesc
        //
        this.menuItemSortDesc.Name = "menuItemSortDesc";
        this.menuItemSortDesc.Size = new System.Drawing.Size(124, 22);
        this.menuItemSortDesc.Text = "从大到小";
        this.menuItemSortDesc.Click += new System.EventHandler(this.MenuItemSort_Click);
        //
        // menuItemAutoStart
        //
        this.menuItemAutoStart.Name = "menuItemAutoStart";
        this.menuItemAutoStart.Size = new System.Drawing.Size(136, 22);
        this.menuItemAutoStart.Text = "开机自启动";
        this.menuItemAutoStart.CheckOnClick = true;
        this.menuItemAutoStart.Click += new System.EventHandler(this.MenuItemAutoStart_Click);
        //
        // menuItemMinimize
        //
        this.menuItemMinimize.Name = "menuItemMinimize";
        this.menuItemMinimize.Size = new System.Drawing.Size(136, 22);
        this.menuItemMinimize.Text = "最小化到托盘";
        this.menuItemMinimize.Click += new System.EventHandler(this.MenuItemMinimize_Click);
        //
        // menuItemRestore
        //
        this.menuItemRestore.Name = "menuItemRestore";
        this.menuItemRestore.Size = new System.Drawing.Size(136, 22);
        this.menuItemRestore.Text = "恢复窗口";
        this.menuItemRestore.Visible = false;
        this.menuItemRestore.Click += new System.EventHandler(this.MenuItemRestore_Click);
        //
        // menuItemSep2
        //
        this.menuItemSep2.Name = "menuItemSep2";
        this.menuItemSep2.Size = new System.Drawing.Size(133, 6);
        //
        // menuItemSep1
        //
        this.menuItemSep1.Name = "menuItemSep1";
        this.menuItemSep1.Size = new System.Drawing.Size(133, 6);
        //
        // menuItemExit
        //
        this.menuItemExit.Name = "menuItemExit";
        this.menuItemExit.Size = new System.Drawing.Size(136, 22);
        this.menuItemExit.Text = "退出";
        this.menuItemExit.Click += new System.EventHandler(this.MenuItemExit_Click);
        //
        // notifyIcon
        //
        this.notifyIcon.ContextMenuStrip = this.contextMenu;
        this.notifyIcon.Text = "Reminder";
        this.notifyIcon.Visible = true;
        this.notifyIcon.DoubleClick += new System.EventHandler(this.NotifyIcon_DoubleClick);
        //
        // MainForm
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.Lime;
        this.ClientSize = new System.Drawing.Size(320, 200);
        this.Controls.Add(this.panelSubscriptions);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        this.Name = "MainForm";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
        this.Text = "Reminder";
        this.TopMost = true;
        this.TransparencyKey = System.Drawing.Color.Lime;
        this.Load += new System.EventHandler(this.MainForm_Load);
        this.Resize += new System.EventHandler(this.MainForm_Resize);
        this.panelSubscriptions.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PanelSubscriptions_MouseDoubleClick);
        this.panelSubscriptions.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PanelSubscriptions_MouseDown);
        this.panelSubscriptions.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PanelSubscriptions_MouseMove);
        this.contextMenu.ResumeLayout(false);
        this.panelSubscriptions.ResumeLayout(false);
        this.ResumeLayout(false);

    }

    private System.Windows.Forms.Panel panelSubscriptions;
    private System.Windows.Forms.ContextMenuStrip contextMenu;
    private System.Windows.Forms.ToolStripMenuItem menuItemManage;
    private System.Windows.Forms.ToolStripMenuItem menuItemRefresh;
    private System.Windows.Forms.ToolStripMenuItem menuItemSort;
    private System.Windows.Forms.ToolStripMenuItem menuItemSortAsc;
    private System.Windows.Forms.ToolStripMenuItem menuItemSortDesc;
    private System.Windows.Forms.ToolStripMenuItem menuItemAutoStart;
    private System.Windows.Forms.ToolStripMenuItem menuItemMinimize;
    private System.Windows.Forms.ToolStripMenuItem menuItemRestore;
    private System.Windows.Forms.ToolStripSeparator menuItemSep2;
    private System.Windows.Forms.ToolStripSeparator menuItemSep1;
    private System.Windows.Forms.ToolStripMenuItem menuItemExit;
    private System.Windows.Forms.NotifyIcon notifyIcon;
}
