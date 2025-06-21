using MediatR;
using Vendors.Application.DTOs;
using Vendors.Application.Interfaces;

namespace Vendors.Application.Queries.Handlers;

public class GetAllVendorsQueryHandler : IRequestHandler<GetAllVendorsQuery, IEnumerable<VendorDto>>
{
    private readonly IVendorApplicationService _vendorApplicationService;

    public GetAllVendorsQueryHandler(IVendorApplicationService vendorApplicationService)
    {
        _vendorApplicationService = vendorApplicationService;
    }

    public async Task<IEnumerable<VendorDto>> Handle(GetAllVendorsQuery request, CancellationToken cancellationToken)
    {
        return await _vendorApplicationService.GetAllVendorsAsync();
    }
} 