namespace BackendPOS.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public string Action { get; set; } = ""; // ORDER_CREATE / ORDER_PAY / ORDER_CANCEL
    public string? ActorUsername { get; set; }
    public string? ActorRole { get; set; }

    public string? EntityType { get; set; }   // "Order"
    public string? EntityId { get; set; }     // Guid.ToString()

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public string? DetailJson { get; set; }
}