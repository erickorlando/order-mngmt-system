using MediatR;
using Orders.Application.Dtos;

namespace Orders.Application.Queries;

public record GetOrderSummaryQuery : IRequest<IEnumerable<OrderSummaryDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public Guid? CustomerId { get; init; }
    public Guid? VendorId { get; init; }
    public string? Status { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}