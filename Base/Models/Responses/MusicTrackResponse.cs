namespace Base.Models.Responses;

public class MusicTrackResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Artist { get; set; }
    public string Department { get; set; } = "all";
    public bool IsPublished { get; set; }
    public int SortOrder { get; set; }
    public long Size { get; set; }
    public double? DurationSeconds { get; set; }
    public DateTime CreatedAt { get; set; }
    public string StreamUrl { get; set; } = string.Empty;
}
