namespace Common.Interfaces;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    
    // Métodos adicionales específicos de Redis
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    Task<TimeSpan?> GetTimeToLiveAsync(string key, CancellationToken cancellationToken = default);
    
    // Métodos para operaciones de Hash
    Task SetHashAsync<T>(string key, string field, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;
    Task<T?> GetHashAsync<T>(string key, string field, CancellationToken cancellationToken = default) where T : class;
    Task RemoveHashAsync(string key, string field, CancellationToken cancellationToken = default);
    
    // Métodos de administración
    Task FlushDatabaseAsync(CancellationToken cancellationToken = default);
    Task<long> GetDatabaseSizeAsync(CancellationToken cancellationToken = default);
} 