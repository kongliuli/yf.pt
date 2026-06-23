namespace Yf.Pt.Models;

/// <summary>
/// 域名绑定：项目 → 域名 → 目标（IP/别名/SLB/OSS） → 关联云资源
/// </summary>
public class DomainBinding
{
    public int Id { get; set; }
    public int AppProjectId { get; set; }
    public AppProject? AppProject { get; set; }

    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// 类型：A / CNAME / SLB / OSS 等
    /// </summary>
    public string Type { get; set; } = "A";

    /// <summary>
    /// 指向：IP / 别名 / SLB ID / OSS bucket
    /// </summary>
    public string Target { get; set; } = string.Empty;

    /// <summary>
    /// 关联云资源 ID（可选，如 i-bp1xxx）
    /// </summary>
    public string? CloudResourceId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
