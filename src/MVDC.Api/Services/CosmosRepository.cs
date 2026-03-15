using Microsoft.Azure.Cosmos;
using System.Net;

namespace MVDC.Api.Services;

public class CosmosRepository<T> : IRepository<T> where T : class
{
    private readonly Container _container;
    private readonly string _documentType;

    public CosmosRepository(CosmosClient cosmosClient, IConfiguration configuration, string documentType)
    {
        var databaseName = configuration["CosmosDb:DatabaseName"] ?? "MVDC";
        var containerName = configuration["CosmosDb:ContainerName"] ?? "Items";
        _container = cosmosClient.GetContainer(databaseName, containerName);
        _documentType = documentType;
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.documentType = @type")
            .WithParameter("@type", _documentType);
        var results = new List<T>();
        using var feed = _container.GetItemQueryIterator<T>(query);
        while (feed.HasMoreResults)
        {
            var response = await feed.ReadNextAsync();
            results.AddRange(response);
        }
        return results;
    }

    public async Task<T?> GetByIdAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<T>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<T> CreateAsync(T item)
    {
        var response = await _container.CreateItemAsync(item);
        return response.Resource;
    }

    public async Task<T> UpdateAsync(string id, T item)
    {
        var response = await _container.UpsertItemAsync(item);
        return response.Resource;
    }

    public async Task DeleteAsync(string id)
    {
        await _container.DeleteItemAsync<T>(id, new PartitionKey(id));
    }
}
