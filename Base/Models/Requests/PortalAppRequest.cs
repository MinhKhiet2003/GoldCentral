using System.ComponentModel.DataAnnotations;

namespace Base.Models.Requests;

public class PortalAppRequest
{
    [Required, StringLength(100)] public string Name { get; set; } = "";
    [StringLength(400)] public string Description { get; set; } = "";
    [Required, RegularExpression("^(vaccination|dermatology|general|office)$")] public string Category { get; set; } = "general";
    [Required, RegularExpression("^(system|facebook|zalo|tiktok|youtube|website)$")] public string Platform { get; set; } = "system";
    [StringLength(2000)] public string? Url { get; set; }
    [StringLength(2000)] public string? ImageUrl { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsFeatured { get; set; }
    [Range(0, 10000)] public int Order { get; set; }
}

