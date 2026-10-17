namespace Base.Entities;

public class PortalApp
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "rehabilitation";
    public string Platform { get; set; } = "system";
    public string? Url { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsFeatured { get; set; }
    public int Order { get; set; }
    public long Clicks { get; set; }
}
