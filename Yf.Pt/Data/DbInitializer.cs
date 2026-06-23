using Microsoft.EntityFrameworkCore;
using Yf.Pt.Models;

namespace Yf.Pt.Data;

/// <summary>
/// 首次启动时向 SQLite 写入种子数据
/// </summary>
public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        if (await db.AppProjects.AnyAsync()) return;

        var projects = new List<AppProject>
        {
            new() { Name = "信息流主站", Description = "公司信息流汇总主站", Owner = "张三", CreatedAt = DateTime.UtcNow.AddDays(-120) },
            new() { Name = "API 网关", Description = "对外 API 网关服务", Owner = "李四", CreatedAt = DateTime.UtcNow.AddDays(-90) },
            new() { Name = "内部管理后台", Description = "运营与运维管理后台", Owner = "王五", CreatedAt = DateTime.UtcNow.AddDays(-60) }
        };
        await db.AppProjects.AddRangeAsync(projects);
        await db.SaveChangesAsync();

        var domains = new List<DomainBinding>
        {
            new() { AppProjectId = projects[0].Id, Domain = "yf.pt", Type = "A", Target = "47.96.10.11", CloudResourceId = "lb-bp1slb01", CreatedAt = DateTime.UtcNow.AddDays(-120) },
            new() { AppProjectId = projects[0].Id, Domain = "www.yf.pt", Type = "CNAME", Target = "cdn.yf.pt", CloudResourceId = "d-bp1domain03", CreatedAt = DateTime.UtcNow.AddDays(-110) },
            new() { AppProjectId = projects[1].Id, Domain = "api.yf.pt", Type = "A", Target = "47.96.20.22", CloudResourceId = "lb-bp1slb02", CreatedAt = DateTime.UtcNow.AddDays(-90) },
            new() { AppProjectId = projects[1].Id, Domain = "openapi.yf.pt", Type = "CNAME", Target = "api.yf.pt", CloudResourceId = "d-bp1domain02", CreatedAt = DateTime.UtcNow.AddDays(-85) },
            new() { AppProjectId = projects[2].Id, Domain = "admin.yf.pt", Type = "A", Target = "10.0.0.10", CloudResourceId = "i-bp1web001node00100", CreatedAt = DateTime.UtcNow.AddDays(-60) },
            new() { AppProjectId = projects[2].Id, Domain = "docs.yf.pt", Type = "CNAME", Target = "oss-static.yf.pt", CloudResourceId = "d-bp1domain05", CreatedAt = DateTime.UtcNow.AddDays(-55) }
        };
        await db.DomainBindings.AddRangeAsync(domains);

        var locals = new List<LocalResource>
        {
            new() { Name = "web-node-01", Type = "server", Ip = "10.0.0.10", Location = "杭州机房A", Owner = "张三", Note = "主站前端" },
            new() { Name = "web-node-02", Type = "server", Ip = "10.0.0.11", Location = "杭州机房A", Owner = "张三", Note = "主站前端备" },
            new() { Name = "api-node-01", Type = "vm", Ip = "10.0.1.10", Location = "杭州机房B", Owner = "李四", Note = "API 服务" },
            new() { Name = "redis-cache", Type = "service", Ip = "10.0.2.20", Location = "杭州机房B", Owner = "李四", Note = "Redis 缓存集群" },
            new() { Name = "nas-storage", Type = "server", Ip = "10.0.3.30", Location = "上海机房", Owner = "王五", Note = "NAS 存储" }
        };
        await db.LocalResources.AddRangeAsync(locals);

        await db.SaveChangesAsync();
    }
}
