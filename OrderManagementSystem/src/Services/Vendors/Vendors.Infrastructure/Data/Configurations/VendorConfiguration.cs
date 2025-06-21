using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vendors.Domain.Entities;

namespace Vendors.Infrastructure.Data.Configurations;

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("Vendors");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(v => v.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(v => v.ContactPerson)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.IsActive)
            .IsRequired();

        // Índices
        builder.HasIndex(v => v.Email).IsUnique();
        builder.HasIndex(v => v.IsActive);
    }
} 