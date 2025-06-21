using Common.Models;

namespace Vendors.Domain.Entities;

public class Vendor : BaseEntity
{
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string Phone { get; private set; }
    public string Address { get; private set; }
    public string ContactPerson { get; private set; }
    public bool IsActive { get; private set; }

    private Vendor() : base() { }

    public Vendor(string name, string email, string phone, string address, string contactPerson) : base()
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Phone = phone ?? throw new ArgumentNullException(nameof(phone));
        Address = address ?? throw new ArgumentNullException(nameof(address));
        ContactPerson = contactPerson ?? throw new ArgumentNullException(nameof(contactPerson));
        IsActive = true;
    }

    public void UpdateContactInfo(string phone, string address, string contactPerson)
    {
        Phone = phone ?? throw new ArgumentNullException(nameof(phone));
        Address = address ?? throw new ArgumentNullException(nameof(address));
        ContactPerson = contactPerson ?? throw new ArgumentNullException(nameof(contactPerson));
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}