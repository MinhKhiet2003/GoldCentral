using Base.Models.Responses;

namespace Base.Common;

public static class PortalReportingCalendar
{
    public static DateOnly VietnamDate(DateTimeOffset instant) =>
        DateOnly.FromDateTime(instant.ToOffset(TimeSpan.FromHours(7)).DateTime);

    public static PortalStatisticsResponse Periods(DateOnly date)
    {
        var weekStart = date.AddDays(-((int)date.DayOfWeek + 6) % 7);
        var monthStart = new DateOnly(date.Year, date.Month, 1);
        return new()
        {
            Date = date, WeekStart = weekStart, WeekEnd = weekStart.AddDays(6),
            MonthStart = monthStart, MonthEnd = monthStart.AddMonths(1).AddDays(-1)
        };
    }
}
