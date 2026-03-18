using System.ComponentModel.DataAnnotations;

namespace MVDC.Shared.Models;

public class MatchReport
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string FixtureId { get; set; } = string.Empty;

    public string CoachId { get; set; } = string.Empty;

    [Required]
    public string Report { get; set; } = string.Empty;

    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public List<string> PhotoUrls { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string DocumentType { get; init; } = nameof(MatchReport);
}
