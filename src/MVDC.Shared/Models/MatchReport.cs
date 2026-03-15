namespace MVDC.Shared.Models;

public class MatchReport
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FixtureId { get; set; } = string.Empty;
    public string CoachId { get; set; } = string.Empty;
    public string Report { get; set; } = string.Empty;
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public List<string> PhotoUrls { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string DocumentType { get; set; } = "MatchReport";
}
