using Base.Data;
using Base.Entities;
using Base.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
namespace Base.Repositories;

public class NewsRepository(AppDbContext db) : INewsRepository
{
    public async Task<(List<NewsArticle> Items, int Total)> ListAsync(bool includeHidden, string? keyword, string? department, bool? published, int page, int size, CancellationToken ct)
    {
        var query = db.NewsArticles.AsNoTracking().Where(a => includeHidden || a.IsPublished);
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var term = keyword.Trim().ToLowerInvariant();
            query = query.Where(a => a.Title.ToLower().Contains(term) || a.Summary.ToLower().Contains(term));
        }
        if (!string.IsNullOrEmpty(department)) query = query.Where(a => a.Department == department);
        if (includeHidden && published.HasValue) query = query.Where(a => a.IsPublished == published.Value);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(a => a.PublishedAt ?? a.CreatedAt).ThenByDescending(a => a.Id)
            .Skip((page - 1) * size).Take(size).ToListAsync(ct);
        return (items, total);
    }
    public Task<NewsArticle?> GetAsync(Guid id, CancellationToken ct) => db.NewsArticles.SingleOrDefaultAsync(a => a.Id == id, ct);
    public async Task<bool> IncrementViewsAsync(Guid id, CancellationToken ct) => await db.NewsArticles.Where(a => a.Id == id && a.IsPublished).ExecuteUpdateAsync(setters => setters.SetProperty(a => a.ViewCount, a => a.ViewCount + 1), ct) > 0;
    public async Task AddAsync(NewsArticle article, CancellationToken ct) { db.NewsArticles.Add(article); await db.SaveChangesAsync(ct); }
    public async Task SaveAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct) => await db.NewsArticles.Where(a => a.Id == id).ExecuteDeleteAsync(ct) > 0;
}
