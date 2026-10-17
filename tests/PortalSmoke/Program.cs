using Base.Data;
using Base.Common;
using Base.Models.Responses;
using Base.Entities;
using Base.Exceptions;
using Base.Models.Requests;
using Base.Repositories.IRepositories;
using Base.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

void Check(bool value, string name) { if (!value) throw new Exception(name); Console.WriteLine("PASS " + name); }
using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
    .UseNpgsql("Host=localhost;Database=portal_model_check;Username=unused;Password=unused").Options);
var model = db.GetService<IDesignTimeModel>().Model;
var entity = model.FindEntityType(typeof(PortalApp))!;
Check(entity.GetTableName() == "PortalApp", "PostgreSQL portal table mapping");
Check(entity.GetSeedData().Count() == 9, "nine deterministic seed applications");
Check(entity.FindProperty(nameof(PortalApp.Name))!.GetMaxLength() == 100, "name length constraint");
Check(entity.GetCheckConstraints().Count() == 4, "category platform order and click constraints");
var categoryConstraint = entity.GetCheckConstraints().Single(item => item.Name == "CK_PortalApp_Category").Sql!;
Check(categoryConstraint.Contains("'rehabilitation'") && categoryConstraint.Contains("'wellness'") && !categoryConstraint.Contains("'general'"), "department categories split into rehabilitation and wellness");
Check(entity.GetIndexes().Count() == 2, "directory query indexes");

var repository = new FakePortalRepository();
var service = new PortalService(repository, TimeProvider.System);
var ct = CancellationToken.None;
var request = new PortalAppRequest { Name = "Smoke app", Url = "https://example.com/", Category = "office", Platform = "system" };
var app = await service.SaveAsync(null, request, ct);
Check((await service.VisitAsync(app.Id, ct)) == request.Url, "configured visit returns URL");
Check(repository.Items.Single().Clicks == 1, "successful visit increments count");
request.IsVisible = false;
await service.SaveAsync(app.Id, request, ct);
Check((await service.ListAsync(false, ct)).Count == 0, "hidden application excluded");
try { await service.VisitAsync(app.Id, ct); throw new Exception("Hidden visit allowed"); }
catch (ApiException e) when (e.StatusCode == 404) { Console.WriteLine("PASS hidden visit blocked"); }
request.Url = "javascript:alert(1)";
try { await service.SaveAsync(app.Id, request, ct); throw new Exception("Unsafe URL accepted"); }
catch (ApiException e) when (e.StatusCode == 400) { Console.WriteLine("PASS unsafe URL rejected"); }
request.Url = null; request.IsVisible = true;
await service.SaveAsync(app.Id, request, ct);
try { await service.VisitAsync(app.Id, ct); throw new Exception("Unconfigured visit allowed"); }
catch (ApiException e) when (e.StatusCode == 400) { Console.WriteLine("PASS unconfigured visit blocked"); }
Check(repository.Items.Single().Clicks == 1, "failed visits preserve count");
request.Url = "https://example.com/";
await service.SaveAsync(app.Id, request, ct);
repository.RejectIncrement = true;
try { await service.VisitAsync(app.Id, ct); throw new Exception("Stale visit allowed"); }
catch (ApiException e) when (e.StatusCode == 409) { Console.WriteLine("PASS concurrent link change handled"); }
Check(await service.DeleteAsync(app.Id, ct), "delete application");

var orderOne = await service.SaveAsync(null, new PortalAppRequest { Name = "Hệ thống 1", Order = 1 }, ct);
var orderTwo = await service.SaveAsync(null, new PortalAppRequest { Name = "Hệ thống 2", Order = 2 }, ct);
var orderThree = await service.SaveAsync(null, new PortalAppRequest { Name = "Hệ thống 3", Order = 3 }, ct);
await service.SaveAsync(orderThree.Id, new PortalAppRequest { Name = "Hệ thống 3", Order = 1 }, ct);
var reordered = await service.ListAsync(true, ct);
Check(reordered.Select(x => x.Name).SequenceEqual(["Hệ thống 3", "Hệ thống 1", "Hệ thống 2"]), "moving an app to position one shifts following apps down");
Check(reordered.Select(x => x.Order).SequenceEqual([1, 2, 3]), "reordered apps have consecutive positions");
Check(await service.DeleteAsync(orderOne.Id, ct), "delete reorders remaining applications");
Check((await service.ListAsync(true, ct)).Select(x => x.Order).SequenceEqual([1, 2]), "delete closes the order gap");

