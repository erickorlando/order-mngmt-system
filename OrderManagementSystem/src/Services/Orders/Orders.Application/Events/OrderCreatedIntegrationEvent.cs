using EventBus.Events;

namespace Orders.Application.Events;

public record OrderCreatedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public string OrderNumber { get; init; }
    public Guid CustomerId { get; init; }
    public Guid VendorId { get; init; }
    public decimal TotalAmount { get; init; }
    public int ItemCount { get; init; }

    public OrderCreatedIntegrationEvent(
        Guid orderId,
        string orderNumber,
        Guid customerId,
        Guid vendorId,
        decimal totalAmount,
        int itemCount)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        CustomerId = customerId;
        VendorId = vendorId;
        TotalAmount = totalAmount;
        ItemCount = itemCount;
    }
}