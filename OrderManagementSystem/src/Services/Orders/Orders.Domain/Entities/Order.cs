using Common.Models;
using Orders.Domain.ValueObjects;
using Vendors.Domain.Entities;

namespace Orders.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; private set; }
    public Guid CustomerId { get; private set; }
    public Customer Customer { get; set; }
    public Guid VendorId { get; private set; }
    public Vendor Vendor { get; set; }
    public OrderStatus Status { get; private set; }
    public Money TotalAmount { get; private set; }
    public DateTime OrderDate { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<OrderItem> _orderItems = new();
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    // Constructor privado para EF Core
    private Order() : base() { }

    public Order(Guid customerId, Guid vendorId, string? notes = null) : base()
    {
        CustomerId = customerId;
        VendorId = vendorId;
        Status = OrderStatus.Pending;
        OrderDate = DateTime.UtcNow;
        Notes = notes;
        OrderNumber = GenerateOrderNumber();
        TotalAmount = Money.Zero;
    }

    public void AddOrderItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot modify confirmed orders");

        var orderItem = new OrderItem(Id, productId, productName, quantity, unitPrice);
        _orderItems.Add(orderItem);
        
        RecalculateTotal();
    }

    public void RemoveOrderItem(Guid orderItemId)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot modify confirmed orders");

        var item = _orderItems.FirstOrDefault(x => x.Id == orderItemId);
        if (item != null)
        {
            _orderItems.Remove(item);
            RecalculateTotal();
        }
    }

    public void ConfirmOrder()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Order is already confirmed or cancelled");

        if (!_orderItems.Any())
            throw new InvalidOperationException("Cannot confirm order without items");

        Status = OrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CancelOrder()
    {
        if (Status == OrderStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed orders");

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    private void RecalculateTotal()
    {
        var total = _orderItems.Sum(item => item.TotalPrice.Amount);
        TotalAmount = new Money(total);
    }

    private string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }
}