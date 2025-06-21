using EventBus.Interfaces;
using MediatR;
using Orders.Application.Dtos;
using Orders.Application.Events;
using Orders.Application.Interfaces;
using Orders.Domain.Entities;
using Orders.Domain.Repositories;

namespace Orders.Application.Commands;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDetailDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IVendorService _vendorService;
    private readonly IEventBus _eventBus;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IVendorService vendorService,
        IEventBus eventBus)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _vendorService = vendorService;
        _eventBus = eventBus;
    }

    public async Task<OrderDetailDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Validar existencia de cliente
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
        if (customer == null)
            throw new ArgumentException("Customer not found");

        // Validar existencia de vendedor usando el servicio HTTP
        var vendor = await _vendorService.GetVendorByIdAsync(request.VendorId);
        if (vendor == null)
            throw new ArgumentException("Vendor not found");

        if (!vendor.IsActive)
            throw new ArgumentException("Vendor is not active");

        // Crear la orden
        var order = new Order(request.CustomerId, request.VendorId, request.Notes);

        // Agregar items
        foreach (var item in request.Items)
        {
            order.AddOrderItem(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice);
        }

        // Guardar en base de datos
        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();

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
        return new OrderDetailDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            CustomerName = customer.Name,
            CustomerEmail = customer.Email,
            VendorId = order.VendorId,
            VendorName = vendor.Name,
            VendorEmail = vendor.Email,
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