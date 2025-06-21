using MediatR;
using Microsoft.AspNetCore.Mvc;
using Orders.Application.Commands;
using Orders.Application.Dtos;
using Orders.Application.Queries;

namespace Orders.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene un resumen de órdenes con filtros opcionales
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetOrdersSummary(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? vendorId = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var query = new GetOrderSummaryQuery
            {
                Page = page,
                PageSize = Math.Min(pageSize, 100), // Limitar a 100 registros por página
                CustomerId = customerId,
                VendorId = vendorId,
                Status = status,
                FromDate = fromDate,
                ToDate = toDate
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders summary");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Obtiene los detalles completos de una orden específica
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDetailDto>> GetOrderDetail(Guid id)
    {
        try
        {
            var query = new GetOrderDetailQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound($"Order with ID {id} not found");

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order detail for ID: {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Crea una nueva orden
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<OrderDetailDto>> CreateOrder([FromBody] CreateOrderCommand command)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetOrderDetail), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument in create order request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Obtiene órdenes por cliente
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetOrdersByCustomer(
        Guid customerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = new GetOrderSummaryQuery
            {
                Page = page,
                PageSize = Math.Min(pageSize, 100),
                CustomerId = customerId
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for customer: {CustomerId}", customerId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Obtiene órdenes por vendedor
    /// </summary>
    [HttpGet("vendor/{vendorId:guid}")]
    public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetOrdersByVendor(
        Guid vendorId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = new GetOrderSummaryQuery
            {
                Page = page,
                PageSize = Math.Min(pageSize, 100),
                VendorId = vendorId
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for vendor: {VendorId}", vendorId);
            return StatusCode(500, "Internal server error");
        }
    }
}