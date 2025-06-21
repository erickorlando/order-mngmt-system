using MediatR;
using Orders.Application.Dtos;

namespace Orders.Application.Queries;

public record GetOrderDetailQuery(Guid OrderId) : IRequest<OrderDetailDto?>;