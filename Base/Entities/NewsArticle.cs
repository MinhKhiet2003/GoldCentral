namespace Base.Entities;

public class NewsArticle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Content { get; set; } = "";
    public string? ImageUrl { get; set; }
    public string Department { get; set; } = "all";
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public long ViewCount { get; set; }
}
