using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Vendors.Application.Interfaces;
using Vendors.Application.Services;

namespace Vendors.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Application Services
        services.AddScoped<IVendorApplicationService, VendorApplicationService>();

        return services;
    }
} 