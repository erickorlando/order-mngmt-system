using Common.Models;

namespace Orders.Domain.Entities;

public class Customer : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public string Address { get; private set; } = null!;

    private Customer() : base() { }

    public Customer(string name, string email, string phone, string address) : base()
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Phone = phone ?? throw new ArgumentNullException(nameof(phone));
        Address = address ?? throw new ArgumentNullException(nameof(address));
    }

    public void UpdateContactInfo(string phone, string address)
    {
        Phone = phone ?? throw new ArgumentNullException(nameof(phone));
        Address = address ?? throw new ArgumentNullException(nameof(address));
        UpdatedAt = DateTime.UtcNow;
    }
}
