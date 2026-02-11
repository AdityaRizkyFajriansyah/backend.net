using BackendPOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackendPOS.Infrastructure.Data;

public class PosDbContext : DbContext
{
    public PosDbContext(DbContextOptions<PosDbContext> options) : base(options){}
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ORDER

        modelBuilder.Entity<Order>(e =>
        {
            e.ToTable("orders");
            e.HasKey(x => x.Id);

            e.Property(x => x.OrderNo)
                .HasMaxLength(30)
                .IsRequired();

            e.Property(x => x.Total)
                .HasPrecision(18,2);
        });

        // ORDER ITEM

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.ToTable("order_items");
            e.HasKey(x => x.Id);

            e.Property(x => x.UnitPrice)
                .HasPrecision(18,2);
            
            e.Property(x => x.SubTotal)
                .HasPrecision(18,2);
            
            e.HasOne(x => x.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(x => x.OrderId);

            e.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId);
        });
    }
}