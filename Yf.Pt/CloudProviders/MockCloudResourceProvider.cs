namespace Yf.Pt.CloudProviders;

/// <summary>
/// Mock 云资源提供者：内存生成资源与指标，保证图表有数据可画。
/// 后续实现 AliyunCloudResourceProvider 替换即可。
/// </summary>
public class MockCloudResourceProvider : ICloudResourceProvider
{
    private readonly List<CloudResource> _resources;
    private readonly Random _rng = new(42);

    public MockCloudResourceProvider()
    {
        _resources = BuildSeedResources();
    }

    public Task<IReadOnlyList<CloudResource>> ListResourcesAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IReadOnlyList<CloudResource>>(_resources);
    }

    public Task<IReadOnlyList<MetricPoint>> GetMetricsAsync(
        string resourceId,
        string metricName,
        TimeRange range,
        CancellationToken ct = default)
    {
        // 每小时一个点，基线 + 正弦扰动 + 随机噪声
        var points = new List<MetricPoint>();
        var hours = (int)Math.Ceiling((range.End - range.Start).TotalHours);
        var baseline = metricName.ToLowerInvariant() switch
        {
            "cpu" => 35.0,
            "memory" => 55.0,
            "network" => 120.0,
            _ => 50.0,
        };
        var amplitude = metricName.ToLowerInvariant() switch
        {
            "cpu" => 25.0,
            "memory" => 10.0,
            "network" => 60.0,
            _ => 20.0,
        };

        for (var i = 0; i <= hours; i++)
        {
            var ts = range.Start.AddHours(i);
            var phase = i / 24.0 * Math.PI * 2;
            var value = baseline + amplitude * Math.Sin(phase) + _rng.NextDouble() * 8 - 4;
            points.Add(new MetricPoint { Timestamp = ts, Value = (decimal)Math.Max(0, Math.Round(value, 2)) });
        }

        return Task.FromResult<IReadOnlyList<MetricPoint>>(points);
    }

    private static List<CloudResource> BuildSeedResources()
    {
        var list = new List<CloudResource>();

        // ECS x8
        var ecsNames = new[] { "web-node-01", "web-node-02", "api-node-01", "api-node-02", "worker-01", "worker-02", "jump-host", "bastion" };
        for (var i = 0; i < ecsNames.Length; i++)
        {
            list.Add(new CloudResource
            {
                ResourceId = $"i-bp1{ecsNames[i].Replace('-', '0')}{i:D2}",
                Provider = "aliyun",
                Type = "ecs",
                Name = ecsNames[i],
                Region = i % 2 == 0 ? "cn-hangzhou" : "cn-shanghai",
                Status = i == 6 ? "Stopped" : "Running",
                Attributes = new()
                {
                    ["ip"] = $"10.0.{i}.{10 + i}",
                    ["spec"] = i % 3 == 0 ? "ecs.g6.large" : "ecs.g6.medium",
                    ["cpu"] = i % 3 == 0 ? "2核" : "1核",
                    ["memory"] = i % 3 == 0 ? "8GiB" : "4GiB"
                }
            });
        }

        // RDS x2
        list.Add(new CloudResource { ResourceId = "rm-bp1db01", Type = "rds", Name = "rds-main", Region = "cn-hangzhou", Status = "Running", Attributes = new() { ["engine"] = "MySQL 8.0", ["spec"] = "rds.mysql.s2.large", ["cpu"] = "2核", ["memory"] = "16GiB" } });
        list.Add(new CloudResource { ResourceId = "rm-bp1db02", Type = "rds", Name = "rds-replica", Region = "cn-hangzhou", Status = "Running", Attributes = new() { ["engine"] = "MySQL 8.0", ["spec"] = "rds.mysql.s2.large", ["cpu"] = "2核", ["memory"] = "16GiB" } });

        // SLB x2
        list.Add(new CloudResource { ResourceId = "lb-bp1slb01", Type = "slb", Name = "slb-web", Region = "cn-hangzhou", Status = "Running", Attributes = new() { ["ip"] = "47.96.10.11", ["spec"] = "slb.s1.small" } });
        list.Add(new CloudResource { ResourceId = "lb-bp1slb02", Type = "slb", Name = "slb-api", Region = "cn-shanghai", Status = "Running", Attributes = new() { ["ip"] = "47.96.20.22", ["spec"] = "slb.s1.small" } });

        // 域名 x5
        list.Add(new CloudResource { ResourceId = "d-bp1domain01", Type = "domain", Name = "yf.pt", Region = null, Status = "Active", Attributes = new() { ["registrar"] = "阿里云", ["expire"] = "2027-01-01" } });
        list.Add(new CloudResource { ResourceId = "d-bp1domain02", Type = "domain", Name = "api.yf.pt", Region = null, Status = "Active", Attributes = new() { ["registrar"] = "阿里云", ["expire"] = "2027-01-01" } });
        list.Add(new CloudResource { ResourceId = "d-bp1domain03", Type = "domain", Name = "cdn.yf.pt", Region = null, Status = "Active", Attributes = new() { ["registrar"] = "阿里云", ["expire"] = "2026-09-01" } });
        list.Add(new CloudResource { ResourceId = "d-bp1domain04", Type = "domain", Name = "admin.yf.pt", Region = null, Status = "Active", Attributes = new() { ["registrar"] = "阿里云", ["expire"] = "2026-08-01" } });
        list.Add(new CloudResource { ResourceId = "d-bp1domain05", Type = "domain", Name = "docs.yf.pt", Region = null, Status = "Active", Attributes = new() { ["registrar"] = "阿里云", ["expire"] = "2027-03-01" } });

        return list;
    }
}
