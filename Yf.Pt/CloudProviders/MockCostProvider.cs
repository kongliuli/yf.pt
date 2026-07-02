using Yf.Pt.Models;

namespace Yf.Pt.CloudProviders;

/// <summary>
/// Mock 成本提供者：内存生成近 3 个月成本记录。
/// </summary>
public class MockCostProvider : ICostProvider
{
    private readonly List<CostRecord> _costs;

    public MockCostProvider()
    {
        var periods = new[]
        {
            DateTime.UtcNow.AddMonths(-2).ToString("yyyy-MM"),
            DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM"),
            DateTime.UtcNow.ToString("yyyy-MM")
        };

        _costs = new();
        var id = 1;
        var resources = new[]
        {
            ("i-bp1web001node00100", "web-node-01", "ecs", 1, 320m),
            ("i-bp1web002node00201", "web-node-02", "ecs", 1, 320m),
            ("i-bp1api001node00101", "api-node-01", "ecs", 2, 280m),
            ("rm-bp1db01", "rds-main", "rds", 1, 860m),
            ("rm-bp1db02", "rds-replica", "rds", 2, 860m),
            ("lb-bp1slb01", "slb-web", "slb", 1, 45m),
            ("lb-bp1slb02", "slb-api", "slb", 2, 45m),
            ("d-bp1domain01", "yf.pt", "domain", 1, 68m)
        };

        foreach (var p in periods)
        {
            foreach (var (rid, name, cat, proj, baseAmt) in resources)
            {
                _costs.Add(new CostRecord
                {
                    Id = id++,
                    ResourceId = rid,
                    ResourceName = name,
                    AppProjectId = proj,
                    Amount = baseAmt * (0.9m + (id % 5) * 0.05m),
                    Currency = "CNY",
                    Period = p,
                    Category = cat
                });
            }
        }
    }

    public Task<IReadOnlyList<CostRecord>> ListCostsAsync(string? period = null, CancellationToken ct = default)
    {
        var result = period == null
            ? _costs
            : _costs.Where(c => c.Period == period).ToList();
        return Task.FromResult<IReadOnlyList<CostRecord>>(result);
    }
}
