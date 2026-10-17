using Base.Exceptions;
using Base.Entities;
using Base.Common;
using Base.Models.Requests;
using Base.Models.Responses;
using Base.Repositories.IRepositories;
using Base.Services.IServices;
namespace Base.Services;

public class PortalService(IPortalRepository repository, TimeProvider clock) : IPortalService
{
    private static PortalAppResponse Map(PortalApp item) => new()
    {
        Id = item.Id, Name = item.Name, Description = item.Description, Category = item.Category,
        Platform = item.Platform, Url = item.Url, ImageUrl = item.ImageUrl,
        IsVisible = item.IsVisible, IsFeatured = item.IsFeatured, Order = item.Order + 1, Clicks = item.Clicks
    };
    public async Task<List<PortalAppResponse>> ListAsync(bool includeHidden, CancellationToken ct) =>
        (await repository.ListAsync(includeHidden, ct)).Select(Map).ToList(); 
    private static string? ValidateUrl(string? value, bool image = false)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (image && value.StartsWith("/uploads/") && !value.Contains("..") && !value.Contains('\\')) return value;
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || (uri.Scheme != "https" && uri.Scheme != "http") || !string.IsNullOrEmpty(uri.UserInfo))
            throw new ApiException(400, "URL phải là địa chỉ http hoặc https hợp lệ.");
        return uri.AbsoluteUri;
    }
    public async Task<PortalAppResponse> SaveAsync(Guid? id, PortalAppRequest request, CancellationToken ct)
    {
        var url = ValidateUrl(request.Url);
        var image = ValidateUrl(request.ImageUrl, true);
        if (string.IsNullOrWhiteSpace(request.Name)) throw new ApiException(400, "Vui lòng nhập tên ứng dụng.");
        var item = id is null ? new PortalApp() : await repository.GetAsync(id.Value, ct) ?? throw new ApiException(404, "Không tìm thấy ứng dụng.");
        item.Name = request.Name.Trim(); item.Description = request.Description?.Trim() ?? ""; item.Category = request.Category;
        item.Platform = request.Platform; item.Url = url; item.ImageUrl = image; item.IsVisible = request.IsVisible;
        item.IsFeatured = request.IsFeatured;
        await repository.SaveWithOrderAsync(item, Math.Max(1, request.Order), ct);
        return Map(item);
    }
    public Task<bool> DeleteAsync(Guid id, CancellationToken ct) => repository.DeleteAsync(id, ct);
    public async Task<string> VisitAsync(Guid id, CancellationToken ct)
    {
        var item = await repository.GetAsync(id, ct);
        if (item is null || !item.IsVisible) throw new ApiException(404, "Không tìm thấy ứng dụng.");
        var url = ValidateUrl(item.Url) ?? throw new ApiException(400, "Ứng dụng đang chờ cấu hình đường dẫn.");
        if (!await repository.IncrementClicksAsync(id, item.Url!, PortalReportingCalendar.VietnamDate(clock.GetUtcNow()), ct))
            throw new ApiException(409, "Ứng dụng vừa được cập nhật. Vui lòng tải lại danh sách.");
        return url;
    }
    public Task<PortalStatisticsResponse> StatisticsAsync(DateOnly? date, CancellationToken ct) => StatisticsAsync(date, date, date, ct);

    public async Task<PortalStatisticsResponse> StatisticsAsync(DateOnly? date, DateOnly? from, DateOnly? to, CancellationToken ct)
    {
        var today = PortalReportingCalendar.VietnamDate(clock.GetUtcNow());
        var rangeEnd = to ?? date ?? today;
        var rangeStart = from ?? date ?? rangeEnd;
        var selected = date ?? rangeEnd;
        if (selected.Year is < 2000 or > 9998 || rangeStart.Year is < 2000 or > 9998 || rangeEnd.Year is < 2000 or > 9998)
            throw new ApiException(400, "Vui lòng chọn ngày từ năm 2000 đến 9998.");
        if (rangeStart > rangeEnd || rangeEnd.DayNumber - rangeStart.DayNumber > 3660)
            throw new ApiException(400, "Khoảng thời gian không hợp lệ. Hãy chọn ngày bắt đầu trước ngày kết thúc, tối đa 10 năm.");
        var periods = PortalReportingCalendar.Periods(selected);
        periods.RangeStart = rangeStart;
        periods.RangeEnd = rangeEnd;
        periods.Items = await repository.StatisticsAsync(periods, ct);
        return periods;
    }
}
