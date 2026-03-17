namespace MVDC.Shared.Models;

public class ApplicationUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string NormalizedUserName { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString();
    public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
    public string Role { get; set; } = "Parent";
    public string DocumentType { get; set; } = "User";
}
