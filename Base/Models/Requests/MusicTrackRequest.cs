using System.ComponentModel.DataAnnotations;

namespace Base.Models.Requests;

public class MusicTrackRequest
{
    [Required, StringLength(160, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(120)]
    public string? Artist { get; set; }

    [Required, StringLength(30)]
    public string Department { get; set; } = "all";

    [Range(1, 10000)]
    public int SortOrder { get; set; } = 1;

    public bool IsPublished { get; set; } = true;

    public IFormFile? File { get; set; }
}
