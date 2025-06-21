using EventBus.Interfaces;
using Microsoft.Extensions.Logging;
using Orders.Application.Dtos;
using Orders.Application.Events;
using Orders.Application.Interfaces;
using Orders.Domain.Entities;
using Orders.Domain.Repositories;
using Orders.Domain.ValueObjects;
using Vendors.Domain;
using Vendors.Domain.Entities;

namespace Orders.Application.Services;

public class OrderApplicationService : IOrderApplicationService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly ICacheService _cacheService;
    private readonly IEventBus _eventBus;
    private readonly ILogger<OrderApplicationService> _logger;

    public OrderApplicationService(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IVendorRepository vendorRepository,
        ICacheService cacheService,
        IEventBus eventBus,
        ILogger<OrderApplicationService> logger)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _vendorRepository = vendorRepository;
        _cacheService = cacheService;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<OrderDetailDto> CreateOrderAsync(CreateOrderRequest request)
    {
        _logger.LogInformation("Creating order for customer {CustomerId} and vendor {VendorId}", 
            request.CustomerId, request.VendorId);

        // Validar existencia de cliente y vendedor
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
        if (customer == null)
            throw new ArgumentException($"Customer with ID {request.CustomerId} not found");

        var vendor = await _vendorRepository.GetByIdAsync(request.VendorId);
        if (vendor == null)
            throw new ArgumentException($"Vendor with ID {request.VendorId} not found");

        // Validar que el vendedor esté activo
        if (!vendor.IsActive)
            throw new InvalidOperationException($"Vendor {vendor.Name} is not active");

        // Crear la orden
        var order = new Order(request.CustomerId, request.VendorId, request.Notes);

        // Agregar items
        foreach (var itemRequest in request.Items)
        {
            if (itemRequest.Quantity <= 0)
                throw new ArgumentException($"Invalid quantity {itemRequest.Quantity} for product {itemRequest.ProductName}");
            
            if (itemRequest.UnitPrice < 0)
                throw new ArgumentException($"Invalid unit price {itemRequest.UnitPrice} for product {itemRequest.ProductName}");

            order.AddOrderItem(itemRequest.ProductId, itemRequest.ProductName, itemRequest.Quantity, itemRequest.UnitPrice);
        }

        if (!order.OrderItems.Any())
            throw new InvalidOperationException("Cannot create order without items");

        // Guardar en base de datos
        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();

        _logger.LogInformation("Order {OrderId} created successfully with {ItemCount} items", 
            order.Id, order.OrderItems.Count);

        // Invalidar cache relacionado
        await InvalidateOrderCacheAsync(order.CustomerId, order.VendorId);

        // Publicar evento
        var orderCreatedEvent = new OrderCreatedIntegrationEvent(
            order.Id,
            order.OrderNumber,
            order.CustomerId,
            order.VendorId,
            order.TotalAmount.Amount,
            order.OrderItems.Count
        );

        await _eventBus.PublishAsync(orderCreatedEvent);

        // Mapear a DTO
        return MapToOrderDetailDto(order, customer, vendor);
    }

    public async Task<OrderDetailDto?> GetOrderByIdAsync(Guid orderId)
    {
        var cacheKey = $"order_detail_{orderId}";
        var cachedOrder = await _cacheService.GetAsync<OrderDetailDto>(cacheKey);
        
        if (cachedOrder != null)
        {
            _logger.LogDebug("Order {OrderId} retrieved from cache", orderId);
            return cachedOrder;
        }

        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        if (order == null)
            return null;

        var customer = await _customerRepository.GetByIdAsync(order.CustomerId);
        var vendor = await _vendorRepository.GetByIdAsync(order.VendorId);

        var orderDto = MapToOrderDetailDto(order, customer, vendor);

        // Cache por 10 minutos
        await _cacheService.SetAsync(cacheKey, orderDto, TimeSpan.FromMinutes(10));

        return orderDto;
    }

    public async Task<IEnumerable<OrderSummaryDto>> GetOrdersAsync(GetOrdersRequest request)
    {
        var orders = await _orderRepository.GetOrderSummariesAsync(
            request.Page,
            request.PageSize,
            request.CustomerId,
            request.VendorId,
            request.Status,
            request.FromDate,
            request.ToDate
        );

        return orders.Select(order => new OrderSummaryDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            CustomerName = "Customer", // Se podría mejorar con un JOIN o cache
            VendorId = order.VendorId,
            VendorName = "Vendor", // Se podría mejorar con un JOIN o cache
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount.Amount,
            OrderDate = order.OrderDate,
            ItemCount = order.OrderItems.Count
        });
    }

    public async Task<OrderDetailDto> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        if (order == null)
            throw new ArgumentException($"Order with ID {orderId} not found");

        _logger.LogInformation("Updating order {OrderId} status from {OldStatus} to {NewStatus}", 
            orderId, order.Status, newStatus);

        switch (newStatus)
        {
            case OrderStatus.Confirmed:
                order.ConfirmOrder();
                break;
            case OrderStatus.Cancelled:
                order.CancelOrder();
                break;
            default:
                throw new InvalidOperationException($"Cannot manually set order status to {newStatus}");
        }

        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();

        // Invalidar cache
        await _cacheService.RemoveAsync($"order_detail_{orderId}");
        await InvalidateOrderCacheAsync(order.CustomerId, order.VendorId);

        var customer = await _customerRepository.GetByIdAsync(order.CustomerId);
        var vendor = await _vendorRepository.GetByIdAsync(order.VendorId);

        return MapToOrderDetailDto(order, customer, vendor);
    }

    public async Task<bool> CancelOrderAsync(Guid orderId, string reason)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        if (order == null)
            return false;

        if (order.Status == OrderStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed orders");

        _logger.LogInformation("Cancelling order {OrderId} with reason: {Reason}", orderId, reason);

        order.CancelOrder();
        
        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();

        // Invalidar cache
        await _cacheService.RemoveAsync($"order_detail_{orderId}");

        return true;
    }

    public async Task<OrderDetailDto> AddOrderItemAsync(Guid orderId, AddOrderItemRequest request)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        if (order == null)
            throw new ArgumentException($"Order with ID {orderId} not found");

        if (!await CanModifyOrderAsync(orderId))
            throw new InvalidOperationException("Cannot modify this order");

        order.AddOrderItem(request.ProductId, request.ProductName, request.Quantity, request.UnitPrice);

        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();

        // Invalidar cache
        await _cacheService.RemoveAsync($"order_detail_{orderId}");

        var customer = await _customerRepository.GetByIdAsync(order.CustomerId);
        var vendor = await _vendorRepository.GetByIdAsync(order.VendorId);

        return MapToOrderDetailDto(order, customer, vendor);
    }

    public async Task<OrderDetailDto> RemoveOrderItemAsync(Guid orderId, Guid orderItemId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        if (order == null)
            throw new ArgumentException($"Order with ID {orderId} not found");

        if (!await CanModifyOrderAsync(orderId))
            throw new InvalidOperationException("Cannot modify this order");

        order.RemoveOrderItem(orderItemId);

        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();

        // Invalidar cache
        await _cacheService.RemoveAsync($"order_detail_{orderId}");

        var customer = await _customerRepository.GetByIdAsync(order.CustomerId);
        var vendor = await _vendorRepository.GetByIdAsync(order.VendorId);

        return MapToOrderDetailDto(order, customer, vendor);
    }

    public async Task<OrderDetailDto> UpdateOrderItemQuantityAsync(Guid orderId, Guid orderItemId, int newQuantity)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        if (order == null)
            throw new ArgumentException($"Order with ID {orderId} not found");

        if (!await CanModifyOrderAsync(orderId))
            throw new InvalidOperationException("Cannot modify this order");

        var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == orderItemId);
        if (orderItem == null)
            throw new ArgumentException($"Order item with ID {orderItemId} not found");

        orderItem.UpdateQuantity(newQuantity);

        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();

        // Invalidar cache
        await _cacheService.RemoveAsync($"order_detail_{orderId}");

        var customer = await _customerRepository.GetByIdAsync(order.CustomerId);
        var vendor = await _vendorRepository.GetByIdAsync(order.VendorId);

        return MapToOrderDetailDto(order, customer, vendor);
    }

    public async Task<decimal> CalculateOrderTotalAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new ArgumentException($"Order with ID {orderId} not found");

        return order.TotalAmount.Amount;
    }

    public async Task<bool> CanModifyOrderAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        return order?.Status == OrderStatus.Pending;
    }

    public async Task<IEnumerable<OrderSummaryDto>> GetOrdersByCustomerAsync(Guid customerId, int page = 1, int pageSize = 10)
    {
        var orders = await _orderRepository.GetOrderSummariesAsync(
            page, pageSize, customerId: customerId);

        return orders.Select(order => new OrderSummaryDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            CustomerName = "Customer",
            VendorId = order.VendorId,
            VendorName = "Vendor",
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount.Amount,
            OrderDate = order.OrderDate,
            ItemCount = order.OrderItems.Count
        });
    }

    public async Task<IEnumerable<OrderSummaryDto>> GetOrdersByVendorAsync(Guid vendorId, int page = 1, int pageSize = 10)
    {
        var orders = await _orderRepository.GetOrderSummariesAsync(
            page, pageSize, vendorId: vendorId);

        return orders.Select(order => new OrderSummaryDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            CustomerName = "Customer",
            VendorId = order.VendorId,
            VendorName = "Vendor",
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount.Amount,
            OrderDate = order.OrderDate,
            ItemCount = order.OrderItems.Count
        });
    }

    public async Task<OrderStatisticsDto> GetOrderStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        // Esta implementación sería más compleja en un escenario real
        // Se podría implementar con queries específicas o usando un servicio de analytics
        await Task.CompletedTask;
        
        return new OrderStatisticsDto
        {
            TotalOrders = 0,
            PendingOrders = 0,
            ConfirmedOrders = 0,
            CompletedOrders = 0,
            CancelledOrders = 0,
            TotalRevenue = 0,
            AverageOrderValue = 0,
            TotalCustomers = 0,
            TotalVendors = 0,
            PeriodFrom = fromDate ?? DateTime.UtcNow.AddMonths(-1),
            PeriodTo = toDate ?? DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<TopCustomerDto>> GetTopCustomersAsync(int count = 10)
    {
        // Implementación simplificada - en un escenario real usarías queries específicas
        await Task.CompletedTask;
        return new List<TopCustomerDto>();
    }

    public async Task<IEnumerable<TopVendorDto>> GetTopVendorsAsync(int count = 10)
    {
        // Implementación simplificada - en un escenario real usarías queries específicas
        await Task.CompletedTask;
        return new List<TopVendorDto>();
    }

    // Métodos privados de utilidad
    private async Task InvalidateOrderCacheAsync(Guid customerId, Guid vendorId)
    {
        await _cacheService.RemoveAsync($"orders_summary_customer_{customerId}");
        await _cacheService.RemoveAsync($"orders_summary_vendor_{vendorId}");
        await _cacheService.RemoveAsync("orders_summary_all");
    }

    private static OrderDetailDto MapToOrderDetailDto(Order order, Customer? customer, Vendor? vendor)
    {
        return new OrderDetailDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            CustomerName = customer?.Name ?? "Unknown Customer",
            CustomerEmail = customer?.Email ?? "Unknown",
            VendorId = order.VendorId,
            VendorName = vendor?.Name ?? "Unknown Vendor",
            VendorEmail = vendor?.Email ?? "Unknown",
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
    }
}