namespace Yf.Pt.Models;

/// <summary>
/// 告警记录
/// </summary>
public class AlertRecord
{
    public int Id { get; set; }
    public string ResourceId { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;

    /// <summary>
    /// critical / warning / info
    /// </summary>
    public string Severity { get; set; } = "warning";

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// active / ack / resolved
    /// </summary>
    public string Status { get; set; } = "active";

    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
}
