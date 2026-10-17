using Base.Models.Requests;
using Base.Models.Responses;
namespace Base.Services.IServices;
public interface IPortalService
{
    Task<List<PortalAppResponse>> ListAsync(bool includeHidden, CancellationToken ct);
    Task<PortalAppResponse> SaveAsync(Guid? id, PortalAppRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
    Task<string> VisitAsync(Guid id, CancellationToken ct);
    Task<PortalStatisticsResponse> StatisticsAsync(DateOnly? date, CancellationToken ct);
    Task<PortalStatisticsResponse> StatisticsAsync(DateOnly? date, DateOnly? from, DateOnly? to, CancellationToken ct);
}
