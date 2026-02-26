using System.Security.Cryptography.X509Certificates;
using BackendPOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackendPOS.Infrastructure.Data;

public class PosDbContext : DbContext
{
    public PosDbContext(DbContextOptions<PosDbContext> options) : base(options){}
    
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<RefreshToken> RefreshTokens => Set <RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

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
            e.Property(x => x.PaidAtUtc);
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

        modelBuilder.Entity<Payment>( e =>
        {
            e.ToTable("payments");
            e.HasKey(x => x.Id);

            e.Property(x => x.Amount)
                .HasPrecision(18,2);
            e.Property(x => x.Change)
                .HasPrecision(18, 2);
            e.Property(x => x.Reference)
                .HasMaxLength(100);
            
            e.HasOne(x => x.Order)
                .WithMany()
                .HasForeignKey(x => x.OrderId);
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.ToTable("audit_logs");
            e.HasKey(x => x.Id);
            e.Property(x => x.Action).HasMaxLength(64).IsRequired();
        });
    }
}