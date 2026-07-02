using Microsoft.EntityFrameworkCore;
using Yf.Pt.CloudProviders;
using Yf.Pt.Data;
using Yf.Pt.Models;

namespace Yf.Pt.Services;

/// <summary>
/// 到期检查：证书 + 云资源域名 expire
/// </summary>
public class ExpiryCheckService
{
    private readonly AppDbContext _db;
    private readonly ICloudResourceProvider _cloud;

    public ExpiryCheckService(AppDbContext db, ICloudResourceProvider cloud)
    {
        _db = db;
        _cloud = cloud;
    }

    public async Task<List<Certificate>> ListCertificatesAsync() =>
        await _db.Certificates.AsNoTracking().ToListAsync();

    public async Task<Certificate?> GetCertificateAsync(int id) =>
        await _db.Certificates.FirstOrDefaultAsync(c => c.Id == id);

    public async Task AddCertificateAsync(Certificate cert)
    {
        _db.Certificates.Add(cert);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateCertificateAsync(Certificate cert)
    {
        _db.Certificates.Update(cert);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteCertificateAsync(int id)
    {
        var c = await _db.Certificates.FindAsync(id);
        if (c != null)
        {
            _db.Certificates.Remove(c);
            await _db.SaveChangesAsync();
        }
    }

    /// <summary>
    /// 即将到期的域名（来自云资源 domain 类型的 expire 属性）
    /// </summary>
    public async Task<List<DomainExpiry>> GetDomainExpiriesAsync()
    {
        var resources = await _cloud.ListResourcesAsync();
        return resources
            .Where(r => r.Type == "domain" && r.Attributes.TryGetValue("expire", out var exp) && DateTime.TryParse(exp, out _))
            .Select(r =>
            {
                DateTime.TryParse(r.Attributes["expire"], out var exp);
                return new DomainExpiry
                {
                    ResourceId = r.ResourceId,
                    Domain = r.Name,
                    ExpireAt = exp,
                    DaysRemaining = (int)Math.Ceiling((exp - DateTime.UtcNow).TotalDays)
                };
            })
            .ToList();
    }

    /// <summary>
    /// 30 天内到期的证书
    /// </summary>
    public async Task<List<Certificate>> GetExpiringCertificatesAsync(int withinDays = 30)
    {
        var threshold = DateTime.UtcNow.AddDays(withinDays);
        return await _db.Certificates
            .AsNoTracking()
            .Where(c => c.ExpireAt <= threshold)
            .OrderBy(c => c.ExpireAt)
            .ToListAsync();
    }
}

public class DomainExpiry
{
    public string ResourceId { get; set; } = "";
    public string Domain { get; set; } = "";
    public DateTime ExpireAt { get; set; }
    public int DaysRemaining { get; set; }
}
