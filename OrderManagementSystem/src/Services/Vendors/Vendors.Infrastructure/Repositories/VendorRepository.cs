using Microsoft.EntityFrameworkCore;
using Vendors.Domain;
using Vendors.Domain.Entities;
using Vendors.Infrastructure.Data;

namespace Vendors.Infrastructure.Repositories;

public class VendorRepository : IVendorRepository
{
    private readonly VendorsDbContext _context;

    public VendorRepository(VendorsDbContext context)
    {
        _context = context;
    }

    public async Task<Vendor?> GetByIdAsync(Guid id)
    {
        return await _context.Vendors
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<IEnumerable<Vendor>> GetAllActiveAsync()
    {
        return await _context.Vendors
            .Where(v => v.IsActive)
            .OrderBy(v => v.Name)
            .ToListAsync();
    }

    public async Task AddAsync(Vendor vendor)
    {
        await _context.Vendors.AddAsync(vendor);
    }

    public Task UpdateAsync(Vendor vendor)
    {
        _context.Vendors.Update(vendor);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor != null)
        {
            _context.Vendors.Remove(vendor);
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
} 