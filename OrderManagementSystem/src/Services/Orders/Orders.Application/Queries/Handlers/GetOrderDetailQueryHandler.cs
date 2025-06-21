using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Orders.Application.Dtos;
using Orders.Domain.Repositories;

namespace Orders.Application.Queries.Handlers;

public class GetOrderDetailQueryHandler : IRequestHandler<GetOrderDetailQuery, OrderDetailDto?>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IDistributedCache _cache;

    public GetOrderDetailQueryHandler(IOrderRepository orderRepository, IDistributedCache cache)
    {
        _orderRepository = orderRepository;
        _cache = cache;
    }

    public async Task<OrderDetailDto?> Handle(GetOrderDetailQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"order_detail_{request.OrderId}";
        var cachedResult = await _cache.GetStringAsync(cacheKey, cancellationToken);

        if (!string.IsNullOrEmpty(cachedResult))
        {
            return JsonSerializer.Deserialize<OrderDetailDto>(cachedResult);
        }

        var order = await _orderRepository.GetByIdWithDetailsAsync(request.OrderId);
        if (order == null)
            return null;

        var result = new OrderDetailDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            // CustomerName = order.Customer?.Name ?? "Unknown",
            // CustomerEmail = order.Customer?.Email ?? "Unknown",
            VendorId = order.VendorId,
            // VendorName = order.Vendor?.Name ?? "Unknown",
            // VendorEmail = order.Vendor?.Email ?? "Unknown",
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount.Amount,
            OrderDate = order.OrderDate,
            Notes = order.Notes,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Items = order.OrderItems.Select(item => new OrderItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice.Amount,
                TotalPrice = item.TotalPrice.Amount
            }).ToList()
        };

        // Cache por 10 minutos
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        };

        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), cacheOptions, cancellationToken);

        return result;
    }
}