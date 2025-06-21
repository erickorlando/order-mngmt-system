using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.Property(o => o.VendorId)
            .IsRequired();

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>();

        // Configurar el ValueObject Money usando OwnsOne
        builder.OwnsOne(o => o.TotalAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("TotalAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();
        });

        builder.Property(o => o.OrderDate)
            .IsRequired();

        builder.Property(o => o.Notes)
            .HasMaxLength(500);

        // Configurar las relaciones
        builder.HasOne(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // No configuramos la relación con Vendor ya que está en otro microservicio
        // builder.HasOne(o => o.Vendor) - Esto se ignora

        builder.HasMany(o => o.OrderItems)
            .WithOne()
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configurar índices
        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();

        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.VendorId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.OrderDate);
    }
}