namespace Reminder.Forms;

partial class SubscriptionManagerForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.panelMain = new System.Windows.Forms.Panel();
        this.panelButtons = new System.Windows.Forms.Panel();
        this.btnAdd = new System.Windows.Forms.Button();
        this.btnEdit = new System.Windows.Forms.Button();
        this.btnDelete = new System.Windows.Forms.Button();
        this.btnClose = new System.Windows.Forms.Button();
        this.listBoxSubscriptions = new System.Windows.Forms.ListBox();
        this.panelMain.SuspendLayout();
        this.panelButtons.SuspendLayout();
        this.SuspendLayout();
        //
        // panelMain
        //
        this.panelMain.Controls.Add(this.panelButtons);
        this.panelMain.Controls.Add(this.listBoxSubscriptions);
        this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
        this.panelMain.Location = new System.Drawing.Point(0, 0);
        this.panelMain.Name = "panelMain";
        this.panelMain.Size = new System.Drawing.Size(500, 450);
        this.panelMain.TabIndex = 0;
        //
        // panelButtons
        //
        this.panelButtons.Controls.Add(this.btnClose);
        this.panelButtons.Controls.Add(this.btnDelete);
        this.panelButtons.Controls.Add(this.btnEdit);
        this.panelButtons.Controls.Add(this.btnAdd);
        this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.panelButtons.Location = new System.Drawing.Point(0, 380);
        this.panelButtons.Name = "panelButtons";
        this.panelButtons.Size = new System.Drawing.Size(500, 70);
        this.panelButtons.TabIndex = 3;
        //
        // btnAdd
        //
        this.btnAdd.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.btnAdd.Location = new System.Drawing.Point(30, 15);
        this.btnAdd.Name = "btnAdd";
        this.btnAdd.Size = new System.Drawing.Size(100, 40);
        this.btnAdd.TabIndex = 0;
        this.btnAdd.Text = "添加";
        this.btnAdd.UseVisualStyleBackColor = true;
        this.btnAdd.Click += new System.EventHandler(this.BtnAdd_Click);
        //
        // btnEdit
        //
        this.btnEdit.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.btnEdit.Location = new System.Drawing.Point(140, 15);
        this.btnEdit.Name = "btnEdit";
        this.btnEdit.Size = new System.Drawing.Size(100, 40);
        this.btnEdit.TabIndex = 1;
        this.btnEdit.Text = "编辑";
        this.btnEdit.UseVisualStyleBackColor = true;
        this.btnEdit.Click += new System.EventHandler(this.BtnEdit_Click);
        //
        // btnDelete
        //
        this.btnDelete.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.btnDelete.Location = new System.Drawing.Point(250, 15);
        this.btnDelete.Name = "btnDelete";
        this.btnDelete.Size = new System.Drawing.Size(100, 40);
        this.btnDelete.TabIndex = 2;
        this.btnDelete.Text = "删除";
        this.btnDelete.UseVisualStyleBackColor = true;
        this.btnDelete.Click += new System.EventHandler(this.BtnDelete_Click);
        //
        // btnClose
        //
        this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.btnClose.Location = new System.Drawing.Point(360, 15);
        this.btnClose.Name = "btnClose";
        this.btnClose.Size = new System.Drawing.Size(100, 40);
        this.btnClose.TabIndex = 3;
        this.btnClose.Text = "确认";
        this.btnClose.UseVisualStyleBackColor = true;
        this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
        //
        // listBoxSubscriptions
        //
        this.listBoxSubscriptions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.listBoxSubscriptions.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
        this.listBoxSubscriptions.FormattingEnabled = true;
        this.listBoxSubscriptions.ItemHeight = 20;
        this.listBoxSubscriptions.Location = new System.Drawing.Point(15, 15);
        this.listBoxSubscriptions.Name = "listBoxSubscriptions";
        this.listBoxSubscriptions.Size = new System.Drawing.Size(450, 344);
        this.listBoxSubscriptions.TabIndex = 0;
        this.listBoxSubscriptions.DoubleClick += new System.EventHandler(this.ListBoxSubscriptions_DoubleClick);
        //
        // SubscriptionManagerForm
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(460, 450);
        this.Controls.Add(this.panelMain);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "SubscriptionManagerForm";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "订阅管理";
        this.Load += new System.EventHandler(this.SubscriptionManagerForm_Load);
        this.panelMain.ResumeLayout(false);
        this.panelButtons.ResumeLayout(false);
        this.ResumeLayout(false);

    }

    private System.Windows.Forms.Panel panelMain;
    private System.Windows.Forms.Panel panelButtons;
    private System.Windows.Forms.Button btnAdd;
    private System.Windows.Forms.Button btnEdit;
    private System.Windows.Forms.Button btnDelete;
    private System.Windows.Forms.Button btnClose;
    private System.Windows.Forms.ListBox listBoxSubscriptions;
}
