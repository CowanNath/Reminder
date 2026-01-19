using System.Text.Json;
using Reminder.Models;

namespace Reminder.Services;

public class ConfigService
{
    private readonly string _configPath;

    // 复用 JsonSerializerOptions，避免重复创建
    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new SubscriptionItemConverter() }
    };

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true,
        Converters = { new SubscriptionItemConverter() }
    };

    public ConfigService()
    {
        _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
    }

    public List<SubscriptionItem> LoadSubscriptions()
    {
        if (!File.Exists(_configPath))
        {
            var defaultList = CreateDefaultSubscriptions();
            SaveSubscriptions(defaultList);
            return defaultList;
        }

        try
        {
            var json = File.ReadAllText(_configPath);
            var data = JsonSerializer.Deserialize<List<SubscriptionItem>>(json, ReadOptions);
            return data ?? CreateDefaultSubscriptions();
        }
        catch
        {
            return CreateDefaultSubscriptions();
        }
    }

    public void SaveSubscriptions(List<SubscriptionItem> subscriptions)
    {
        var json = JsonSerializer.Serialize(subscriptions, WriteOptions);
        File.WriteAllText(_configPath, json);
    }

    private static List<SubscriptionItem> CreateDefaultSubscriptions()
    {
        return new List<SubscriptionItem>
        {
            new RegularSubscription
            {
                Name = "示例订阅",
                CurrentSubscriptions = 100,
                TotalSubscriptions = 1000,
                ExpiryDate = DateTime.Now.AddDays(30)
            }
        };
    }
}

/// <summary>
/// 订阅项JSON转换器
/// </summary>
public class SubscriptionItemConverter : System.Text.Json.Serialization.JsonConverter<SubscriptionItem>
{
    public override SubscriptionItem? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("Type", out var typeProp))
        {
            return null;
        }

        var typeStr = typeProp.GetString();
        var type = (SubscriptionType)Enum.Parse(typeof(SubscriptionType), typeStr!);

        return type switch
        {
            SubscriptionType.Regular => DeserializeRegularSubscription(root),
            SubscriptionType.VPN => DeserializeVPNSubscription(root),
            _ => null
        };
    }

    private static RegularSubscription DeserializeRegularSubscription(JsonElement root)
    {
        return new RegularSubscription
        {
            Id = root.TryGetProperty("Id", out var id) ? id.GetString() ?? Guid.NewGuid().ToString() : Guid.NewGuid().ToString(),
            Name = root.TryGetProperty("Name", out var name) ? name.GetString() ?? "" : "",
            CurrentSubscriptions = root.TryGetProperty("CurrentSubscriptions", out var current) ? current.GetInt32() : 0,
            TotalSubscriptions = root.TryGetProperty("TotalSubscriptions", out var total) ? total.GetInt32() : 0,
            ExpiryDate = root.TryGetProperty("ExpiryDate", out var expiry) && DateTime.TryParse(expiry.GetString(), out var expiryDate) ? expiryDate : DateTime.Now
        };
    }

    private static VPNSubscription DeserializeVPNSubscription(JsonElement root)
    {
        var vpn = new VPNSubscription
        {
            Id = root.TryGetProperty("Id", out var id) ? id.GetString() ?? Guid.NewGuid().ToString() : Guid.NewGuid().ToString(),
            Name = root.TryGetProperty("Name", out var name) ? name.GetString() ?? "" : "",
            Url = root.TryGetProperty("Url", out var url) ? url.GetString() ?? "" : "",
            CurrentUsed = root.TryGetProperty("CurrentUsed", out var current) ? current.GetInt32() : null,
            TotalQuota = root.TryGetProperty("TotalQuota", out var total) ? total.GetInt32() : null,
            ErrorMessage = root.TryGetProperty("ErrorMessage", out var error) ? error.GetString() : null
        };

        if (root.TryGetProperty("ExpiryDate", out var expiry) && DateTime.TryParse(expiry.GetString(), out var expiryDate))
        {
            vpn.ExpiryDate = expiryDate;
        }

        // 反序列化 Usage 对象
        if (root.TryGetProperty("Usage", out var usageElement) && usageElement.ValueKind == JsonValueKind.Object)
        {
            vpn.Usage = new SubscriptionUsage
            {
                UploadBytes = usageElement.TryGetProperty("UploadBytes", out var upload) ? upload.GetInt64() : 0,
                DownloadBytes = usageElement.TryGetProperty("DownloadBytes", out var download) ? download.GetInt64() : 0,
                TotalBytes = usageElement.TryGetProperty("TotalBytes", out var totalBytes) ? totalBytes.GetInt64() : 0,
                ExpireTime = usageElement.TryGetProperty("ExpireTime", out var expireTime) && DateTime.TryParse(expireTime.GetString(), out var expTime) ? expTime : null
            };
        }

        return vpn;
    }

    public override void Write(Utf8JsonWriter writer, SubscriptionItem value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        // 写入 Type 属性
        writer.WriteString("Type", value.Type switch
        {
            SubscriptionType.Regular => "Regular",
            SubscriptionType.VPN => "VPN",
            _ => "Regular"
        });

        // 写入 Id 属性
        writer.WriteString("Id", value.Id);
        writer.WriteString("Name", value.Name);

        // 根据类型写入特定属性
        if (value is RegularSubscription regular)
        {
            writer.WriteNumber("CurrentSubscriptions", regular.CurrentSubscriptions);
            writer.WriteNumber("TotalSubscriptions", regular.TotalSubscriptions);
            writer.WriteString("ExpiryDate", regular.ExpiryDate.ToString("O"));
        }
        else if (value is VPNSubscription vpn)
        {
            writer.WriteString("Url", vpn.Url);

            if (vpn.CurrentUsed.HasValue)
                writer.WriteNumber("CurrentUsed", vpn.CurrentUsed.Value);

            if (vpn.TotalQuota.HasValue)
                writer.WriteNumber("TotalQuota", vpn.TotalQuota.Value);

            if (vpn.ExpiryDate.HasValue)
                writer.WriteString("ExpiryDate", vpn.ExpiryDate.Value.ToString("O"));

            if (vpn.ErrorMessage != null)
                writer.WriteString("ErrorMessage", vpn.ErrorMessage);

            // 序列化 Usage 对象
            if (vpn.Usage != null)
            {
                writer.WritePropertyName("Usage");
                writer.WriteStartObject();
                writer.WriteNumber("UploadBytes", vpn.Usage.UploadBytes);
                writer.WriteNumber("DownloadBytes", vpn.Usage.DownloadBytes);
                writer.WriteNumber("TotalBytes", vpn.Usage.TotalBytes);
                if (vpn.Usage.ExpireTime.HasValue)
                    writer.WriteString("ExpireTime", vpn.Usage.ExpireTime.Value.ToString("O"));
                writer.WriteEndObject();
            }
        }

        writer.WriteEndObject();
    }
}
