using Common.Models;
using Orders.Domain.ValueObjects;

namespace Orders.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    public Money TotalPrice { get; private set; }

    // Constructor privado para EF Core
    private OrderItem() : base() { }

    public OrderItem(Guid orderId, Guid productId, string productName, int quantity, decimal unitPrice) : base()
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero");
        
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative");

        OrderId = orderId;
        ProductId = productId;
        ProductName = productName ?? throw new ArgumentNullException(nameof(productName));
        Quantity = quantity;
        UnitPrice = new Money(unitPrice);
        TotalPrice = new Money(unitPrice * quantity);
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero");

        Quantity = newQuantity;
        TotalPrice = new Money(UnitPrice.Amount * newQuantity);
        UpdatedAt = DateTime.UtcNow;
    }
}