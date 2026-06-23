using Microsoft.EntityFrameworkCore;
using Yf.Pt.Models;

namespace Yf.Pt.Data;

/// <summary>
/// 首次启动时向 SQLite 写入种子数据。每张表独立判断，便于增量补充。
/// </summary>
public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        await SeedProjectsAsync(db);
        await SeedDomainsAsync(db);
        await SeedLocalsAsync(db);
        await SeedKnowledgeAsync(db);
        await SeedCertificatesAsync(db);
    }

    private static async Task SeedProjectsAsync(AppDbContext db)
    {
        if (await db.AppProjects.AnyAsync()) return;

        var projects = new List<AppProject>
        {
            new() { Name = "信息流主站", Description = "公司信息流汇总主站", Owner = "张三", CreatedAt = DateTime.UtcNow.AddDays(-120) },
            new() { Name = "API 网关", Description = "对外 API 网关服务", Owner = "李四", CreatedAt = DateTime.UtcNow.AddDays(-90) },
            new() { Name = "内部管理后台", Description = "运营与运维管理后台", Owner = "王五", CreatedAt = DateTime.UtcNow.AddDays(-60) }
        };
        await db.AppProjects.AddRangeAsync(projects);
        await db.SaveChangesAsync();
    }

    private static async Task SeedDomainsAsync(AppDbContext db)
    {
        if (await db.DomainBindings.AnyAsync()) return;

        var projects = await db.AppProjects.OrderBy(p => p.Id).ToListAsync();
        if (projects.Count < 3) return;

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
        await db.SaveChangesAsync();
    }

    private static async Task SeedLocalsAsync(AppDbContext db)
    {
        if (await db.LocalResources.AnyAsync()) return;

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

    private static async Task SeedKnowledgeAsync(AppDbContext db)
    {
        if (await db.KnowledgeArticles.AnyAsync()) return;

        var projects = await db.AppProjects.OrderBy(p => p.Id).ToListAsync();
        int? p1 = projects.Count > 0 ? projects[0].Id : null;
        int? p2 = projects.Count > 1 ? projects[1].Id : null;

        var articles = new List<KnowledgeArticle>
        {
            new()
            {
                Title = "ECS 发布 SOP",
                Category = "sop",
                Tags = "发布,ecs,运维",
                AppProjectId = p1,
                Author = "张三",
                Content = @"# ECS 发布 SOP

## 1. 发布前检查
- 确认代码已合并到 main 分支
- 检查 CI 构建通过
- 通知值班人员

## 2. 发布步骤
1. 登录 jump-host
2. 拉取最新镜像
3. 滚动重启服务
4. 验证健康检查

## 3. 回滚
- 执行 `rollback.sh` 脚本
- 通知相关负责人"
            },
            new()
            {
                Title = "信息流主站架构文档",
                Category = "doc",
                Tags = "架构,主站",
                AppProjectId = p1,
                Author = "张三",
                Content = @"# 信息流主站架构

## 整体架构
- 前端：Blazor Server
- 后端：ASP.NET Core API
- 数据库：RDS MySQL 8.0
- 缓存：Redis
- 负载均衡：SLB

## 资源依赖
- slb-web (47.96.10.11)
- web-node-01 / web-node-02
- rds-main
- redis-cache"
            },
            new()
            {
                Title = "API 网关限流配置",
                Category = "doc",
                Tags = "api,网关,限流",
                AppProjectId = p2,
                Author = "李四",
                Content = @"# API 网关限流

## 限流策略
- 全局：1000 QPS
- 单 IP：100 QPS
- 单用户：50 QPS

## 配置位置
SLB api (lb-bp1slb02) 监听规则"
            },
            new()
            {
                Title = "域名解析故障排查 FAQ",
                Category = "faq",
                Tags = "域名,dns,故障",
                Author = "王五",
                Content = @"# 域名解析故障排查

## Q: 域名无法访问？
1. `nslookup 域名` 检查解析
2. 确认 SLB 后端健康检查
3. 检查安全组规则

## Q: HTTPS 证书报错？
- 检查证书到期时间
- 在平台「到期提醒」页查看"
            },
            new()
            {
                Title = "值班交接 SOP",
                Category = "sop",
                Tags = "值班,交接",
                Author = "王五",
                Content = @"# 值班交接 SOP

## 交接内容
- 当日告警处理情况
- 未解决问题跟进
- 待发布事项

## 交接时间
每日 18:00 前完成交接"
            }
        };
        await db.KnowledgeArticles.AddRangeAsync(articles);
        await db.SaveChangesAsync();
    }

    private static async Task SeedCertificatesAsync(AppDbContext db)
    {
        if (await db.Certificates.AnyAsync()) return;

        var now = DateTime.UtcNow;
        var certs = new List<Certificate>
        {
            new() { Domain = "yf.pt", Issuer = "Let's Encrypt", ExpireAt = now.AddDays(-3), Note = "已过期，需立即续期" },
            new() { Domain = "api.yf.pt", Issuer = "阿里云数字证书", ExpireAt = now.AddDays(12), Note = "即将到期" },
            new() { Domain = "admin.yf.pt", Issuer = "Let's Encrypt", ExpireAt = now.AddDays(25), Note = "30天内到期" },
            new() { Domain = "cdn.yf.pt", Issuer = "阿里云数字证书", ExpireAt = now.AddDays(180), Note = "正常" },
            new() { Domain = "docs.yf.pt", Issuer = "Let's Encrypt", ExpireAt = now.AddDays(45), Note = "正常" }
        };
        await db.Certificates.AddRangeAsync(certs);
        await db.SaveChangesAsync();
    }
}
