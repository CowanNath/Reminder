using Reminder.Models;

namespace Reminder.Forms;

public partial class SubscriptionEditForm : Form
{
    private SubscriptionItem? _editingItem;

    public SubscriptionEditForm()
    {
        InitializeComponent();
    }

    public SubscriptionEditForm(SubscriptionItem item) : this()
    {
        _editingItem = item;
        Text = "编辑订阅";
    }

    private void SubscriptionEditForm_Load(object sender, EventArgs e)
    {
        if (_editingItem != null)
        {
            txtName.Text = _editingItem.Name;
            cmbType.SelectedIndex = _editingItem.Type == SubscriptionType.Regular ? 0 : 1;

            if (_editingItem is RegularSubscription regular)
            {
                dtpExpiryDate.Value = regular.ExpiryDate;
            }
            else if (_editingItem is VPNSubscription vpn)
            {
                txtUrl.Text = vpn.Url ?? "";
            }
        }
        else
        {
            cmbType.SelectedIndex = 0;
            dtpExpiryDate.Value = DateTime.Now.AddDays(30);
        }
    }

    private void CmbType_SelectedIndexChanged(object sender, EventArgs e)
    {
        panelRegular.Visible = cmbType.SelectedIndex == 0;
        panelVPN.Visible = cmbType.SelectedIndex == 1;
    }

    private void BtnOK_Click(object sender, EventArgs e)
    {
        if (!ValidateInput())
        {
            return;
        }

        this.DialogResult = DialogResult.OK;
        this.Close();
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("请输入名称", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtName.Focus();
            return false;
        }

        if (cmbType.SelectedIndex == 0) // 常规订阅
        {
            // DateTimePicker 总是有有效值，不需要验证
        }
        else // VPN订阅
        {
            if (string.IsNullOrWhiteSpace(txtUrl.Text))
            {
                MessageBox.Show("请输入URL", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUrl.Focus();
                return false;
            }

            if (!Uri.TryCreate(txtUrl.Text, UriKind.Absolute, out _))
            {
                MessageBox.Show("请输入有效的URL", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUrl.Focus();
                return false;
            }
        }

        return true;
    }

    public SubscriptionItem CreateSubscription()
    {
        if (cmbType.SelectedIndex == 0) // 常规订阅
        {
            return new RegularSubscription
            {
                Name = txtName.Text,
                CurrentSubscriptions = 0,
                TotalSubscriptions = 0,
                ExpiryDate = dtpExpiryDate.Value
            };
        }
        else // VPN订阅
        {
            return new VPNSubscription
            {
                Name = txtName.Text,
                Url = txtUrl.Text
            };
        }
    }
}
