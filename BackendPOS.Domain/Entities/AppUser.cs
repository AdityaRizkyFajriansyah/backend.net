namespace BackendPOS.Domain.Entities;

public class AppUser
{
    public Guid Id { get; set;}    
    public string Username { get; set;} = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set;} = "Cashier"; // Admin, Cashier
    public bool IsActive { get; set; } = true;  

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}