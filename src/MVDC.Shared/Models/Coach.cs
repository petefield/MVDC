using System.ComponentModel.DataAnnotations;

namespace MVDC.Shared.Models;

public class Coach
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, Phone]
    public string Phone { get; set; } = string.Empty;

    public string DocumentType { get; init; } = nameof(Coach);
}
