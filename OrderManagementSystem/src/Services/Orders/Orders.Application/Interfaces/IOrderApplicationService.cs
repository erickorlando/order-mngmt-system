using Orders.Application.Dtos;
using Orders.Domain.ValueObjects;

namespace Orders.Application.Interfaces;

public interface IOrderApplicationService
{
    Task<OrderDetailDto> CreateOrderAsync(CreateOrderRequest request);
    Task<OrderDetailDto?> GetOrderByIdAsync(Guid orderId);
    Task<IEnumerable<OrderSummaryDto>> GetOrdersAsync(GetOrdersRequest request);
    Task<OrderDetailDto> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus);
    Task<bool> CancelOrderAsync(Guid orderId, string reason);
    
    // Order Items Management
    Task<OrderDetailDto> AddOrderItemAsync(Guid orderId, AddOrderItemRequest request);
    Task<OrderDetailDto> RemoveOrderItemAsync(Guid orderId, Guid orderItemId);
    Task<OrderDetailDto> UpdateOrderItemQuantityAsync(Guid orderId, Guid orderItemId, int newQuantity);
    
    // Business Logic
    Task<decimal> CalculateOrderTotalAsync(Guid orderId);
    Task<bool> CanModifyOrderAsync(Guid orderId);
    Task<IEnumerable<OrderSummaryDto>> GetOrdersByCustomerAsync(Guid customerId, int page = 1, int pageSize = 10);
    Task<IEnumerable<OrderSummaryDto>> GetOrdersByVendorAsync(Guid vendorId, int page = 1, int pageSize = 10);
    
    // Analytics and Reporting
    Task<OrderStatisticsDto> GetOrderStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<IEnumerable<TopCustomerDto>> GetTopCustomersAsync(int count = 10);
    Task<IEnumerable<TopVendorDto>> GetTopVendorsAsync(int count = 10);
}