Check(PortalReportingCalendar.VietnamDate(DateTimeOffset.Parse("2026-09-30T16:59:59Z")) == new DateOnly(2026,9,30), "Vietnam date before local midnight");
Check(PortalReportingCalendar.VietnamDate(DateTimeOffset.Parse("2026-09-30T17:00:00Z")) == new DateOnly(2026,10,1), "Vietnam date after local midnight");
var sunday = PortalReportingCalendar.Periods(new DateOnly(2026,10,4));
Check(sunday.WeekStart == new DateOnly(2026,9,28) && sunday.WeekEnd == new DateOnly(2026,10,4), "Sunday belongs to previous Monday week");
var monday = PortalReportingCalendar.Periods(new DateOnly(2026,10,5));
Check(monday.WeekStart == new DateOnly(2026,10,5), "Monday starts a new week");
var leap = PortalReportingCalendar.Periods(new DateOnly(2028,2,29));
Check(leap.MonthEnd == new DateOnly(2028,2,29), "leap year month boundary");
var year = PortalReportingCalendar.Periods(new DateOnly(2027,1,1));
Check(year.WeekStart == new DateOnly(2026,12,28) && year.WeekEnd == new DateOnly(2027,1,3), "week crosses calendar year");
request.IsFeatured = true;
var featured = await service.SaveAsync(null, request, ct);
Check(featured.IsFeatured, "admin saves featured flag");
request.IsFeatured = false;
Check(!(await service.SaveAsync(featured.Id, request, ct)).IsFeatured, "admin removes featured flag");
Check(model.FindEntityType(typeof(PortalAppDailyClick))!.FindPrimaryKey()!.Properties.Count == 2, "daily buckets have composite key");
if (args.Contains("--postgres")) await IntegrationChecks.RunAsync(args.Contains("--serve"));

// Service tests use an isolated fake. No connection to the user's PostgreSQL database.
sealed class FakePortalRepository : IPortalRepository
{
    public Task<List<PortalStatisticsRow>> StatisticsAsync(PortalStatisticsResponse periods, CancellationToken ct) => Task.FromResult(new List<PortalStatisticsRow>());
    public List<PortalApp> Items { get; } = [];
    public bool RejectIncrement { get; set; }
    public Task<List<PortalApp>> ListAsync(bool includeHidden, CancellationToken ct) => Task.FromResult(Items.Where(x => includeHidden || x.IsVisible).OrderBy(x => x.Order).ToList());
    public Task<PortalApp?> GetAsync(Guid id, CancellationToken ct) => Task.FromResult(Items.SingleOrDefault(x => x.Id == id));
    public Task SaveWithOrderAsync(PortalApp item, int position, CancellationToken ct)
    {
        Items.RemoveAll(x => x.Id == item.Id);
        var index = Math.Clamp(position - 1, 0, Items.Count);
        Items.Insert(index, item);
        for (var i = 0; i < Items.Count; i++) Items[i].Order = i;
        return Task.CompletedTask;
    }
    public Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var removed = Items.RemoveAll(x => x.Id == id) > 0;
        var ordered = Items.OrderBy(x => x.Order).ThenBy(x => x.Name).ToList();
        for (var i = 0; i < ordered.Count; i++) ordered[i].Order = i;
        return Task.FromResult(removed);
    }
    public Task<bool> IncrementClicksAsync(Guid id, string url, DateOnly date, CancellationToken ct)
    {
        var item = Items.SingleOrDefault(x => x.Id == id && x.Url == url && x.IsVisible);
        if (item is null || RejectIncrement) return Task.FromResult(false);
        item.Clicks++; return Task.FromResult(true);
    }
}


