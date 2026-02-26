namespace BackendPOS.Domain.Entities;

public class Payment
{
    public Guid Id { get; set;} = Guid.NewGuid();
    
    public Guid OrderId { get; set;}
    public Order Order { get; set; } = null!;
    
    public PaymentMethod Method { get; set; }
    public decimal Amount { get; set; }
    public decimal? Change { get; set;}
    public string? Reference { get; set; }

    public DateTime PaidAtUtc { get; set; } = DateTime.UtcNow;
}