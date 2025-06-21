namespace Vendors.Application.Interfaces;

public interface ICosmosDbService
{
    Task<T?> GetItemAsync<T>(string id, string partitionKey) where T : class;
    Task<T> CreateItemAsync<T>(T item, string partitionKey) where T : class;
    Task<T> UpdateItemAsync<T>(T item, string id, string partitionKey) where T : class;
    Task DeleteItemAsync(string id, string partitionKey);
    Task<IEnumerable<T>> QueryItemsAsync<T>(string query) where T : class;
} 