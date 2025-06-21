using AutoMapper;
using Vendors.Application.DTOs;
using Vendors.Application.Interfaces;
using Vendors.Domain;
using Vendors.Domain.Entities;

namespace Vendors.Application.Services;

public class VendorApplicationService : IVendorApplicationService
{
    private readonly IVendorRepository _vendorRepository;
    private readonly IMapper _mapper;

    public VendorApplicationService(IVendorRepository vendorRepository, IMapper mapper)
    {
        _vendorRepository = vendorRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<VendorDto>> GetAllVendorsAsync()
    {
        var vendors = await _vendorRepository.GetAllActiveAsync();
        return _mapper.Map<IEnumerable<VendorDto>>(vendors);
    }

    public async Task<VendorDto?> GetVendorByIdAsync(Guid id)
    {
        var vendor = await _vendorRepository.GetByIdAsync(id);
        return vendor != null ? _mapper.Map<VendorDto>(vendor) : null;
    }

    public async Task<VendorDto> CreateVendorAsync(CreateVendorRequest request)
    {
        var vendor = new Vendor(
            request.Name,
            request.Email,
            request.Phone,
            request.Address,
            request.ContactPerson
        );

        await _vendorRepository.AddAsync(vendor);
        await _vendorRepository.SaveChangesAsync();

        return _mapper.Map<VendorDto>(vendor);
    }
} 