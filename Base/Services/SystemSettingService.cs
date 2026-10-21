using Base.Entities;
using Base.Exceptions;
using Base.Models.Requests;
using Base.Models.Responses;
using Base.Repositories.IRepositories;
using Base.Services.IServices;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace Base.Services;

public class SystemSettingService(ISystemSettingRepository repository, IMemoryCache cache) : ISystemSettingService
{
    private const string PortalEventThemeKey = "portal.eventTheme";
    private const string PortalEventThemeCacheKey = "portal.eventTheme.response";
    private static readonly HashSet<string> PortalEventKeys = ["none", "october20", "halloween", "christmas", "new-year"];

    public async Task<IEnumerable<SystemSettingResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var settings = await repository.ListAsync(cancellationToken);
        return settings.Select(ToResponse);
    }

    public async Task<SystemSettingResponse> UpsertAsync(string key, SystemSettingRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length > 100 || !key.All(item => char.IsLetterOrDigit(item) || item is '.' or '_' or '-'))
        {
            throw new ApiException(400, "Khóa cấu hình chỉ gồm chữ, số, dấu chấm, gạch dưới hoặc gạch ngang và không quá 100 ký tự.");
        }

        var setting = await repository.GetAsync(key, cancellationToken);
        if (setting is null)
        {
            setting = new SystemSetting { Key = key };
            await repository.AddAsync(setting, cancellationToken);
        }

        setting.Value = request.Value;
        setting.Description = request.Description;
        setting.UpdatedAt = DateTime.UtcNow;
        await repository.SaveAsync(cancellationToken);
        if (key == PortalEventThemeKey) cache.Remove(PortalEventThemeCacheKey);
        return ToResponse(setting);
    }

    public async Task<PortalEventThemeResponse> GetPortalEventThemeAsync(CancellationToken cancellationToken)
    {
        return await cache.GetOrCreateAsync(PortalEventThemeCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
            var setting = await repository.GetAsync(PortalEventThemeKey, cancellationToken);
            if (setting is null)
            {
                return new PortalEventThemeResponse { Enabled = true, EventKey = "october20" };
            }

            try
            {
                var configured = JsonSerializer.Deserialize<PortalEventThemeRequest>(setting.Value);
                if (configured is not null && PortalEventKeys.Contains(configured.EventKey))
                {
                    return new PortalEventThemeResponse
                    {
                        Enabled = configured.Enabled && configured.EventKey != "none",
                        EventKey = configured.Enabled ? configured.EventKey : "none",
                        Messages = NormalizeMessages(configured.Messages)
                    };
                }
            }
            catch (JsonException)
            {
                // Fall back safely if a stored value was edited outside the settings UI.
            }

            return new PortalEventThemeResponse { Enabled = false, EventKey = "none" };
        }) ?? new PortalEventThemeResponse { Enabled = false, EventKey = "none" };
    }

    public async Task<PortalEventThemeResponse> UpdatePortalEventThemeAsync(PortalEventThemeRequest request, CancellationToken cancellationToken)
    {
        if (!PortalEventKeys.Contains(request.EventKey))
        {
            throw new ApiException(400, "Sự kiện giao diện không hợp lệ.");
        }

        var normalized = new PortalEventThemeRequest
        {
            Enabled = request.Enabled && request.EventKey != "none",
            EventKey = request.Enabled ? request.EventKey : "none",
            Messages = NormalizeMessages(request.Messages).ToDictionary(
                item => item.Key,
                item => new PortalEventTextRequest { Title = item.Value.Title, Description = item.Value.Description })
        };
        var serialized = JsonSerializer.Serialize(normalized);
        await UpsertAsync(PortalEventThemeKey, new SystemSettingRequest
        {
            Value = serialized,
            Description = "Bật/tắt chủ đề sự kiện trên cổng Gold Central"
        }, cancellationToken);

        return new PortalEventThemeResponse
        {
            Enabled = normalized.Enabled,
            EventKey = normalized.EventKey,
            Messages = NormalizeMessages(normalized.Messages)
        };
    }

    private static Dictionary<string, PortalEventTextResponse> NormalizeMessages(
        IReadOnlyDictionary<string, PortalEventTextRequest>? messages)
    {
        var normalized = new Dictionary<string, PortalEventTextResponse>(StringComparer.Ordinal);
        if (messages is null) return normalized;

        foreach (var (key, value) in messages)
        {
            if (value is null || !PortalEventKeys.Contains(key) || key == "none") continue;
            normalized[key] = new PortalEventTextResponse
            {
                Title = value.Title?.Trim() ?? string.Empty,
                Description = value.Description?.Trim() ?? string.Empty
            };
        }

        return normalized;
    }

    private static SystemSettingResponse ToResponse(SystemSetting item)
    {
        return new SystemSettingResponse { Key = item.Key, Value = item.Value, Description = item.Description, UpdatedAt = item.UpdatedAt };
    }
}
