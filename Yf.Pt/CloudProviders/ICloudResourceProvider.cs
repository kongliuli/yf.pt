namespace Yf.Pt.CloudProviders;

/// <summary>
/// 云资源（运行时视图，来自 Provider，不一定落库）
/// </summary>
public class CloudResource
{
    public string ResourceId { get; set; } = string.Empty;
    public string Provider { get; set; } = "aliyun";

    /// <summary>
    /// ecs / rds / slb / domain
    /// </summary>
    public string Type { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string? Region { get; set; }
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 附加属性（IP/规格/域名类型等）
    /// </summary>
    public Dictionary<string, string> Attributes { get; set; } = new();
}

/// <summary>
/// 单个指标数据点
/// </summary>
public class MetricPoint
{
    public DateTime Timestamp { get; set; }
    public decimal Value { get; set; }
}

/// <summary>
/// 时间范围
/// </summary>
public record TimeRange(DateTime Start, DateTime End);

/// <summary>
/// 云资源提供者抽象。当前 Mock 实现，后续替换为阿里云 OpenAPI 实现。
/// </summary>
public interface ICloudResourceProvider
{
    Task<IReadOnlyList<CloudResource>> ListResourcesAsync(CancellationToken ct = default);

    Task<IReadOnlyList<MetricPoint>> GetMetricsAsync(
        string resourceId,
        string metricName,
        TimeRange range,
        CancellationToken ct = default);
}
