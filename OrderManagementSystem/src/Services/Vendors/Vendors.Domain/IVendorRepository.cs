using Vendors.Domain.Entities;

namespace Vendors.Domain;

public interface IVendorRepository
{
    Task<Vendor?> GetByIdAsync(Guid id);
    Task<IEnumerable<Vendor>> GetAllActiveAsync();
    Task AddAsync(Vendor vendor);
    Task UpdateAsync(Vendor vendor);
    Task DeleteAsync(Guid id);
    Task<int> SaveChangesAsync();
}