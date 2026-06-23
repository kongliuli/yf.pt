using Microsoft.EntityFrameworkCore;
using System.Text;
using Yf.Pt.CloudProviders;
using Yf.Pt.Data;
using Yf.Pt.Models;

namespace Yf.Pt.Services;

/// <summary>
/// 资源拓扑：基于 项目→域名→云资源 关系构建 Mermaid flowchart 文本
/// </summary>
public class TopologyService
{
    private readonly AppDbContext _db;
    private readonly ICloudResourceProvider _cloud;

    public TopologyService(AppDbContext db, ICloudResourceProvider cloud)
    {
        _db = db;
        _cloud = cloud;
    }

    public async Task<string> BuildMermaidAsync()
    {
        var projects = await _db.AppProjects.Include(p => p.DomainBindings).AsNoTracking().ToListAsync();
        var cloud = await _cloud.ListResourcesAsync();
        var cloudMap = cloud.ToDictionary(c => c.ResourceId);

        var sb = new StringBuilder();
        sb.AppendLine("flowchart LR");

        foreach (var p in projects)
        {
            var pid = $"p{p.Id}";
            sb.AppendLine($"  {pid}[\"{p.Name}\"]:::project");

            foreach (var d in p.DomainBindings)
            {
                var did = $"d{d.Id}";
                sb.AppendLine($"  {pid} --> {did}[\"{d.Domain}\"]:::domain");

                if (!string.IsNullOrEmpty(d.CloudResourceId) && cloudMap.TryGetValue(d.CloudResourceId, out var cr))
                {
                    var rid = $"r{cr.ResourceId.Replace("-", "")}";
                    sb.AppendLine($"  {did} --> {rid}[\"{cr.Name}<br/>{cr.Type}\"]:::resource");
                }
                else if (!string.IsNullOrEmpty(d.Target))
                {
                    var tid = $"t{d.Id}";
                    sb.AppendLine($"  {did} --> {tid}[\"{d.Target}\"]:::target");
                }
            }
        }

        sb.AppendLine("  classDef project fill:#0d6efd,color:#fff,stroke:#0a58ca,stroke-width:2px");
        sb.AppendLine("  classDef domain fill:#198754,color:#fff,stroke:#146c43,stroke-width:1px");
        sb.AppendLine("  classDef resource fill:#fd7e14,color:#fff,stroke:#ca6510,stroke-width:1px");
        sb.AppendLine("  classDef target fill:#6c757d,color:#fff,stroke:#565e64,stroke-width:1px");
        return sb.ToString();
    }
}
