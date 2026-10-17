using Base.Services.IServices;
using Base.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Base.Controllers;

public class PortalController(IPortalService service) : Controller
{
    [HttpGet("/")]
    public IActionResult Index() => View();
    [HttpGet("/admin")]
    public IActionResult Admin() => View("Index");
    [HttpGet("/api/portal/apps")]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await service.ListAsync(false, ct));
    [Authorize(Roles = "admin"), HttpGet("/api/portal/manage")]
    public async Task<IActionResult> Manage(CancellationToken ct) => Ok(await service.ListAsync(true, ct));
    [Authorize(Roles = "admin"), HttpGet("/api/portal/statistics")]
    public async Task<IActionResult> Statistics([FromQuery] DateOnly? date, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(new { message = "Ngày thống kê phải có định dạng yyyy-MM-dd." });
        return Ok(await service.StatisticsAsync(date, from, to, ct));
    }
    [Authorize(Roles = "admin"), HttpPost("/api/portal/apps")]
    public async Task<IActionResult> Create([FromBody] PortalAppRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return Ok(await service.SaveAsync(null, request, ct));
    }
    [Authorize(Roles = "admin"), HttpPut("/api/portal/apps/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] PortalAppRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return Ok(await service.SaveAsync(id, request, ct));
    }
    [Authorize(Roles = "admin"), HttpDelete("/api/portal/apps/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) => await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
    [HttpPost("/api/portal/apps/{id:guid}/visit")]
    public async Task<IActionResult> Visit(Guid id, CancellationToken ct) => Ok(new { url = await service.VisitAsync(id, ct) });
}
