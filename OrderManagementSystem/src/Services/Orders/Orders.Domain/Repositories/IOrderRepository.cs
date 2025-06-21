using Orders.Domain.Entities;

namespace Orders.Domain.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<Order?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<Order>> GetOrderSummariesAsync(
        int page, 
        int pageSize, 
        Guid? customerId = null, 
        Guid? vendorId = null, 
        string? status = null, 
        DateTime? fromDate = null, 
        DateTime? toDate = null);
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task DeleteAsync(Guid id);
    Task<int> SaveChangesAsync();
}