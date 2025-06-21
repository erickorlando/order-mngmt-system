using Vendors.Application.DTOs;

namespace Vendors.Application.Interfaces;

public interface IVendorApplicationService
{
    Task<IEnumerable<VendorDto>> GetAllVendorsAsync();
    Task<VendorDto?> GetVendorByIdAsync(Guid id);
    Task<VendorDto> CreateVendorAsync(CreateVendorRequest request);
} 