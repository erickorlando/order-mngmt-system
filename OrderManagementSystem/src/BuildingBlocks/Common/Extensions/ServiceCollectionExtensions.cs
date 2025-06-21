using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Common.Interfaces;
using Common.Services;

namespace Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var options = ConfigurationOptions.Parse(redisConnectionString);
                options.ConnectRetry = 3;
                options.ReconnectRetryPolicy = new ExponentialRetry(5000);
                return ConnectionMultiplexer.Connect(options);
            });

            services.AddScoped<ICacheService, CacheService>();
        }

        return services;
    }
} 