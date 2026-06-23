using Microsoft.EntityFrameworkCore;
using Yf.Pt.CloudProviders;
using Yf.Pt.Data;
using Yf.Pt.Models;

namespace Yf.Pt.Services;

/// <summary>
/// 仪表盘聚合服务：汇总计数、状态分布、趋势
/// </summary>
public class DashboardService
{
    private readonly AppDbContext _db;
    private readonly ICloudResourceProvider _cloud;

    public DashboardService(AppDbContext db, ICloudResourceProvider cloud)
    {
        _db = db;
        _cloud = cloud;
    }

    public async Task<DashboardSummary> GetSummaryAsync()
    {
        var projects = await _db.AppProjects.CountAsync();
        var domains = await _db.DomainBindings.CountAsync();
        var locals = await _db.LocalResources.CountAsync();
        var cloud = await _cloud.ListResourcesAsync();
        var stopped = cloud.Where(c => c.Status.Equals("Stopped", StringComparison.OrdinalIgnoreCase)).ToList();

        return new DashboardSummary
        {
            ProjectCount = projects,
            DomainCount = domains,
            LocalResourceCount = locals,
            CloudResourceCount = cloud.Count,
            StoppedResources = stopped
        };
    }

    public async Task<List<CloudResource>> ListCloudResourcesAsync() =>
        (await _cloud.ListResourcesAsync()).ToList();

    public async Task<CloudResource?> GetCloudResourceAsync(string resourceId) =>
        (await _cloud.ListResourcesAsync()).FirstOrDefault(c => c.ResourceId == resourceId);

    public Task<IReadOnlyList<MetricPoint>> GetMetricsAsync(string resourceId, string metricName, TimeRange range) =>
        _cloud.GetMetricsAsync(resourceId, metricName, range);

    public async Task<List<MetricSeries>> GetCpuTrendAsync(int hours = 24)
    {
        var resources = await _cloud.ListResourcesAsync();
        var ecs = resources.Where(r => r.Type == "ecs").Take(4).ToList();
        var range = new TimeRange(DateTime.UtcNow.AddHours(-hours), DateTime.UtcNow);

        var series = new List<MetricSeries>();
        foreach (var r in ecs)
        {
            var points = await _cloud.GetMetricsAsync(r.ResourceId, "cpu", range);
            series.Add(new MetricSeries { Name = r.Name, Points = points.ToList() });
        }
        return series;
    }
}

public class DashboardSummary
{
    public int ProjectCount { get; set; }
    public int DomainCount { get; set; }
    public int LocalResourceCount { get; set; }
    public int CloudResourceCount { get; set; }
    public List<CloudResource> StoppedResources { get; set; } = new();
}

public class MetricSeries
{
    public string Name { get; set; } = string.Empty;
    public List<MetricPoint> Points { get; set; } = new();
}
