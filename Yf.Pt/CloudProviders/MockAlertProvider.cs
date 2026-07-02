using Yf.Pt.Models;

namespace Yf.Pt.CloudProviders;

/// <summary>
/// Mock 告警提供者：内存生成告警记录。
/// </summary>
public class MockAlertProvider : IAlertProvider
{
    private readonly List<AlertRecord> _alerts;

    public MockAlertProvider()
    {
        var now = DateTime.UtcNow;
        _alerts = new List<AlertRecord>
        {
            new() { ResourceId = "i-bp1web001node00100", ResourceName = "web-node-01", Severity = "critical", Title = "CPU 使用率持续过高", Message = "近 10 分钟 CPU 平均使用率 92%，超过阈值 85%", Status = "active", TriggeredAt = now.AddMinutes(-12) },
            new() { ResourceId = "i-bp1api001node00101", ResourceName = "api-node-01", Severity = "warning", Title = "内存使用率偏高", Message = "内存使用率 78%，接近阈值 80%", Status = "active", TriggeredAt = now.AddMinutes(-35) },
            new() { ResourceId = "rm-bp1db01", ResourceName = "rds-main", Severity = "warning", Title = "磁盘空间不足", Message = "磁盘使用率 82%，剩余 18%", Status = "ack", TriggeredAt = now.AddHours(-2) },
            new() { ResourceId = "i-bp1jump0host007", ResourceName = "jump-host", Severity = "critical", Title = "实例已停止", Message = "ECS 实例处于 Stopped 状态", Status = "active", TriggeredAt = now.AddHours(-5) },
            new() { ResourceId = "lb-bp1slb01", ResourceName = "slb-web", Severity = "info", Title = "后端健康检查异常恢复", Message = "ECS web-node-02 健康检查已恢复", Status = "resolved", TriggeredAt = now.AddHours(-8) },
            new() { ResourceId = "d-bp1domain04", ResourceName = "admin.yf.pt", Severity = "warning", Title = "域名即将到期", Message = "域名将于 30 天内到期", Status = "active", TriggeredAt = now.AddHours(-1) },
            new() { ResourceId = "i-bp1worker001002", ResourceName = "worker-02", Severity = "info", Title = "网络入流量突增", Message = "入流量较基线增长 150%", Status = "resolved", TriggeredAt = now.AddHours(-20) }
        };
    }

    public Task<IReadOnlyList<AlertRecord>> ListAlertsAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<AlertRecord>>(_alerts);
}
