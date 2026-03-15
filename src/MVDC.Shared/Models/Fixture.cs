namespace MVDC.Shared.Models;

public class Fixture
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TeamId { get; set; } = string.Empty;
    public string Opponent { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Venue { get; set; } = string.Empty;
    public bool IsHome { get; set; }
    public string DocumentType { get; set; } = "Fixture";
}
