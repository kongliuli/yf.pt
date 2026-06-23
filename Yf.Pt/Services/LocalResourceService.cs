using Microsoft.EntityFrameworkCore;
using Yf.Pt.Data;
using Yf.Pt.Models;

namespace Yf.Pt.Services;

public class LocalResourceService
{
    private readonly AppDbContext _db;
    public LocalResourceService(AppDbContext db) => _db = db;

    public Task<List<LocalResource>> ListAsync() =>
        _db.LocalResources.AsNoTracking().ToListAsync();

    public Task<LocalResource?> GetAsync(int id) =>
        _db.LocalResources.FirstOrDefaultAsync(x => x.Id == id);

    public async Task AddAsync(LocalResource resource)
    {
        _db.LocalResources.Add(resource);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(LocalResource resource)
    {
        _db.LocalResources.Update(resource);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var r = await _db.LocalResources.FindAsync(id);
        if (r != null)
        {
            _db.LocalResources.Remove(r);
            await _db.SaveChangesAsync();
        }
    }
}
