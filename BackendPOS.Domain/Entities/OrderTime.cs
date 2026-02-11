namespace BackendPOS.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = default!;

    public Guid ProductId { get; set;}
    public Product Product { get; set; } = default!;

    public int Qty { get; set; }
    public decimal UnitPrice { get; set; } // snapshot harga saat transaksi
    public decimal SubTotal { get; set; } // Qty * UnitPrice
}