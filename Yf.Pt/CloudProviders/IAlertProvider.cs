using Yf.Pt.Models;

namespace Yf.Pt.CloudProviders;

/// <summary>
/// 告警提供者抽象。当前 Mock 实现，后续可对接云监控/Prometheus。
/// </summary>
public interface IAlertProvider
{
    Task<IReadOnlyList<AlertRecord>> ListAlertsAsync(CancellationToken ct = default);
}

/// <summary>
/// 成本提供者抽象。当前 Mock 实现，后续可对接阿里云 BSS OpenAPI。
/// </summary>
public interface ICostProvider
{
    Task<IReadOnlyList<CostRecord>> ListCostsAsync(string? period = null, CancellationToken ct = default);
}
