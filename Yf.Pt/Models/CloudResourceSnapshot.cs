namespace Yf.Pt.Models;

/// <summary>
/// 云资源快照：把 ICloudResourceProvider 返回的云资源落库一份，便于关联查询与离线展示
/// </summary>
public class CloudResourceSnapshot
{
    public int Id { get; set; }

    /// <summary>
    /// 云资源 ID（如 i-bp1xxx）
    /// </summary>
    public string ResourceId { get; set; } = string.Empty;

    public string Provider { get; set; } = "aliyun";

    /// <summary>
    /// ecs / rds / slb / domain
    /// </summary>
    public string Type { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string? Region { get; set; }

    /// <summary>
    /// Running / Stopped 等
    /// </summary>
    public string Status { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
