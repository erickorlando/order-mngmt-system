namespace Vendors.Application.DTOs;

public record CreateVendorRequest
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string ContactPerson { get; init; } = string.Empty;
} 