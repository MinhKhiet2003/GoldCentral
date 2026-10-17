using Base.Models.Requests;
using Base.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Base.Controllers;

public class NewsController(INewsService service) : Controller
{
    [HttpGet("/news")]
    public IActionResult Index() => View();
    [HttpGet("/news/{id:guid}")]
    public IActionResult Article(Guid id) { ViewData["ArticleId"] = id; return View("Index"); }
    [AllowAnonymous, HttpGet("/api/news")]
    public async Task<IActionResult> List(string? keyword, string? department, int page = 1, int size = 9, CancellationToken ct = default) =>
        Ok(await service.ListAsync(false, keyword, department, null, page, size, ct));
    [AllowAnonymous, HttpGet("/api/news/{id:guid}")]
    public async Task<IActionResult> Detail(Guid id, CancellationToken ct)
    {
        await service.RecordViewAsync(id, ct);
        return Ok(await service.GetAsync(id, false, ct));
    }
    [Authorize(Roles = "admin"), HttpGet("/api/news/manage")]
    public async Task<IActionResult> Manage(string? keyword, string? department, bool? published, int page = 1, int size = 12, CancellationToken ct = default) =>
        Ok(await service.ListAsync(true, keyword, department, published, page, size, ct));
    [Authorize(Roles = "admin"), HttpGet("/api/news/manage/{id:guid}")]
    public async Task<IActionResult> GetForEdit(Guid id, CancellationToken ct) => Ok(await service.GetAsync(id, true, ct));
    [Authorize(Roles = "admin"), HttpPost("/api/news")]
    public async Task<IActionResult> Create([FromBody] NewsArticleRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(new { message = "Vui lòng kiểm tra nội dung và độ dài các trường." });
        return Ok(await service.SaveAsync(null, request, ct));
    }
    [Authorize(Roles = "admin"), HttpPut("/api/news/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] NewsArticleRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(new { message = "Vui lòng kiểm tra nội dung và độ dài các trường." });
        return Ok(await service.SaveAsync(id, request, ct));
    }
    [Authorize(Roles = "admin"), HttpDelete("/api/news/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) => await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
