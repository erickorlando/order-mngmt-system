namespace Orders.Application.Interfaces;

public interface IVendorService
{
    Task<VendorInfo?> GetVendorByIdAsync(Guid vendorId);
}

public record VendorInfo
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool IsActive { get; init; }
} 