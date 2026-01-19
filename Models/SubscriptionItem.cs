namespace Reminder.Models;

/// <summary>
/// 订阅流量使用信息
/// </summary>
public class SubscriptionUsage
{
    /// <summary>
    /// 上传流量（字节）
    /// </summary>
    public long UploadBytes { get; set; }

    /// <summary>
    /// 下载流量（字节）
    /// </summary>
    public long DownloadBytes { get; set; }

    /// <summary>
    /// 总流量（字节）
    /// </summary>
    public long TotalBytes { get; set; }

    /// <summary>
    /// 到期时间（Unix 时间戳，可能为 null）
    /// </summary>
    public DateTime? ExpireTime { get; set; }

    /// <summary>
    /// 已用流量（字节）= 上传 + 下载
    /// </summary>
    public long UsedBytes => UploadBytes + DownloadBytes;

    /// <summary>
    /// 剩余流量（字节）= 总量 - 已用
    /// </summary>
    public long RemainingBytes => TotalBytes - UsedBytes;

    /// <summary>
    /// 已用流量（GB）
    /// </summary>
    public double UsedGB => Math.Round(UsedBytes / 1024d / 1024d / 1024d, 2);

    /// <summary>
    /// 总流量（GB）
    /// </summary>
    public double TotalGB => Math.Round(TotalBytes / 1024d / 1024d / 1024d, 2);

    /// <summary>
    /// 剩余流量（GB）
    /// </summary>
    public double RemainingGB => Math.Round(RemainingBytes / 1024d / 1024d / 1024d, 2);

    /// <summary>
    /// 流量使用百分比
    /// </summary>
    public double UsagePercentage => TotalBytes > 0 ? Math.Round((double)UsedBytes / TotalBytes * 100, 2) : 0;
}

/// <summary>
/// 订阅类型枚举
/// </summary>
public enum SubscriptionType
{
    /// <summary>
    /// 常规订阅配置（手动输入）
    /// </summary>
    Regular,

    /// <summary>
    /// VPN订阅配置（通过URL获取）
    /// </summary>
    VPN
}

/// <summary>
/// 订阅项基类
/// </summary>
public abstract class SubscriptionItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public SubscriptionType Type { get; set; }
    public string Name { get; set; } = "新订阅";
}

/// <summary>
/// 常规订阅配置
/// </summary>
public class RegularSubscription : SubscriptionItem
{
    public int CurrentSubscriptions { get; set; }
    public int TotalSubscriptions { get; set; }
    public DateTime ExpiryDate { get; set; }

    public RegularSubscription()
    {
        Type = SubscriptionType.Regular;
    }
}

/// <summary>
/// VPN订阅配置
/// </summary>
public class VPNSubscription : SubscriptionItem
{
    public string Url { get; set; } = "";

    /// <summary>
    /// 已用流量（GB）- 保留用于兼容性
    /// </summary>
    public int? CurrentUsed { get; set; }

    /// <summary>
    /// 总流量（GB）- 保留用于兼容性
    /// </summary>
    public int? TotalQuota { get; set; }

    /// <summary>
    /// 到期日期
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// 错误消息（如果有）
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 详细的流量使用信息（从订阅 URL 响应头获取）
    /// </summary>
    public SubscriptionUsage? Usage { get; set; }

    /// <summary>
    /// 流量使用百分比
    /// </summary>
    public double? UsagePercentage
    {
        get
        {
            if (Usage != null && Usage.TotalBytes > 0)
                return Usage.UsagePercentage;

            if (TotalQuota.HasValue && TotalQuota.Value > 0 && CurrentUsed.HasValue)
                return Math.Round((double)CurrentUsed.Value / TotalQuota.Value * 100, 2);

            return null;
        }
    }

    /// <summary>
    /// 剩余流量（GB）
    /// </summary>
    public double? RemainingGB
    {
        get
        {
            if (Usage != null)
                return Usage.RemainingGB;

            if (TotalQuota.HasValue && CurrentUsed.HasValue)
                return TotalQuota.Value - CurrentUsed.Value;

            return null;
        }
    }

    public VPNSubscription()
    {
        Type = SubscriptionType.VPN;
    }
}
