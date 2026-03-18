using System.ComponentModel.DataAnnotations;

namespace MVDC.Shared.Models;

public class Parent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, Phone]
    public string Phone { get; set; } = string.Empty;

    public List<string> PlayerIds { get; set; } = new();
    public string DocumentType { get; init; } = nameof(Parent);
}
