using EventBus.Interfaces;
using EventBus.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vendors.Application.Interfaces;
using Vendors.Domain;
using Vendors.Infrastructure.Repositories;
using Vendors.Infrastructure.Services;

namespace Vendors.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Repositories
        services.AddScoped<IVendorRepository, VendorRepository>();

        // Services
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<ICosmosDbService, CosmosDbService>();

        // Event Bus
        services.AddSingleton<IEventBus, RabbitMqEventBus>(sp =>
        {
            var rabbitMqConfig = configuration.GetSection("RabbitMQ");
            return new RabbitMqEventBus(
                rabbitMqConfig["HostName"]!,
                rabbitMqConfig["UserName"]!,
                rabbitMqConfig["Password"]!,
                rabbitMqConfig["ExchangeName"]!,
                rabbitMqConfig["QueueName"]!
            );
        });

        return services;
    }
} 