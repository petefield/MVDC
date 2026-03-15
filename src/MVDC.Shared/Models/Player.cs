namespace MVDC.Shared.Models;

public class Player
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Position { get; set; } = string.Empty;
    public string? TeamId { get; set; }
    public string DocumentType { get; set; } = "Player";
}
