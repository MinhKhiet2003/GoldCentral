using Base.Common;
using System.Diagnostics;
using Base.Data;
using Base.Entities;
using Base.Models.Requests;
using Base.Repositories;
using Base.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;

internal static class IntegrationChecks
{
    public static async Task RunAsync(bool serve = false)
    {
        using var configured = new AppDbContextFactory().CreateDbContext([]);
        var settings = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("GOLD_PORTAL_TEST_CONNECTION") ?? configured.Database.GetConnectionString());
        var testDatabase = "goldportal_smoke_" + Guid.NewGuid().ToString("N");
        var serverSettings = new NpgsqlConnectionStringBuilder(settings.ConnectionString) { Database = "postgres", Pooling = false };
        settings.Database = testDatabase; settings.Pooling = false;
        await using var server = new NpgsqlConnection(serverSettings.ConnectionString);
        await server.OpenAsync();
        await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{testDatabase}\"", server)) await create.ExecuteNonQueryAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(settings.ConnectionString).Options;
        AppDbContext Open() => new(options);
        void Check(bool condition, string name) { if (!condition) throw new Exception(name); Console.WriteLine("PASS PostgreSQL " + name); }
        try
        {
            await using (var db = Open())
            {
                await db.Database.MigrateAsync();
                Check(await db.PortalApps.CountAsync() == 9, "full migration chain and seeds");
                Check(!await db.PortalApps.AnyAsync(x => x.IsFeatured), "featured requires admin selection");
            }
            Guid id;
            var request = new PortalAppRequest { Name = "Integration app", Category = "office", Platform = "website", Url = "https://example.com/", IsFeatured = true };
            await using (var db = Open())
            {
                var app = await new PortalService(new PortalRepository(db), TimeProvider.System).SaveAsync(null, request, default);
                id = app.Id;
                Check(app.Platform == "website", "website platform saved and returned");
                await db.PortalApps.Where(x => x.Id == id).ExecuteUpdateAsync(s => s.SetProperty(x => x.Clicks, 100L));
            }
            async Task Visit(string instant)
            {
                await using var db = Open();
                var service = new PortalService(new PortalRepository(db), new FixedClock(DateTimeOffset.Parse(instant)));
                await service.VisitAsync(id, default);
            }
            await Visit("2026-09-30T16:59:59Z");
            await Visit("2026-09-30T17:00:00Z");
            await Visit("2026-10-04T16:59:59Z");
            await Visit("2026-10-04T17:00:00Z");
            await Task.WhenAll(Enumerable.Range(0,20).Select(_ => Visit("2026-10-01T03:00:00Z")));
            await using (var db = Open())
            {
                var service = new PortalService(new PortalRepository(db), TimeProvider.System);
                var report = await service.StatisticsAsync(new DateOnly(2026,10,1), default);
                var row = report.Items.Single(x => x.Id == id);
                Check(row.DayClicks == 21 && row.WeekClicks == 23 && row.MonthClicks == 23 && row.TotalClicks == 124, "day week month lifetime counts with concurrent clicks");
                var nextWeek = (await service.StatisticsAsync(new DateOnly(2026,10,5), default)).Items.Single(x => x.Id == id);
                Check(nextWeek.DayClicks == 1 && nextWeek.WeekClicks == 1 && nextWeek.MonthClicks == 23, "Sunday Monday boundary");
                var previousMonth = (await service.StatisticsAsync(new DateOnly(2026,9,30), default)).Items.Single(x => x.Id == id);
                Check(previousMonth.DayClicks == 1 && previousMonth.MonthClicks == 1, "local midnight month boundary");
                Check(await db.PortalAppDailyClicks.Where(x => x.AppId == id).SumAsync(x => x.Clicks) == 24, "legacy lifetime count not assigned to daily buckets");
                Check((await db.PortalApps.AsNoTracking().SingleAsync(x => x.Id == id)).IsFeatured, "featured persisted in PostgreSQL");
                var beforeEdit = await new PortalRepository(db).GetAsync(id, default);
                await Visit("2026-10-01T03:00:00Z");
                request.IsFeatured = false;
                await service.SaveAsync(id, request, default);
                Check((await db.PortalApps.AsNoTracking().SingleAsync(x => x.Id == id)).Clicks == 125, "editing card preserves concurrent counters");
                Check(!await new PortalRepository(db).IncrementClicksAsync(id, "https://stale.example.com/", new DateOnly(2026,10,1), default), "stale link rejected");
                Check((await db.PortalApps.AsNoTracking().SingleAsync(x => x.Id == id)).Clicks == 125, "rejected link preserves lifetime count");
                request.IsVisible = false;
                await service.SaveAsync(id, request, default);
                Check(!await new PortalRepository(db).IncrementClicksAsync(id, request.Url!, new DateOnly(2026,10,1), default), "hidden application cannot record clicks");
                await service.DeleteAsync(id, default);
                Check(!await db.PortalAppDailyClicks.AnyAsync(x => x.AppId == id), "daily buckets cascade when card deleted");
            }
            await NewsChecks.RunAsync(options);
            if (serve)
            {
                await using (var db = Open())
                {
                    var seedId = Guid.Parse("00000000-0000-0000-0000-000000000001");
                    await db.PortalApps.Where(x => x.Id == seedId).ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.IsFeatured, true).SetProperty(x => x.Url, "https://example.com/"));
                    await new NewsService(new NewsRepository(db), TimeProvider.System).SaveAsync(null, new NewsArticleRequest { Title = "Thông báo nội bộ (dữ liệu kiểm thử)", Summary = "Cập nhật hoạt động trong hệ sinh thái Gold.", Content = "Bài viết dùng để kiểm tra giao diện tin tức nội bộ.\nNội dung này chỉ nằm trong database tạm.", Department = "all", IsPublished = true }, default);
                    var repo = new PortalRepository(db);
                    await repo.IncrementClicksAsync(seedId, "https://example.com/", PortalReportingCalendar.VietnamDate(DateTimeOffset.UtcNow), default);
                }
                var appRoot = Path.GetFullPath(Path.Combine("Base", "Base"));
                var start = new ProcessStartInfo("dotnet") { WorkingDirectory = appRoot, UseShellExecute = false, CreateNoWindow = true };
                start.ArgumentList.Add(Path.Combine(appRoot, "bin", "Release", "net10.0", "Base.dll"));
                start.ArgumentList.Add("--urls"); start.ArgumentList.Add("http://localhost:5090");
                start.Environment["ConnectionStrings__DefaultConnection"] = settings.ConnectionString;
                using var process = Process.Start(start) ?? throw new Exception("Cannot start isolated preview.");
                try
                {
                    Console.WriteLine("Isolated test preview: http://localhost:5090 — press Enter to stop and remove its temporary database.");
                    await Console.In.ReadLineAsync();
                }
                finally { if (!process.HasExited) { process.Kill(entireProcessTree: true); await process.WaitForExitAsync(); } }
            }
        }
        finally
        {
            // Only the disposable database created above can be removed by this test.
            if (testDatabase.StartsWith("goldportal_smoke_") && testDatabase.Length == 49)
            {
                await using var drop = new NpgsqlCommand($"DROP DATABASE \"{testDatabase}\" WITH (FORCE)", server);
                await drop.ExecuteNonQueryAsync();
            }
        }
    }
    private sealed class FixedClock(DateTimeOffset instant) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => instant;
    }
}




