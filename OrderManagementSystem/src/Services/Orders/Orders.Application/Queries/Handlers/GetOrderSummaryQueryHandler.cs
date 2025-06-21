using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Orders.Application.Dtos;
using Orders.Domain.Repositories;

namespace Orders.Application.Queries.Handlers;

public class GetOrderSummaryQueryHandler : IRequestHandler<GetOrderSummaryQuery, IEnumerable<OrderSummaryDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IDistributedCache _cache;

    public GetOrderSummaryQueryHandler(IOrderRepository orderRepository, IDistributedCache cache)
    {
        _orderRepository = orderRepository;
        _cache = cache;
    }

    public async Task<IEnumerable<OrderSummaryDto>> Handle(GetOrderSummaryQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = GenerateCacheKey(request);
        var cachedResult = await _cache.GetStringAsync(cacheKey, cancellationToken);

        if (!string.IsNullOrEmpty(cachedResult))
        {
            return JsonSerializer.Deserialize<IEnumerable<OrderSummaryDto>>(cachedResult) ?? Enumerable.Empty<OrderSummaryDto>();
        }

        var orders = await _orderRepository.GetOrderSummariesAsync(
            request.Page,
            request.PageSize,
            request.CustomerId,
            request.VendorId,
            request.Status,
            request.FromDate,
            request.ToDate
        );

        var result = orders.Select(order => new OrderSummaryDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            CustomerName = order.Customer?.Name ?? "Unknown",
            VendorId = order.VendorId,
            VendorName = order.Vendor?.Name ?? "Unknown",
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount.Amount,
            OrderDate = order.OrderDate,
            ItemCount = order.OrderItems.Count
        });   
        
// Cache por 5 minutos
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), cacheOptions, cancellationToken);

        return result;
    }

    private static string GenerateCacheKey(GetOrderSummaryQuery request)
    {
        return $"orders_summary_{request.Page}_{request.PageSize}_{request.CustomerId}_{request.VendorId}_{request.Status}_{request.FromDate}_{request.ToDate}";
    }
}