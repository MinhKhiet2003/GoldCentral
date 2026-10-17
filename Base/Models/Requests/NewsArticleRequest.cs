using System.ComponentModel.DataAnnotations;
namespace Base.Models.Requests;

public class NewsArticleRequest
{
    [Required, StringLength(160)] public string Title { get; set; } = "";
    [StringLength(400)] public string Summary { get; set; } = "";
    [Required, StringLength(30000)] public string Content { get; set; } = "";
    [StringLength(2000)] public string? ImageUrl { get; set; }
    [Required, RegularExpression("^(all|vaccination|dermatology|rehabilitation|wellness|office)$")]
    public string Department { get; set; } = "all";
    public bool IsPublished { get; set; }
}
