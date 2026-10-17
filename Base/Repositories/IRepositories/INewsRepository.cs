using Base.Entities;
namespace Base.Repositories.IRepositories;

public interface INewsRepository
{
    Task<(List<NewsArticle> Items, int Total)> ListAsync(bool includeHidden, string? keyword, string? department, bool? published, int page, int size, CancellationToken ct);
    Task<NewsArticle?> GetAsync(Guid id, CancellationToken ct);
    Task<bool> IncrementViewsAsync(Guid id, CancellationToken ct);
    Task AddAsync(NewsArticle article, CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
