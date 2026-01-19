using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Reminder.Models;

namespace Reminder.Services;

/// <summary>
/// VPN订阅数据获取服务 - 使用共享 HttpClient
/// </summary>
public class VPNSubscriptionService : IDisposable
{
    private static readonly HttpClient _sharedHttpClient;
    private static readonly HttpClientHandler _sharedHandler;

    static VPNSubscriptionService()
    {
        // 创建静态共享的 HttpClient，减少资源占用
        _sharedHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = ValidateServerCertificate
        };

        _sharedHttpClient = new HttpClient(_sharedHandler)
        {
            Timeout = TimeSpan.FromSeconds(15)
        };
    }

    /// <summary>
    /// SSL 证书验证回调 - 接受所有证书（用于自签名证书）
    /// </summary>
    private static bool ValidateServerCertificate(HttpRequestMessage sender, X509Certificate2? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
    {
        // 对于个人使用的工具，接受所有证书
        return true;
    }

    /// <summary>
    /// 从URL获取VPN订阅信息
    /// 支持两种模式：
    /// 1. 标准订阅URL：返回YAML配置，流量信息在响应头的 subscription-userinfo 中
    /// 2. 流量统计API URL：返回JSON格式的流量数据
    /// </summary>
    public async Task<VPNSubscription> FetchSubscriptionDataAsync(string url, string name)
    {
        var subscription = new VPNSubscription
        {
            Name = name,
            Url = url
        };

        try
        {
            // 设置 User-Agent 头，模拟 Clash 客户端（有些机场会校验）
            _sharedHttpClient.DefaultRequestHeaders.Clear();
            _sharedHttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Clash-Verge/1.5.0 (Windows)");

            var response = await _sharedHttpClient.GetAsync(url);

            // 检查响应状态
            if (!response.IsSuccessStatusCode)
            {
                subscription.ErrorMessage = $"HTTP 错误: {(int)response.StatusCode} {response.StatusCode}";
                return subscription;
            }

            // 优先：尝试从响应头获取流量信息（标准订阅URL方式）
            var userInfoHeader = FindHeaderIgnoreCase(response.Headers, "subscription-userinfo");
            if (!string.IsNullOrEmpty(userInfoHeader))
            {
                var usage = ParseSubscriptionUserInfo(userInfoHeader);
                subscription.Usage = usage;
                subscription.CurrentUsed = (int)usage.UsedGB;
                subscription.TotalQuota = (int)usage.TotalGB;
                subscription.ExpiryDate = usage.ExpireTime;
                subscription.ErrorMessage = null; // 成功
                return subscription;
            }

            // 如果响应头没有流量信息，尝试解析响应体（流量统计API方式）
            var content = await response.Content.ReadAsStringAsync();

            // 检查内容类型
            if (string.IsNullOrWhiteSpace(content))
            {
                subscription.ErrorMessage = "返回内容为空，且响应头中未找到流量信息";
                return subscription;
            }

            // 尝试检测内容类型并解析
            ParseSubscriptionContent(content, subscription);
        }
        catch (HttpRequestException ex)
        {
            subscription.ErrorMessage = $"网络错误: {ex.Message}";
        }
        catch (TaskCanceledException)
        {
            subscription.ErrorMessage = "请求超时";
        }
        catch (Exception ex)
        {
            subscription.ErrorMessage = $"解析错误: {ex.Message}";
        }

        return subscription;
    }

    /// <summary>
    /// 解析订阅内容，自动检测类型
    /// </summary>
    private void ParseSubscriptionContent(string content, VPNSubscription subscription)
    {
        // 检查是否为 JSON 格式
        if (content.TrimStart().StartsWith("{"))
        {
            ParseJsonContent(content, subscription);
            return;
        }

        // 检查是否为 YAML 格式（某些配置使用 YAML）
        if (content.TrimStart().StartsWith("proxies:") || content.TrimStart().StartsWith("mixed-port:"))
        {
            subscription.ErrorMessage = "这是 Clash 配置文件。响应头中未找到流量信息，该机场可能不支持流量统计。";
            subscription.CurrentUsed = null;
            subscription.TotalQuota = null;
            subscription.ExpiryDate = null;
            return;
        }

        // 检查是否为 Base64 编码的订阅内容
        if (IsBase64Content(content))
        {
            ParseBase64Subscription(content, subscription);
            return;
        }

        // 检查是否为 V2Ray/VLESS 等协议的订阅内容
        if (content.Contains("vmess://") || content.Contains("vless://") ||
            content.Contains("trojan://") || content.Contains("ss://"))
        {
            subscription.ErrorMessage = "这是节点订阅链接，不包含流量统计信息。请配置专门的流量统计 API URL。";
            subscription.CurrentUsed = null;
            subscription.TotalQuota = null;
            subscription.ExpiryDate = null;
            return;
        }

        subscription.ErrorMessage = "无法识别的响应格式。请确认这是流量统计 API URL。";
    }

    /// <summary>
    /// 检查是否为 Base64 编码内容
    /// </summary>
    private bool IsBase64Content(string content)
    {
        // 移除可能的前缀
        var cleanContent = content.Trim();
        if (cleanContent.StartsWith("base64://", StringComparison.OrdinalIgnoreCase))
        {
            cleanContent = cleanContent.Substring(9);
        }

        // 检查是否只包含 base64 字符
        return cleanContent.All(c => char.IsLetterOrDigit(c) || c == '+' || c == '/' || c == '=' || c == '\r' || c == '\n');
    }

    /// <summary>
    /// 解析 Base64 订阅内容
    /// </summary>
    private void ParseBase64Subscription(string content, VPNSubscription subscription)
    {
        try
        {
            // 移除可能的 base64:// 前缀
            var cleanContent = content.Trim();
            if (cleanContent.StartsWith("base64://", StringComparison.OrdinalIgnoreCase))
            {
                cleanContent = cleanContent.Substring(9);
            }

            // 解码 Base64
            var decodedBytes = Convert.FromBase64String(cleanContent);
            var decodedContent = Encoding.UTF8.GetString(decodedBytes);

            // 解码后可能是 JSON、YAML 或节点列表
            if (decodedContent.TrimStart().StartsWith("{"))
            {
                ParseJsonContent(decodedContent, subscription);
            }
            else if (decodedContent.Contains("upload") || decodedContent.Contains("download") || decodedContent.Contains("total"))
            {
                // 尝试从节点信息中提取流量数据
                ExtractTrafficFromNodes(decodedContent, subscription);
            }
            else
            {
                subscription.ErrorMessage = "这是订阅配置内容（节点列表）。要获取流量信息，请使用专门的流量统计 API。";
            }
        }
        catch (FormatException)
        {
            subscription.ErrorMessage = "Base64 解码失败，内容格式不正确";
        }
        catch (Exception ex)
        {
            subscription.ErrorMessage = $"解析订阅内容失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 尝试从节点列表中提取流量信息（某些服务商会将流量信息嵌入节点中）
    /// </summary>
    private void ExtractTrafficFromNodes(string content, VPNSubscription subscription)
    {
        try
        {
            // 尝试查找流量相关的字段
            // 例如：upload=xxx, download=xxx, total=xxx

            var patterns = new[]
            {
                @"upload=(\d+)",           // V2Ray 格式
                @"download=(\d+)",
                @"total=(\d+)",
                @"upload""\s*:\s*(\d+)",  // JSON 格式
                @"download""\s*:\s*(\d+)",
                @"total""\s*:\s*(\d+)"
            };

            long? upload = null;
            long? download = null;
            long? total = null;

            foreach (var line in content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var lowerLine = line.ToLower();
                if (lowerLine.Contains("upload") && !upload.HasValue)
                {
                    var match = System.Text.RegularExpressions.Regex.Match(line, @"\d+");
                    if (match.Success) long.TryParse(match.Value, out var val);
                }
                if (lowerLine.Contains("download") && !download.HasValue)
                {
                    var match = System.Text.RegularExpressions.Regex.Match(line, @"\d+");
                    if (match.Success) long.TryParse(match.Value, out var val);
                }
                if (lowerLine.Contains("total") && !total.HasValue)
                {
                    var match = System.Text.RegularExpressions.Regex.Match(line, @"\d+");
                    if (match.Success) long.TryParse(match.Value, out var val);
                }

                if (upload.HasValue && download.HasValue && total.HasValue)
                {
                    break;
                }
            }

            if (total.HasValue)
            {
                subscription.TotalQuota = (int)(total.Value / (1024 * 1024 * 1024));
                if (upload.HasValue && download.HasValue)
                {
                    subscription.CurrentUsed = (int)((upload.Value + download.Value) / (1024 * 1024 * 1024));
                    subscription.ErrorMessage = null; // 清除错误消息
                }
                else
                {
                    subscription.ErrorMessage = "部分流量信息获取成功（仅总量）";
                }
            }
            else
            {
                subscription.ErrorMessage = "这是订阅配置内容。要获取完整的流量统计，请使用专门的流量统计 API URL。";
            }
        }
        catch
        {
            subscription.ErrorMessage = "这是订阅配置内容（节点列表）。要获取流量信息，请使用专门的流量统计 API。";
        }
    }

    /// <summary>
    /// 解析 JSON 格式的内容
    /// </summary>
    private void ParseJsonContent(string content, VPNSubscription subscription)
    {
        try
        {
            var jsonDoc = JsonDocument.Parse(content);
            var root = jsonDoc.RootElement;

            // 格式1: 标准响应格式 { "success": true, "data": { "used": xxx, "total": xxx, "expiry": xxx } }
            if (root.TryGetProperty("success", out var success) && success.GetBoolean() &&
                root.TryGetProperty("data", out var data))
            {
                ParseStandardDataFormat(data, subscription);
                return;
            }

            // 格式2: 直接包含数据的格式 { "used": xxx, "total": xxx, "expiry": xxx }
            if (root.TryGetProperty("used", out _) || root.TryGetProperty("total", out _))
            {
                ParseDirectDataFormat(root, subscription);
                return;
            }

            // 格式3: V2Ray/Xray 格式 { "upload": 0, "download": 0, "total": 107374182400, "expire": 1234567890 }
            if (root.TryGetProperty("total", out var total) && root.TryGetProperty("expire", out var expire))
            {
                var totalBytes = total.GetInt64();
                var uploadBytes = root.TryGetProperty("upload", out var upload) ? upload.GetInt64() : 0;
                var downloadBytes = root.TryGetProperty("download", out var download) ? download.GetInt64() : 0;

                subscription.TotalQuota = (int)(totalBytes / (1024 * 1024 * 1024));
                subscription.CurrentUsed = (int)((uploadBytes + downloadBytes) / (1024 * 1024 * 1024));

                var expireTimestamp = expire.GetInt64();
                if (expireTimestamp > 0)
                {
                    if (expireTimestamp > 1000000000000)
                        subscription.ExpiryDate = DateTimeOffset.FromUnixTimeMilliseconds(expireTimestamp).DateTime;
                    else
                        subscription.ExpiryDate = DateTimeOffset.FromUnixTimeSeconds(expireTimestamp).DateTime;
                }
                subscription.ErrorMessage = null; // 成功解析
                return;
            }

            // 格式4: data_limit/data_used 格式
            if (root.TryGetProperty("data_limit", out total))
            {
                subscription.TotalQuota = total.GetInt32();

                if (root.TryGetProperty("data_used", out var used))
                {
                    subscription.CurrentUsed = used.GetInt32();
                }

                if (root.TryGetProperty("expiry", out var expiry))
                {
                    ParseExpiryDate(expiry, subscription);
                }
                subscription.ErrorMessage = null;
                return;
            }

            subscription.ErrorMessage = "无法识别的 API 响应格式。请确认返回的数据包含 used/total/expiry 字段。";
        }
        catch (JsonException ex)
        {
            subscription.ErrorMessage = $"JSON 解析失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 解析标准数据格式
    /// </summary>
    private void ParseStandardDataFormat(JsonElement data, VPNSubscription subscription)
    {
        if (data.TryGetProperty("used", out var used))
        {
            subscription.CurrentUsed = ConvertBytesToGB(used);
        }

        if (data.TryGetProperty("total", out var total))
        {
            subscription.TotalQuota = ConvertBytesToGB(total);
        }

        if (data.TryGetProperty("expiry", out var expiry))
        {
            ParseExpiryDate(expiry, subscription);
        }

        subscription.ErrorMessage = null;
    }

    /// <summary>
    /// 解析直接数据格式
    /// </summary>
    private void ParseDirectDataFormat(JsonElement root, VPNSubscription subscription)
    {
        if (root.TryGetProperty("used", out var used))
        {
            subscription.CurrentUsed = ConvertBytesToGB(used);
        }

        if (root.TryGetProperty("total", out var total))
        {
            subscription.TotalQuota = ConvertBytesToGB(total);
        }

        if (root.TryGetProperty("expiry", out var expiry))
        {
            ParseExpiryDate(expiry, subscription);
        }

        if (root.TryGetProperty("remaining_days", out var remainingDays))
        {
            var days = remainingDays.GetInt32();
            subscription.ExpiryDate = DateTime.Now.AddDays(days);
        }

        subscription.ErrorMessage = null;
    }

    /// <summary>
    /// 将字节数转换为GB
    /// </summary>
    private int ConvertBytesToGB(JsonElement bytesElement)
    {
        long bytes = bytesElement.ValueKind switch
        {
            JsonValueKind.Number => bytesElement.GetInt64(),
            JsonValueKind.String => long.TryParse(bytesElement.GetString(), out var val) ? val : 0,
            _ => 0
        };
        return (int)(bytes / (1024 * 1024 * 1024));
    }

    /// <summary>
    /// 解析到期日期
    /// </summary>
    private void ParseExpiryDate(JsonElement expiry, VPNSubscription subscription)
    {
        try
        {
            if (expiry.ValueKind == JsonValueKind.String)
            {
                if (DateTime.TryParse(expiry.GetString(), out var date))
                {
                    subscription.ExpiryDate = date;
                }
            }
            else if (expiry.ValueKind == JsonValueKind.Number)
            {
                var timestamp = expiry.GetInt64();
                if (timestamp > 1000000000000)
                    subscription.ExpiryDate = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
                else
                    subscription.ExpiryDate = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
            }
        }
        catch
        {
            // 忽略解析错误
        }
    }

    /// <summary>
    /// 从订阅 URL 获取流量使用信息（通过 HTTP 响应头）
    /// 与 Clash Verge Rev 相同的实现方式
    /// </summary>
    public async Task<SubscriptionUsage?> GetSubscriptionUsageFromUrlAsync(string url)
    {
        try
        {
            // 设置 User-Agent 头，模拟 Clash 客户端（有些机场会校验）
            _sharedHttpClient.DefaultRequestHeaders.Clear();
            _sharedHttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Clash-Verge/1.5.0 (Windows)");

            using var response = await _sharedHttpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"HTTP 错误: {(int)response.StatusCode} {response.StatusCode}");
            }

            // 查找 subscription-userinfo 响应头
            // 注意：Header 名大小写不统一，需要遍历查找
            var userInfoHeader = FindHeaderIgnoreCase(response.Headers, "subscription-userinfo");

            if (string.IsNullOrEmpty(userInfoHeader))
            {
                // 机场不支持流量信息
                return null;
            }

            return ParseSubscriptionUserInfo(userInfoHeader);
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"网络错误: {ex.Message}", ex);
        }
        catch (TaskCanceledException)
        {
            throw new Exception("请求超时");
        }
    }

    /// <summary>
    /// 在 HTTP 响应头中查找指定名称的 Header（忽略大小写）
    /// </summary>
    private string? FindHeaderIgnoreCase(HttpResponseHeaders headers, string headerName)
    {
        var lowerHeaderName = headerName.ToLower();

        foreach (var header in headers)
        {
            if (header.Key.ToLower() == lowerHeaderName)
            {
                return header.Value.FirstOrDefault();
            }
        }

        return null;
    }

    /// <summary>
    /// 解析 subscription-userinfo 响应头
    /// 常见格式: upload=123456; download=789012; total=107374182400; expire=1735689600
    /// </summary>
    private SubscriptionUsage ParseSubscriptionUserInfo(string header)
    {
        var result = new SubscriptionUsage();

        // 分割字段
        var parts = header.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var part in parts)
        {
            var kv = part.Split('=', 2);
            if (kv.Length != 2) continue;

            var key = kv[0].Trim().ToLower();
            var value = kv[1].Trim();

            switch (key)
            {
                case "upload":
                    if (long.TryParse(value, out var upload))
                    {
                        result.UploadBytes = upload;
                    }
                    break;

                case "download":
                    if (long.TryParse(value, out var download))
                    {
                        result.DownloadBytes = download;
                    }
                    break;

                case "total":
                    if (long.TryParse(value, out var total))
                    {
                        result.TotalBytes = total;
                    }
                    break;

                case "expire":
                    if (long.TryParse(value, out var expire))
                    {
                        // expire 可能不存在（按流量不过期或无限套餐）
                        if (expire > 0)
                        {
                            result.ExpireTime = DateTimeOffset.FromUnixTimeSeconds(expire).LocalDateTime;
                        }
                    }
                    break;
            }
        }

        return result;
    }

    public void Dispose()
    {
        // 静态 HttpClient 不需要释放
    }
}
