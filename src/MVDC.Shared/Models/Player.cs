using System.ComponentModel.DataAnnotations;

namespace MVDC.Shared.Models;

public class Player
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public string Position { get; set; } = string.Empty;

    public string? TeamId { get; set; }
    public string DocumentType { get; init; } = nameof(Player);
}
