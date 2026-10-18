using Base.Models.Requests;
using Base.Services;
using Base.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Base.Controllers;

[ApiController]
[Route("api/music/tracks")]
[Produces("application/json")]
public class MusicController(IMusicService service) : ControllerBase
{
    [AllowAnonymous, HttpGet]
    public async Task<IActionResult> List([FromQuery] string? department, CancellationToken cancellationToken) =>
        Ok(await service.ListAsync(department, false, cancellationToken));

    [Authorize(Roles = "admin"), HttpGet("manage")]
    public async Task<IActionResult> Manage(CancellationToken cancellationToken) =>
        Ok(await service.ListAsync("all", true, cancellationToken));

    [AllowAnonymous, HttpGet("{id:guid}/stream")]
    public async Task<IActionResult> Stream(Guid id, CancellationToken cancellationToken)
    {
        var audio = await service.GetStreamAsync(id, cancellationToken);
        if (audio is null) return NotFound();
        return PhysicalFile(audio.Value.Path, audio.Value.ContentType, enableRangeProcessing: true);
    }

    [Authorize(Roles = "admin"), HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MusicService.MaxAudioSize + 65536)]
    [RequestFormLimits(MultipartBodyLengthLimit = MusicService.MaxAudioSize + 65536)]
    public async Task<IActionResult> Create([FromForm] MusicTrackRequest request, CancellationToken cancellationToken) =>
        Ok(await service.SaveAsync(null, request, cancellationToken));

    [Authorize(Roles = "admin"), HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MusicService.MaxAudioSize + 65536)]
    [RequestFormLimits(MultipartBodyLengthLimit = MusicService.MaxAudioSize + 65536)]
    public async Task<IActionResult> Update(Guid id, [FromForm] MusicTrackRequest request, CancellationToken cancellationToken) =>
        Ok(await service.SaveAsync(id, request, cancellationToken));

    [Authorize(Roles = "admin"), HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        await service.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}
