using Reminder.Models;
using Reminder.Services;

namespace Reminder.Forms;

public partial class SubscriptionManagerForm : Form
{
    private readonly ConfigService _configService;
    private readonly VPNSubscriptionService _vpnService;
    private List<SubscriptionItem> _subscriptions;

    public SubscriptionManagerForm(ConfigService configService)
    {
        InitializeComponent();
        _configService = configService;
        _vpnService = new VPNSubscriptionService();
        _subscriptions = _configService.LoadSubscriptions();
    }

    private void SubscriptionManagerForm_Load(object sender, EventArgs e)
    {
        RefreshList();
    }

    private void RefreshList()
    {
        listBoxSubscriptions.Items.Clear();
        foreach (var sub in _subscriptions)
        {
            string displayText = sub.Type switch
            {
                SubscriptionType.Regular => $"[常规] {sub.Name}",
                SubscriptionType.VPN => $"[VPN] {sub.Name}",
                _ => sub.Name
            };
            listBoxSubscriptions.Items.Add(displayText);
        }
    }

    private void BtnAdd_Click(object sender, EventArgs e)
    {
        using var form = new SubscriptionEditForm();
        if (form.ShowDialog() == DialogResult.OK)
        {
            var newSub = form.CreateSubscription();
            _subscriptions.Add(newSub);
            _configService.SaveSubscriptions(_subscriptions);
            RefreshList();
        }
    }

    private void BtnEdit_Click(object sender, EventArgs e)
    {
        if (listBoxSubscriptions.SelectedIndex < 0)
        {
            MessageBox.Show("请先选择要编辑的订阅", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var sub = _subscriptions[listBoxSubscriptions.SelectedIndex];
        using var form = new SubscriptionEditForm(sub);
        if (form.ShowDialog() == DialogResult.OK)
        {
            var updatedSub = form.CreateSubscription();
            updatedSub.Id = sub.Id;
            _subscriptions[listBoxSubscriptions.SelectedIndex] = updatedSub;
            _configService.SaveSubscriptions(_subscriptions);
            RefreshList();
        }
    }

    private void ListBoxSubscriptions_DoubleClick(object sender, EventArgs e)
    {
        BtnEdit_Click(sender, e);
    }

    private void BtnDelete_Click(object sender, EventArgs e)
    {
        if (listBoxSubscriptions.SelectedIndex < 0)
        {
            MessageBox.Show("请先选择要删除的订阅", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (MessageBox.Show("确定要删除这个订阅吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            _subscriptions.RemoveAt(listBoxSubscriptions.SelectedIndex);
            _configService.SaveSubscriptions(_subscriptions);
            RefreshList();
        }
    }

    private async void BtnClose_Click(object sender, EventArgs e)
    {
        // 检查是否有 VPN 订阅需要刷新
        var vpnSubscriptions = _subscriptions.OfType<VPNSubscription>().Where(s => !string.IsNullOrEmpty(s.Url)).ToList();

        if (vpnSubscriptions.Any())
        {
            btnClose.Enabled = false;
            btnClose.Text = "刷新中...";

            int successCount = 0;
            int failCount = 0;
            var errorMessages = new List<string>();

            for (int i = 0; i < _subscriptions.Count; i++)
            {
                if (_subscriptions[i] is VPNSubscription vpn && !string.IsNullOrEmpty(vpn.Url))
                {
                    try
                    {
                        var updated = await _vpnService.FetchSubscriptionDataAsync(vpn.Url, vpn.Name);
                        updated.Id = vpn.Id;
                        _subscriptions[i] = updated;

                        if (string.IsNullOrEmpty(updated.ErrorMessage))
                        {
                            successCount++;
                        }
                        else
                        {
                            failCount++;
                            errorMessages.Add($"{vpn.Name}: {updated.ErrorMessage}");
                        }
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        errorMessages.Add($"{vpn.Name}: {ex.Message}");
                    }
                }
            }

            // 保存更新后的数据
            _configService.SaveSubscriptions(_subscriptions);
            RefreshList();

            btnClose.Enabled = true;
            btnClose.Text = "确认";

            // 显示刷新结果
            if (failCount == 0)
            {
                MessageBox.Show($"成功刷新 {successCount} 个 VPN 订阅", "刷新完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (successCount == 0)
            {
                MessageBox.Show($"刷新失败:\n{string.Join("\n", errorMessages)}", "刷新失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show($"刷新完成：成功 {successCount} 个，失败 {failCount} 个\n\n失败详情:\n{string.Join("\n", errorMessages)}",
                    "刷新结果", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        this.DialogResult = DialogResult.OK;
        this.Close();
    }
}
