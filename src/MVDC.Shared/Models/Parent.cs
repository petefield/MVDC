namespace MVDC.Shared.Models;

public class Parent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public List<string> PlayerIds { get; set; } = new();
    public string DocumentType { get; set; } = "Parent";
}
