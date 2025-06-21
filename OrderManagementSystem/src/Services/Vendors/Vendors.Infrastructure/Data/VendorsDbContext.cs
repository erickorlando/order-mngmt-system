using Microsoft.EntityFrameworkCore;
using Vendors.Domain.Entities;

namespace Vendors.Infrastructure.Data;

public class VendorsDbContext : DbContext
{
    public VendorsDbContext(DbContextOptions<VendorsDbContext> options) : base(options)
    {
    }

    public DbSet<Vendor> Vendors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuraciones de entidades
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VendorsDbContext).Assembly);
    }
} 