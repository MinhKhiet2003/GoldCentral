using Base.Entities;
using Base.Exceptions;
using Base.Models.Requests;
using Base.Models.Responses;
using Base.Repositories.IRepositories;
using Base.Services.IServices;
namespace Base.Services;

public class NewsService(INewsRepository repository, TimeProvider clock) : INewsService
{
    private static NewsArticleResponse Map(NewsArticle a, bool content) => new()
    {
        Id = a.Id, Title = a.Title, Summary = a.Summary, Content = content ? a.Content : null,
        ImageUrl = a.ImageUrl, Department = a.Department, IsPublished = a.IsPublished,
        CreatedAt = a.CreatedAt, UpdatedAt = a.UpdatedAt, PublishedAt = a.PublishedAt, ViewCount = a.ViewCount
    };
    public async Task<PagedResult<NewsArticleResponse>> ListAsync(bool includeHidden, string? keyword, string? department, bool? published, int page, int size, CancellationToken ct)
    {
        if (page < 1 || page > 100000 || size < 1 || size > 100 || keyword?.Length > 200)
            throw new ApiException(400, "Bộ lọc hoặc phân trang không hợp lệ.");
        var result = await repository.ListAsync(includeHidden, keyword, department, published, page, size, ct);
        return new() { Items = result.Items.Select(a => Map(a, false)), TotalItems = result.Total, PageNumber = page, PageSize = size, TotalPages = (int)Math.Ceiling(result.Total / (double)size) };
    }
    public async Task<NewsArticleResponse> GetAsync(Guid id, bool includeHidden, CancellationToken ct)
    {
        var article = await repository.GetAsync(id, ct);
        if (article is null || (!includeHidden && !article.IsPublished)) throw new ApiException(404, "Không tìm thấy bài viết.");
        return Map(article, true);
    }
    public async Task RecordViewAsync(Guid id, CancellationToken ct)
    {
        if (!await repository.IncrementViewsAsync(id, ct)) throw new ApiException(404, "Không tìm thấy bài viết.");
    }
    public async Task<NewsArticleResponse> SaveAsync(Guid? id, NewsArticleRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Content))
            throw new ApiException(400, "Vui lòng nhập tiêu đề và nội dung bài viết.");
        var image = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim();
        if (image is not null && !(image.StartsWith("/uploads/") && !image.Contains("..") && !image.Contains('\\'))
            && (!Uri.TryCreate(image, UriKind.Absolute, out var uri) || uri.Scheme != "https" || !string.IsNullOrEmpty(uri.UserInfo)))
            throw new ApiException(400, "Ảnh bìa phải là URL HTTPS hoặc ảnh đã tải lên hệ thống.");
        var now = clock.GetUtcNow().UtcDateTime;
        var article = id is null ? new NewsArticle { CreatedAt = now } : await repository.GetAsync(id.Value, ct) ?? throw new ApiException(404, "Không tìm thấy bài viết.");
        article.Title = request.Title.Trim(); article.Summary = request.Summary?.Trim() ?? ""; article.Content = request.Content.Trim();
        article.Department = request.Department; article.ImageUrl = image; article.IsPublished = request.IsPublished; article.UpdatedAt = now;
        if (article.IsPublished) article.PublishedAt ??= now;
        if (id is null) await repository.AddAsync(article, ct); else await repository.SaveAsync(ct);
        return Map(article, true);
    }
    public Task<bool> DeleteAsync(Guid id, CancellationToken ct) => repository.DeleteAsync(id, ct);
}
