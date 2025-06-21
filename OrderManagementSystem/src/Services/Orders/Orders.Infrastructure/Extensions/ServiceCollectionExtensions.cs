using EventBus.Interfaces;
using EventBus.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.Application.Interfaces;
using Orders.Domain.Repositories;
using Orders.Infrastructure.Repositories;
using Orders.Infrastructure.Services;

namespace Orders.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Repositories
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();

        // Services
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<ICosmosDbService, CosmosDbService>();
        services.AddScoped<IVendorService, VendorService>();

        // HTTP Client for Vendors API
        services.AddHttpClient<IVendorService, VendorService>(client =>
        {
            client.BaseAddress = new Uri(configuration["VendorApi:BaseUrl"]!);
        });

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
