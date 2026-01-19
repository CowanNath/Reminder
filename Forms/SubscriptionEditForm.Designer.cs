namespace Reminder.Forms;

partial class SubscriptionEditForm
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
        this.panelRegular = new System.Windows.Forms.Panel();
        this.dtpExpiryDate = new System.Windows.Forms.DateTimePicker();
        this.lblExpiryDate = new System.Windows.Forms.Label();
        this.panelVPN = new System.Windows.Forms.Panel();
        this.txtUrl = new System.Windows.Forms.TextBox();
        this.lblUrl = new System.Windows.Forms.Label();
        this.panelButtons = new System.Windows.Forms.Panel();
        this.btnOK = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.cmbType = new System.Windows.Forms.ComboBox();
        this.lblType = new System.Windows.Forms.Label();
        this.txtName = new System.Windows.Forms.TextBox();
        this.lblName = new System.Windows.Forms.Label();
        this.panelRegular.SuspendLayout();
        this.panelVPN.SuspendLayout();
        this.panelButtons.SuspendLayout();
        this.SuspendLayout();
        //
        // panelRegular
        //
        this.panelRegular.Controls.Add(this.dtpExpiryDate);
        this.panelRegular.Controls.Add(this.lblExpiryDate);
        this.panelRegular.Location = new System.Drawing.Point(15, 95);
        this.panelRegular.Name = "panelRegular";
        this.panelRegular.Size = new System.Drawing.Size(360, 60);
        this.panelRegular.TabIndex = 7;
        //
        // dtpExpiryDate
        //
        this.dtpExpiryDate.CustomFormat = "yyyy-MM-dd";
        this.dtpExpiryDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
        this.dtpExpiryDate.Location = new System.Drawing.Point(60, 15);
        this.dtpExpiryDate.Name = "dtpExpiryDate";
        this.dtpExpiryDate.Size = new System.Drawing.Size(120, 23);
        this.dtpExpiryDate.TabIndex = 5;
        //
        // lblExpiryDate
        //
        this.lblExpiryDate.AutoSize = true;
        this.lblExpiryDate.Location = new System.Drawing.Point(0, 18);
        this.lblExpiryDate.Name = "lblExpiryDate";
        this.lblExpiryDate.Size = new System.Drawing.Size(68, 17);
        this.lblExpiryDate.TabIndex = 4;
        this.lblExpiryDate.Text = "到期时间:";
        //
        // panelVPN
        //
        this.panelVPN.Controls.Add(this.txtUrl);
        this.panelVPN.Controls.Add(this.lblUrl);
        this.panelVPN.Location = new System.Drawing.Point(15, 95);
        this.panelVPN.Name = "panelVPN";
        this.panelVPN.Size = new System.Drawing.Size(360, 60);
        this.panelVPN.TabIndex = 6;
        this.panelVPN.Visible = false;
        //
        // txtUrl
        //
        this.txtUrl.Location = new System.Drawing.Point(60, 15);
        this.txtUrl.Name = "txtUrl";
        this.txtUrl.Size = new System.Drawing.Size(280, 23);
        this.txtUrl.TabIndex = 1;
        //
        // lblUrl
        //
        this.lblUrl.AutoSize = true;
        this.lblUrl.Location = new System.Drawing.Point(0, 18);
        this.lblUrl.Name = "lblUrl";
        this.lblUrl.Size = new System.Drawing.Size(32, 17);
        this.lblUrl.TabIndex = 0;
        this.lblUrl.Text = "URL:";
        //
        // panelButtons
        //
        this.panelButtons.Controls.Add(this.btnCancel);
        this.panelButtons.Controls.Add(this.btnOK);
        this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.panelButtons.Location = new System.Drawing.Point(0, 200);
        this.panelButtons.Name = "panelButtons";
        this.panelButtons.Size = new System.Drawing.Size(400, 60);
        this.panelButtons.TabIndex = 5;
        //
        // btnOK
        //
        this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.btnOK.Location = new System.Drawing.Point(20, 15);
        this.btnOK.Name = "btnOK";
        this.btnOK.Size = new System.Drawing.Size(90, 35);
        this.btnOK.TabIndex = 1;
        this.btnOK.Text = "确定";
        this.btnOK.UseVisualStyleBackColor = true;
        this.btnOK.Click += new System.EventHandler(this.BtnOK_Click);
        //
        // btnCancel
        //
        this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.btnCancel.Location = new System.Drawing.Point(120, 15);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(90, 35);
        this.btnCancel.TabIndex = 0;
        this.btnCancel.Text = "取消";
        this.btnCancel.UseVisualStyleBackColor = true;
        //
        // cmbType
        //
        this.cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbType.FormattingEnabled = true;
        this.cmbType.Items.AddRange(new object[] {
        "常规订阅",
        "VPN订阅"});
        this.cmbType.Location = new System.Drawing.Point(75, 55);
        this.cmbType.Name = "cmbType";
        this.cmbType.Size = new System.Drawing.Size(280, 25);
        this.cmbType.TabIndex = 3;
        this.cmbType.SelectedIndexChanged += new System.EventHandler(this.CmbType_SelectedIndexChanged);
        //
        // lblType
        //
        this.lblType.AutoSize = true;
        this.lblType.Location = new System.Drawing.Point(15, 58);
        this.lblType.Name = "lblType";
        this.lblType.Size = new System.Drawing.Size(56, 17);
        this.lblType.TabIndex = 2;
        this.lblType.Text = "类型:";
        //
        // txtName
        //
        this.txtName.Location = new System.Drawing.Point(75, 20);
        this.txtName.Name = "txtName";
        this.txtName.Size = new System.Drawing.Size(280, 23);
        this.txtName.TabIndex = 1;
        //
        // lblName
        //
        this.lblName.AutoSize = true;
        this.lblName.Location = new System.Drawing.Point(15, 23);
        this.lblName.Name = "lblName";
        this.lblName.Size = new System.Drawing.Size(44, 17);
        this.lblName.TabIndex = 0;
        this.lblName.Text = "名称:";
        //
        // SubscriptionEditForm
        //
        this.AcceptButton = this.btnOK;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this.btnCancel;
        this.ClientSize = new System.Drawing.Size(400, 260);
        this.Controls.Add(this.panelRegular);
        this.Controls.Add(this.panelVPN);
        this.Controls.Add(this.panelButtons);
        this.Controls.Add(this.cmbType);
        this.Controls.Add(this.lblType);
        this.Controls.Add(this.txtName);
        this.Controls.Add(this.lblName);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "SubscriptionEditForm";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "编辑订阅";
        this.Load += new System.EventHandler(this.SubscriptionEditForm_Load);
        this.panelRegular.ResumeLayout(false);
        this.panelRegular.PerformLayout();
        this.panelVPN.ResumeLayout(false);
        this.panelVPN.PerformLayout();
        this.panelButtons.ResumeLayout(false);
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    private System.Windows.Forms.Panel panelRegular;
    private System.Windows.Forms.DateTimePicker dtpExpiryDate;
    private System.Windows.Forms.Label lblExpiryDate;
    private System.Windows.Forms.Panel panelVPN;
    private System.Windows.Forms.TextBox txtUrl;
    private System.Windows.Forms.Label lblUrl;
    private System.Windows.Forms.Panel panelButtons;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.ComboBox cmbType;
    private System.Windows.Forms.Label lblType;
    private System.Windows.Forms.TextBox txtName;
    private System.Windows.Forms.Label lblName;
}
