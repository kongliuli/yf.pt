using Yf.Pt.CloudProviders;
using Yf.Pt.Models;

namespace Yf.Pt.Services;

public class AlertService
{
    private readonly IAlertProvider _provider;
    public AlertService(IAlertProvider provider) => _provider = provider;

    public async Task<List<AlertRecord>> ListAsync()
    {
        var alerts = await _provider.ListAlertsAsync();
        return alerts.OrderByDescending(a => a.Severity == "critical")
                     .ThenByDescending(a => a.Severity == "warning")
                     .ThenByDescending(a => a.TriggeredAt)
                     .ToList();
    }

    public async Task<AlertSummary> GetSummaryAsync()
    {
        var alerts = await _provider.ListAlertsAsync();
        return new AlertSummary
        {
            Total = alerts.Count,
            Critical = alerts.Count(a => a.Severity == "critical"),
            Warning = alerts.Count(a => a.Severity == "warning"),
            Info = alerts.Count(a => a.Severity == "info"),
            Active = alerts.Count(a => a.Status == "active")
        };
    }
}

public class AlertSummary
{
    public int Total { get; set; }
    public int Critical { get; set; }
    public int Warning { get; set; }
    public int Info { get; set; }
    public int Active { get; set; }
}
