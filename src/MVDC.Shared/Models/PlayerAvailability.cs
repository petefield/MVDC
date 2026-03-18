using System.ComponentModel.DataAnnotations;

namespace MVDC.Shared.Models;

public class PlayerAvailability
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string PlayerId { get; set; } = string.Empty;

    [Required]
    public string FixtureId { get; set; } = string.Empty;

    public string ParentId { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public string? Notes { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string DocumentType { get; init; } = nameof(PlayerAvailability);
}
