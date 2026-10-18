namespace Base.Models.Responses;

public class PortalEventThemeResponse
{
    public bool Enabled { get; set; }
    public string EventKey { get; set; } = "none";
    public Dictionary<string, PortalEventTextResponse> Messages { get; set; } = new();
}

public class PortalEventTextResponse
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
