using Microsoft.EntityFrameworkCore;
using Orders.Domain.Entities;
using Orders.Domain.Repositories;
using Orders.Infrastructure.Data;

namespace Orders.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    
    private readonly OrdersDbContext _context;

    public CustomerRepository(OrdersDbContext context)
    {
        _context = context;
    }
    
    public async Task<Customer?> GetByIdAsync(Guid id)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        return await _context.Customers
            .OrderBy(c => c.Name)
            .ToListAsync();
    }


    public async Task AddAsync(Customer customer)
    {
        if (customer == null)
            throw new ArgumentNullException(nameof(customer));

        await _context.Customers.AddAsync(customer);
    }

    public Task UpdateAsync(Customer customer)
    {
        if (customer == null)
            throw new ArgumentNullException(nameof(customer));

        _context.Customers.Update(customer);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer != null)
        {
            // Soft delete: En lugar de eliminar, podrías marcar como inactivo
            // customer.Deactivate(); // Si implementas IsActive en Customer
            
            // O hard delete si es apropiado para tu negocio
            _context.Customers.Remove(customer);
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

}