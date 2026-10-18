using System.ComponentModel.DataAnnotations;

namespace Base.Models.Requests;

public class PortalEventThemeRequest
{
    public bool Enabled { get; set; } = true;

    [Required, StringLength(40)]
    public string EventKey { get; set; } = "october20";

    public Dictionary<string, PortalEventTextRequest> Messages { get; set; } = new();
}

public class PortalEventTextRequest
{
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(300)]
    public string Description { get; set; } = string.Empty;
}
