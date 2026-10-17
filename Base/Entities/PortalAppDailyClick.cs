namespace Base.Entities;

public class PortalAppDailyClick
{
    public Guid AppId { get; set; }
    public DateOnly Date { get; set; }
    public long Clicks { get; set; }
}
