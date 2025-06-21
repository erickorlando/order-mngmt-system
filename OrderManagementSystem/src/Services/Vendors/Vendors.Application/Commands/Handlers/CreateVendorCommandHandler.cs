using MediatR;
using Vendors.Application.DTOs;
using Vendors.Application.Interfaces;

namespace Vendors.Application.Commands.Handlers;

public class CreateVendorCommandHandler : IRequestHandler<CreateVendorCommand, VendorDto>
{
    private readonly IVendorApplicationService _vendorApplicationService;

    public CreateVendorCommandHandler(IVendorApplicationService vendorApplicationService)
    {
        _vendorApplicationService = vendorApplicationService;
    }

    public async Task<VendorDto> Handle(CreateVendorCommand request, CancellationToken cancellationToken)
    {
        var createVendorRequest = new CreateVendorRequest
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            ContactPerson = request.ContactPerson
        };

        return await _vendorApplicationService.CreateVendorAsync(createVendorRequest);
    }
} 