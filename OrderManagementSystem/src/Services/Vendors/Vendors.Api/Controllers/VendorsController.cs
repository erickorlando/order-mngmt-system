using MediatR;
using Microsoft.AspNetCore.Mvc;
using Vendors.Application.Commands;
using Vendors.Application.DTOs;
using Vendors.Application.Queries;

namespace Vendors.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VendorsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<VendorsController> _logger;

    public VendorsController(IMediator mediator, ILogger<VendorsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VendorDto>>> GetVendors()
    {
        try
        {
            var query = new GetAllVendorsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vendors");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VendorDto>> GetVendor(Guid id)
    {
        try
        {
            var query = new GetVendorByIdQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound($"Vendor with ID {id} not found");

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vendor with ID: {VendorId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<VendorDto>> CreateVendor([FromBody] CreateVendorCommand command)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetVendor), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vendor");
            return StatusCode(500, "Internal server error");
        }
    }
}