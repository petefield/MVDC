using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Cosmos;
using MVDC.Shared.Models;
using System.Net;

namespace MVDC.Api.Identity;

public class CosmosUserStore :
    IUserStore<ApplicationUser>,
    IUserPasswordStore<ApplicationUser>,
    IUserRoleStore<ApplicationUser>,
    IUserEmailStore<ApplicationUser>
{
    private readonly Container _container;

    public CosmosUserStore(CosmosClient cosmosClient, IConfiguration configuration)
    {
        var databaseName = configuration["CosmosDb:DatabaseName"] ?? "MVDC";
        var containerName = configuration["CosmosDb:ContainerName"] ?? "Items";
        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    // IUserStore

    public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        await _container.CreateItemAsync(user, new PartitionKey(user.Id), cancellationToken: cancellationToken);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        await _container.UpsertItemAsync(user, new PartitionKey(user.Id), cancellationToken: cancellationToken);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        await _container.DeleteItemAsync<ApplicationUser>(user.Id, new PartitionKey(user.Id), cancellationToken: cancellationToken);
        return IdentityResult.Success;
    }

    public async Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _container.ReadItemAsync<ApplicationUser>(userId, new PartitionKey(userId), cancellationToken: cancellationToken);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.documentType = 'User' AND c.normalizedUserName = @name")
            .WithParameter("@name", normalizedUserName);
        return await QuerySingleAsync(query, cancellationToken);
    }

    public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        Task.FromResult<string?>(user.NormalizedUserName);

    public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        Task.FromResult(user.Id);

    public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        Task.FromResult<string?>(user.Email);

    public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken cancellationToken)
    {
        user.Email = userName ?? string.Empty;
        return Task.CompletedTask;
    }

    // IUserPasswordStore

    public Task SetPasswordHashAsync(ApplicationUser user, string? passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        Task.FromResult(user.PasswordHash);

    public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));

    // IUserRoleStore

    public Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        user.Role = roleName;
        return Task.CompletedTask;
    }

    public Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        if (string.Equals(user.Role, roleName, StringComparison.OrdinalIgnoreCase))
            user.Role = string.Empty;
        return Task.CompletedTask;
    }

    public Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        Task.FromResult<IList<string>>(string.IsNullOrEmpty(user.Role) ? [] : [user.Role]);

    public Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken) =>
        Task.FromResult(string.Equals(user.Role, roleName, StringComparison.OrdinalIgnoreCase));

    public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        var normalizedRole = roleName.ToUpperInvariant();
        // Role is stored as-is (e.g. "Admin"), but we compare case-insensitively via UPPER().
        var query = new QueryDefinition("SELECT * FROM c WHERE c.documentType = 'User' AND UPPER(c.role) = @role")
            .WithParameter("@role", normalizedRole);
        return await QueryListAsync(query, cancellationToken);
    }

    // IUserEmailStore

    public Task SetEmailAsync(ApplicationUser user, string? email, CancellationToken cancellationToken)
    {
        user.Email = email ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        Task.FromResult<string?>(user.Email);

    public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        Task.FromResult(true); // We auto-confirm emails for this app

    public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public async Task<ApplicationUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.documentType = 'User' AND c.normalizedEmail = @email")
            .WithParameter("@email", normalizedEmail);
        return await QuerySingleAsync(query, cancellationToken);
    }

    public Task<string?> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        Task.FromResult<string?>(user.NormalizedEmail);

    public Task SetNormalizedEmailAsync(ApplicationUser user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        user.NormalizedEmail = normalizedEmail ?? string.Empty;
        return Task.CompletedTask;
    }

    // Helpers

    private async Task<ApplicationUser?> QuerySingleAsync(QueryDefinition query, CancellationToken cancellationToken)
    {
        using var feed = _container.GetItemQueryIterator<ApplicationUser>(query);
        while (feed.HasMoreResults)
        {
            var response = await feed.ReadNextAsync(cancellationToken);
            if (response.Any())
                return response.First();
        }
        return null;
    }

    private async Task<IList<ApplicationUser>> QueryListAsync(QueryDefinition query, CancellationToken cancellationToken)
    {
        var results = new List<ApplicationUser>();
        using var feed = _container.GetItemQueryIterator<ApplicationUser>(query);
        while (feed.HasMoreResults)
        {
            var response = await feed.ReadNextAsync(cancellationToken);
            results.AddRange(response);
        }
        return results;
    }

    public void Dispose() { }
}
