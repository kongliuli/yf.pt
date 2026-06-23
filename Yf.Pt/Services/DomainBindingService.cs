using Microsoft.EntityFrameworkCore;
using Yf.Pt.Data;
using Yf.Pt.Models;

namespace Yf.Pt.Services;

public class DomainBindingService
{
    private readonly AppDbContext _db;
    public DomainBindingService(AppDbContext db) => _db = db;

    public Task<List<DomainBinding>> ListAsync() =>
        _db.DomainBindings.Include(d => d.AppProject).AsNoTracking().ToListAsync();

    public Task<DomainBinding?> GetAsync(int id) =>
        _db.DomainBindings.Include(d => d.AppProject).FirstOrDefaultAsync(d => d.Id == id);

    public async Task AddAsync(DomainBinding binding)
    {
        binding.CreatedAt = DateTime.UtcNow;
        _db.DomainBindings.Add(binding);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(DomainBinding binding)
    {
        _db.DomainBindings.Update(binding);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var d = await _db.DomainBindings.FindAsync(id);
        if (d != null)
        {
            _db.DomainBindings.Remove(d);
            await _db.SaveChangesAsync();
        }
    }
}
