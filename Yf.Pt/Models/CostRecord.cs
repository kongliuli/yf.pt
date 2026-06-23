namespace Yf.Pt.Models;

/// <summary>
/// 成本记录（按月）
/// </summary>
public class CostRecord
{
    public int Id { get; set; }
    public string ResourceId { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;

    public int? AppProjectId { get; set; }
    public AppProject? AppProject { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "CNY";

    /// <summary>
    /// 周期 yyyy-MM
    /// </summary>
    public string Period { get; set; } = DateTime.UtcNow.ToString("yyyy-MM");

    /// <summary>
    /// ecs / rds / slb / domain / other
    /// </summary>
    public string Category { get; set; } = "other";
}
