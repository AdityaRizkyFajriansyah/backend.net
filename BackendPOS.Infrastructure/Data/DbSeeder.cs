using System.Data.Common;
using BackendPOS.Domain.Entities;
using BCrypt.Net;
using Microsoft.AspNetCore.Identity;

namespace BackendPOS.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(PosDbContext db)
    {
        if (!db.Users.Any(u => u.Username == "admin"))
        {
            db.Users.Add(new AppUser
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin12345", workFactor: 12),
                Role = "Admin",
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }
    }
}