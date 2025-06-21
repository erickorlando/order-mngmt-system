using MediatR;
using Orders.Application.Dtos;

namespace Orders.Application.Commands;

public record CreateOrderCommand : IRequest<OrderDetailDto>
{
    public Guid CustomerId { get; init; }
    public Guid VendorId { get; init; }
    public string? Notes { get; init; }
    public List<CreateOrderItemCommand> Items { get; init; } = new();
}

public record CreateOrderItemCommand
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}
