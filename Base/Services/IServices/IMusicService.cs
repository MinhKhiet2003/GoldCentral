using Base.Models.Requests;
using Base.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Base.Services.IServices;

public interface IMusicService
{
    Task<List<MusicTrackResponse>> ListAsync(string? department, bool includeUnpublished, CancellationToken cancellationToken);
    Task<MusicTrackResponse> SaveAsync(Guid? id, MusicTrackRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<(string Path, string ContentType, string Title)?> GetStreamAsync(Guid id, CancellationToken cancellationToken);
}
