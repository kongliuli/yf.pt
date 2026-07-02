using Microsoft.EntityFrameworkCore;
using Yf.Pt.CloudProviders;
using Yf.Pt.Data;
using Yf.Pt.Models;

namespace Yf.Pt.Services;

public class CostService
{
    private readonly ICostProvider _provider;
    private readonly AppDbContext _db;

    public CostService(ICostProvider provider, AppDbContext db)
    {
        _provider = provider;
        _db = db;
    }

    public async Task<List<CostRecord>> ListAsync(string? period = null)
    {
        var costs = await _provider.ListCostsAsync(period);
        var projectIds = costs.Select(c => c.AppProjectId).Where(id => id != null).Distinct().ToList();
        var projects = await _db.AppProjects
            .Where(p => projectIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        foreach (var c in costs)
        {
            if (c.AppProjectId.HasValue && projects.TryGetValue(c.AppProjectId.Value, out var p))
                c.AppProject = p;
        }
        return costs.ToList();
    }

    public async Task<List<string>> GetPeriodsAsync()
    {
        var costs = await _provider.ListCostsAsync();
        return costs.Select(c => c.Period).Distinct().OrderByDescending(x => x).ToList();
    }

    public async Task<CostSummary> GetSummaryAsync(string? period = null)
    {
        var costs = await ListAsync(period);
        return new CostSummary
        {
            TotalAmount = costs.Sum(c => c.Amount),
            ByProject = costs
                .Where(c => c.AppProjectId.HasValue)
                .GroupBy(c => c.AppProject?.Name ?? "未分配")
                .Select(g => new CostSlice { Label = g.Key, Amount = g.Sum(x => x.Amount) })
                .OrderByDescending(x => x.Amount)
                .ToList(),
            ByCategory = costs
                .GroupBy(c => c.Category)
                .Select(g => new CostSlice { Label = g.Key, Amount = g.Sum(x => x.Amount) })
                .OrderByDescending(x => x.Amount)
                .ToList()
        };
    }
}

public class CostSummary
{
    public decimal TotalAmount { get; set; }
    public List<CostSlice> ByProject { get; set; } = new();
    public List<CostSlice> ByCategory { get; set; } = new();
}

public class CostSlice
{
    public string Label { get; set; } = "";
    public decimal Amount { get; set; }
}
