using MediatR;
using Vendors.Application.DTOs;
using Vendors.Application.Interfaces;

namespace Vendors.Application.Queries.Handlers;

public class GetVendorByIdQueryHandler : IRequestHandler<GetVendorByIdQuery, VendorDto?>
{
    private readonly IVendorApplicationService _vendorApplicationService;

    public GetVendorByIdQueryHandler(IVendorApplicationService vendorApplicationService)
    {
        _vendorApplicationService = vendorApplicationService;
    }

    public async Task<VendorDto?> Handle(GetVendorByIdQuery request, CancellationToken cancellationToken)
    {
        return await _vendorApplicationService.GetVendorByIdAsync(request.Id);
    }
} 