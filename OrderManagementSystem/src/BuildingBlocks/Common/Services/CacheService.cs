using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Common.Interfaces;

namespace Common.Services;

public class CacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly ILogger<CacheService> _logger;
    private readonly string _instanceName;

    public CacheService(IConnectionMultiplexer redis, IConfiguration configuration, ILogger<CacheService> logger)
    {
        _redis = redis;
        _database = redis.GetDatabase();
        _logger = logger;
        _instanceName = configuration["Redis:InstanceName"] ?? "DefaultAPI";
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var fullKey = GetFullKey(key);
            var cachedValue = await _database.StringGetAsync(fullKey);
            
            if (!cachedValue.HasValue)
            {
                _logger.LogDebug("Cache miss for key: {Key}", fullKey);
                return null;
            }

            _logger.LogDebug("Cache hit for key: {Key}", fullKey);
            return JsonSerializer.Deserialize<T>(cachedValue!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from Redis for key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var fullKey = GetFullKey(key);
            var serializedValue = JsonSerializer.Serialize(value);
            var expiry = expiration ?? TimeSpan.FromMinutes(30);

            await _database.StringSetAsync(fullKey, serializedValue, expiry);
            _logger.LogDebug("Value cached successfully for key: {Key} with expiry: {Expiry}", fullKey, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in Redis for key: {Key}", key);
            throw;
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullKey = GetFullKey(key);
            var result = await _database.KeyDeleteAsync(fullKey);
            
            if (result)
            {
                _logger.LogDebug("Key removed successfully: {Key}", fullKey);
            }
            else
            {
                _logger.LogDebug("Key not found for removal: {Key}", fullKey);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing key from Redis: {Key}", key);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullKey = GetFullKey(key);
            return await _database.KeyExistsAsync(fullKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if key exists in Redis: {Key}", key);
            return false;
        }
    }

    public async Task<TimeSpan?> GetTimeToLiveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullKey = GetFullKey(key);
            var ttl = await _database.KeyTimeToLiveAsync(fullKey);
            return ttl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting TTL for key: {Key}", key);
            return null;
        }
    }

    public async Task SetHashAsync<T>(string key, string field, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var fullKey = GetFullKey(key);
            var serializedValue = JsonSerializer.Serialize(value);
            var expiry = expiration ?? TimeSpan.FromMinutes(30);

            await _database.HashSetAsync(fullKey, field, serializedValue);
            await _database.KeyExpireAsync(fullKey, expiry);
            
            _logger.LogDebug("Hash field set successfully for key: {Key}, field: {Field}", fullKey, field);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting hash field in Redis for key: {Key}, field: {Field}", key, field);
            throw;
        }
    }

    public async Task<T?> GetHashAsync<T>(string key, string field, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var fullKey = GetFullKey(key);
            var cachedValue = await _database.HashGetAsync(fullKey, field);
            
            if (!cachedValue.HasValue)
            {
                _logger.LogDebug("Hash field cache miss for key: {Key}, field: {Field}", fullKey, field);
                return null;
            }

            _logger.LogDebug("Hash field cache hit for key: {Key}, field: {Field}", fullKey, field);
            return JsonSerializer.Deserialize<T>(cachedValue!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting hash field from Redis for key: {Key}, field: {Field}", key, field);
            return null;
        }
    }

    public async Task RemoveHashAsync(string key, string field, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullKey = GetFullKey(key);
            var result = await _database.HashDeleteAsync(fullKey, field);
            
            if (result)
            {
                _logger.LogDebug("Hash field removed successfully: {Key}, field: {Field}", fullKey, field);
            }
            else
            {
                _logger.LogDebug("Hash field not found for removal: {Key}, field: {Field}", fullKey, field);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing hash field from Redis: {Key}, field: {Field}", key, field);
            throw;
        }
    }

    public async Task FlushDatabaseAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _database.ExecuteAsync("FLUSHDB");
            _logger.LogInformation("Redis database flushed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error flushing Redis database");
            throw;
        }
    }

    public async Task<long> GetDatabaseSizeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var info = await _database.ExecuteAsync("DBSIZE");
            return (long)info;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Redis database size");
            return 0;
        }
    }

    private string GetFullKey(string key)
    {
        return $"{_instanceName}:{key}";
    }

    public void Dispose()
    {
        // Redis connection is managed by DI container, no need to dispose here
    }
} 