namespace BackendPOS.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set;}
    public Guid UserId { get; set;}
    public AppUser User { get; set; } = null!;

    public string TokenHash { get; set; } = ""; // simpan hash, bukan token asli
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? RevokedAtUtc { get; set; }
    public string? ReplacedByTokenHash { get; set;}

    public bool IsActive => RevokedAtUtc == null && DateTime.UtcNow < ExpiresAtUtc;
}