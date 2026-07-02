using Microsoft.EntityFrameworkCore;
using Yf.Pt.Data;
using Yf.Pt.Models;

namespace Yf.Pt.Services;

public class KnowledgeService
{
    private readonly AppDbContext _db;
    public KnowledgeService(AppDbContext db) => _db = db;

    public Task<List<KnowledgeArticle>> ListAsync() =>
        _db.KnowledgeArticles.Include(a => a.AppProject).AsNoTracking().ToListAsync();

    public Task<List<KnowledgeArticle>> SearchAsync(string keyword)
    {
        var kw = keyword.Trim();
        return _db.KnowledgeArticles
            .Include(a => a.AppProject)
            .AsNoTracking()
            .Where(a => a.Title.Contains(kw) || a.Content.Contains(kw) || (a.Tags != null && a.Tags.Contains(kw)))
            .ToListAsync();
    }

    public Task<KnowledgeArticle?> GetAsync(int id) =>
        _db.KnowledgeArticles.Include(a => a.AppProject).FirstOrDefaultAsync(a => a.Id == id);

    public async Task AddAsync(KnowledgeArticle article)
    {
        article.CreatedAt = DateTime.UtcNow;
        article.UpdatedAt = DateTime.UtcNow;
        _db.KnowledgeArticles.Add(article);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(KnowledgeArticle article)
    {
        article.UpdatedAt = DateTime.UtcNow;
        _db.KnowledgeArticles.Update(article);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var a = await _db.KnowledgeArticles.FindAsync(id);
        if (a != null)
        {
            _db.KnowledgeArticles.Remove(a);
            await _db.SaveChangesAsync();
        }
    }
}
