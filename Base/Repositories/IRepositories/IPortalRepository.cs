using Base.Entities;
using Base.Models.Responses;
namespace Base.Repositories.IRepositories;

public interface IPortalRepository
{
    Task<List<PortalApp>> ListAsync(bool includeHidden, CancellationToken ct);
    Task<PortalApp?> GetAsync(Guid id, CancellationToken ct);
    Task AddAsync(PortalApp item, CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
    Task<bool> IncrementClicksAsync(Guid id, string expectedUrl, DateOnly date, CancellationToken ct);
    Task<List<PortalStatisticsRow>> StatisticsAsync(PortalStatisticsResponse periods, CancellationToken ct);
}
