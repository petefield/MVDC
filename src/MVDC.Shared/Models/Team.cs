using System.ComponentModel.DataAnnotations;

namespace MVDC.Shared.Models;

public class Team
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string Name { get; set; } = string.Empty;

    public string CoachId { get; set; } = string.Empty;
    public List<string> PlayerIds { get; set; } = new();
    public string DocumentType { get; init; } = nameof(Team);
}
