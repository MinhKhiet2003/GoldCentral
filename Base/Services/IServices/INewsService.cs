using Base.Models.Requests;
using Base.Models.Responses;
namespace Base.Services.IServices;

public interface INewsService
{
    Task<PagedResult<NewsArticleResponse>> ListAsync(bool includeHidden, string? keyword, string? department, bool? published, int page, int size, CancellationToken ct);
    Task<NewsArticleResponse> GetAsync(Guid id, bool includeHidden, CancellationToken ct);
    Task RecordViewAsync(Guid id, CancellationToken ct);
    Task<NewsArticleResponse> SaveAsync(Guid? id, NewsArticleRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
