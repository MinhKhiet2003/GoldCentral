using Base.Data;
using Base.Exceptions;
using Base.Models.Requests;
using Base.Repositories;
using Base.Services;
using Microsoft.EntityFrameworkCore;

internal static class NewsChecks
{
    public static async Task RunAsync(DbContextOptions<AppDbContext> options)
    {
        void Check(bool condition, string name) { if (!condition) throw new Exception(name); Console.WriteLine("PASS News " + name); }
        await using var db = new AppDbContext(options);
        var service = new NewsService(new NewsRepository(db), TimeProvider.System);
        var request = new NewsArticleRequest { Title = "Thông báo kiểm thử", Summary = "Tóm tắt", Content = "Nội dung thử nghiệm\nDòng thứ hai", Department = "office" };
        var draft = await service.SaveAsync(null, request, default);
        Check(!draft.IsPublished && draft.PublishedAt is null, "draft stays unpublished");
        Check((await service.ListAsync(false, null, null, null, 1, 9, default)).TotalItems == 0, "draft excluded from employee feed");
        try { await service.GetAsync(draft.Id, false, default); throw new Exception("Draft readable"); }
        catch (ApiException e) when (e.StatusCode == 404) { Console.WriteLine("PASS News draft detail blocked"); }
        Check((await service.GetAsync(draft.Id, true, default)).Content == request.Content, "admin can edit draft content");
        request.IsPublished = true;
        var published = await service.SaveAsync(draft.Id, request, default);
        Check(published.PublishedAt.HasValue, "publication records date");
        var firstPublication = published.PublishedAt;
        request.Title = "Thông báo đã cập nhật";
        var edited = await service.SaveAsync(draft.Id, request, default);
        Check(edited.PublishedAt == firstPublication && edited.Title == request.Title, "editing preserves first publication date");
        Check((await service.ListAsync(false, "cập nhật", "office", null, 1, 9, default)).TotalItems == 1, "search and department filters");
        Check((await service.ListAsync(false, null, null, false, 1, 9, default)).TotalItems == 1, "employee cannot override published filter");
        request.IsPublished = false;
        await service.SaveAsync(draft.Id, request, default);
        Check((await service.ListAsync(false, null, null, null, 1, 9, default)).TotalItems == 0, "unpublishing removes article from feed");
        request.ImageUrl = "javascript:alert(1)";
        try { await service.SaveAsync(draft.Id, request, default); throw new Exception("Unsafe image accepted"); }
        catch (ApiException e) when (e.StatusCode == 400) { Console.WriteLine("PASS News unsafe image rejected"); }
        request.ImageUrl = null; request.IsPublished = true;
        var ids = new List<Guid> { draft.Id };
        for (var i = 0; i < 13; i++) { request.Title = "Bài viết phân trang " + i; ids.Add((await service.SaveAsync(null, request, default)).Id); }
        var page = await service.ListAsync(false, null, null, null, 2, 9, default);
        Check(page.TotalItems == 13 && page.TotalPages == 2 && page.Items.Count() == 4, "pagination and totals");
        Check(page.Items.All(a => a.Content is null), "feed omits full article content");
        foreach (var id in ids) await service.DeleteAsync(id, default);
        Check((await service.ListAsync(true, null, null, null, 1, 12, default)).TotalItems == 0, "delete removes articles");
    }
}
