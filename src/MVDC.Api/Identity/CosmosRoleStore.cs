using Microsoft.AspNetCore.Identity;

namespace MVDC.Api.Identity;

/// <summary>
/// Minimal role store. This application uses a single Role property on ApplicationUser
/// rather than a full roles collection. The store satisfies the Identity framework contract.
/// </summary>
public class CosmosRoleStore : IRoleStore<IdentityRole>
{
    // We support three fixed roles — no need to persist them in Cosmos.
    private static readonly Dictionary<string, IdentityRole> Roles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Admin"] = new IdentityRole { Id = "admin", Name = "Admin", NormalizedName = "ADMIN" },
        ["Coach"] = new IdentityRole { Id = "coach", Name = "Coach", NormalizedName = "COACH" },
        ["Parent"] = new IdentityRole { Id = "parent", Name = "Parent", NormalizedName = "PARENT" },
    };

    public Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken) =>
        Task.FromResult(IdentityResult.Success);

    public Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken) =>
        Task.FromResult(IdentityResult.Success);

    public Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken) =>
        Task.FromResult(IdentityResult.Success);

    public Task<IdentityRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        var role = Roles.Values.FirstOrDefault(r => string.Equals(r.Id, roleId, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(role);
    }

    public Task<IdentityRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        var role = Roles.Values.FirstOrDefault(r => string.Equals(r.NormalizedName, normalizedRoleName, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(role);
    }

    public Task<string?> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken) =>
        Task.FromResult(role.NormalizedName);

    public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken) =>
        Task.FromResult(role.Id!);

    public Task<string?> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken) =>
        Task.FromResult(role.Name);

    public Task SetNormalizedRoleNameAsync(IdentityRole role, string? normalizedName, CancellationToken cancellationToken)
    {
        role.NormalizedName = normalizedName;
        return Task.CompletedTask;
    }

    public Task SetRoleNameAsync(IdentityRole role, string? roleName, CancellationToken cancellationToken)
    {
        role.Name = roleName;
        return Task.CompletedTask;
    }

    public void Dispose() { }
}
