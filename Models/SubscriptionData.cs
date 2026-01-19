namespace Reminder.Models;

public class SubscriptionData
{
    public int CurrentSubscriptions { get; set; }
    public int TotalSubscriptions { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string Title { get; set; } = "订阅提醒";
}
