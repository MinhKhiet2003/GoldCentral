using Base.Data;
using Base.Entities;
using Base.Repositories.IRepositories;
using Base.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace Base.Repositories;

public sealed class PortalRepository(AppDbContext db) : IPortalRepository
{
    public Task<List<PortalApp>> ListAsync(bool includeHidden, CancellationToken ct) => db.PortalApps
        .AsNoTracking().Where(item => includeHidden || item.IsVisible)
        .OrderBy(item => item.Order).ThenBy(item => item.Name).ToListAsync(ct);

    public Task<PortalApp?> GetAsync(Guid id, CancellationToken ct) => db.PortalApps
        .SingleOrDefaultAsync(item => item.Id == id, ct);

    public async Task AddAsync(PortalApp item, CancellationToken ct)
    {
        db.PortalApps.Add(item);
        await db.SaveChangesAsync(ct);
    }

    public async Task SaveAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct) => await db.PortalApps
        .Where(item => item.Id == id).ExecuteDeleteAsync(ct) > 0;

    // Increment in SQL, preserving clicks across concurrent requests and multiple server instances.
    // Match the URL and visibility again to avoid counting a stale/hidden link.
    public async Task<bool> IncrementClicksAsync(Guid id, string expectedUrl, DateOnly date, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var changed = await db.PortalApps
            .Where(item => item.Id == id && item.IsVisible && item.Url == expectedUrl)
            .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.Clicks, item => item.Clicks + 1), ct);
        if (changed == 0) return false;

        // PostgreSQL upsert is atomic even when the first clicks of a day arrive simultaneously.
        // Both the lifetime counter and daily bucket commit together, or neither does.
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "PortalAppDailyClick" ("AppId", "Date", "Clicks")
            VALUES ({id}, {date}, 1)
            ON CONFLICT ("AppId", "Date")
            DO UPDATE SET "Clicks" = "PortalAppDailyClick"."Clicks" + 1
            """, ct);
        await transaction.CommitAsync(ct);
        return true;
    }

    public Task<List<PortalStatisticsRow>> StatisticsAsync(PortalStatisticsResponse periods, CancellationToken ct) =>
        db.PortalApps.AsNoTracking().OrderBy(item => item.Order).ThenBy(item => item.Name)
            .Select(item => new PortalStatisticsRow
            {
                Id = item.Id, Name = item.Name, Category = item.Category, IsVisible = item.IsVisible,
                DayClicks = db.PortalAppDailyClicks.Where(click => click.AppId == item.Id && click.Date == periods.Date)
                    .Sum(click => (long?)click.Clicks) ?? 0,
                RangeClicks = db.PortalAppDailyClicks.Where(click => click.AppId == item.Id && click.Date >= periods.RangeStart && click.Date <= periods.RangeEnd)
                    .Sum(click => (long?)click.Clicks) ?? 0,
                WeekClicks = db.PortalAppDailyClicks.Where(click => click.AppId == item.Id && click.Date >= periods.WeekStart && click.Date <= periods.WeekEnd)
                    .Sum(click => (long?)click.Clicks) ?? 0,
                MonthClicks = db.PortalAppDailyClicks.Where(click => click.AppId == item.Id && click.Date >= periods.MonthStart && click.Date <= periods.MonthEnd)
                    .Sum(click => (long?)click.Clicks) ?? 0,
                TotalClicks = item.Clicks
            }).ToListAsync(ct);
}
