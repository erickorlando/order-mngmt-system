using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Vendors.Application.Interfaces;

namespace Vendors.Infrastructure.Services;

public class CosmosDbService : ICosmosDbService
{
    private readonly Container _container;

    public CosmosDbService(IConfiguration configuration)
    {
        var cosmosConfig = configuration.GetSection("CosmosDB");
        var client = new CosmosClient(cosmosConfig["EndpointUri"], cosmosConfig["PrimaryKey"]);
        
        var database = client.GetDatabase(cosmosConfig["DatabaseName"]);
        _container = database.GetContainer(cosmosConfig["ContainerName"]);
    }

    public async Task<T?> GetItemAsync<T>(string id, string partitionKey) where T : class
    {
        try
        {
            var response = await _container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<T> CreateItemAsync<T>(T item, string partitionKey) where T : class
    {
        var response = await _container.CreateItemAsync(item, new PartitionKey(partitionKey));
        return response.Resource;
    }

    public async Task<T> UpdateItemAsync<T>(T item, string id, string partitionKey) where T : class
    {
        var response = await _container.ReplaceItemAsync(item, id, new PartitionKey(partitionKey));
        return response.Resource;
    }

    public async Task DeleteItemAsync(string id, string partitionKey)
    {
        await _container.DeleteItemAsync<object>(id, new PartitionKey(partitionKey));
    }

    public async Task<IEnumerable<T>> QueryItemsAsync<T>(string query) where T : class
    {
        var queryDefinition = new QueryDefinition(query);
        var queryResultSetIterator = _container.GetItemQueryIterator<T>(queryDefinition);

        var results = new List<T>();
        while (queryResultSetIterator.HasMoreResults)
        {
            var currentResultSet = await queryResultSetIterator.ReadNextAsync();
            results.AddRange(currentResultSet);
        }

        return results;
    }
} 