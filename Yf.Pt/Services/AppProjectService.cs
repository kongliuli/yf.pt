using Microsoft.EntityFrameworkCore;
using Yf.Pt.Data;
using Yf.Pt.Models;

namespace Yf.Pt.Services;

public class AppProjectService
{
    private readonly AppDbContext _db;
    public AppProjectService(AppDbContext db) => _db = db;

    public Task<List<AppProject>> ListAsync() =>
        _db.AppProjects.Include(p => p.DomainBindings).AsNoTracking().ToListAsync();

    public Task<AppProject?> GetAsync(int id) =>
        _db.AppProjects.Include(p => p.DomainBindings).FirstOrDefaultAsync(p => p.Id == id);

    public async Task AddAsync(AppProject project)
    {
        project.CreatedAt = DateTime.UtcNow;
        _db.AppProjects.Add(project);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(AppProject project)
    {
        _db.AppProjects.Update(project);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var p = await _db.AppProjects.FindAsync(id);
        if (p != null)
        {
            _db.AppProjects.Remove(p);
            await _db.SaveChangesAsync();
        }
    }
}
