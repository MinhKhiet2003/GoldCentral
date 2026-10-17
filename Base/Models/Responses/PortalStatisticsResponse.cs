namespace Base.Models.Responses;

public class PortalStatisticsResponse
{
    public DateOnly Date { get; set; }
    public DateOnly WeekStart { get; set; }
    public DateOnly WeekEnd { get; set; }
    public DateOnly MonthStart { get; set; }
    public DateOnly MonthEnd { get; set; }
    public DateOnly RangeStart { get; set; }
    public DateOnly RangeEnd { get; set; }
    public string TimeZone { get; set; } = "Asia/Ho_Chi_Minh (UTC+07:00)";
    public List<PortalStatisticsRow> Items { get; set; } = [];
}

public class PortalStatisticsRow
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public bool IsVisible { get; set; }
    public long DayClicks { get; set; }
    public long RangeClicks { get; set; }
    public long WeekClicks { get; set; }
    public long MonthClicks { get; set; }
    public long TotalClicks { get; set; }
}
