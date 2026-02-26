using System.Dynamic;

namespace BackendPOS.Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OrderNo { get; set; } = default!;
    public OrderStatus Status { get; set; } = OrderStatus.Draft;

    public decimal Total { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<OrderItem> Items { get; set; } = new();

    public DateTime? PaidAtUtc { get; set;}
}