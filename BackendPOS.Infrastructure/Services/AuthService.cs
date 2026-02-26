using BackendPOS.Application.DTOs.Auth;
using BackendPOS.Application.Services;
using BackendPOS.Domain.Entities;
using BackendPOS.Infrastructure.Data;

using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BackendPOS.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly PosDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(PosDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task RegisterAsync(RegisterRequest req)
    {
        var username = req.Username.Trim();
        if (username.Length < 3) throw new ArgumentException("Username too short");
        if (req.Password.Length < 8) throw new ArgumentException("Password must be >= 8 chars");

        var role = req.Role is "Admin" or "Cashier" ? req.Role : throw new ArgumentException("Invalid role");

        var exists = await _db.Users.AnyAsync(u => u.Username == username);
        if (exists) throw new ArgumentException("Username already exists");

        var hash = BCrypt.Net.BCrypt.HashPassword(req.Password, workFactor: 12);

        _db.Users.Add(new AppUser
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = hash,
            Role = role,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest req, string? ip, string? userAgent)
    {
        var username = req.Username.Trim();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user == null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        var accessToken = CreateJwt(user);

        var (refreshPlain, refreshHash) = CreateRefreshToken();
        _db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshHash,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(14)
        });

        await _db.SaveChangesAsync();
        return new TokenResponse(accessToken, refreshPlain, GetAccessExpiresSeconds());
    }

    public async Task<TokenResponse> RefreshAsync(RefreshRequest req, string? ip, string? userAgent)
    {
        var incomingHash = Sha256(req.RefreshToken);

        var rt = await _db.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == incomingHash);

        if (rt == null || !rt.IsActive || !rt.User.IsActive)
            throw new UnauthorizedAccessException("Invalid refresh token");

        // rotation
        rt.RevokedAtUtc = DateTime.UtcNow;

        var (newPlain, newHash) = CreateRefreshToken();
        rt.ReplacedByTokenHash = newHash;

        _db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = rt.UserId,
            TokenHash = newHash,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(14)
        });

        var newAccess = CreateJwt(rt.User);
        await _db.SaveChangesAsync();

        return new TokenResponse(newAccess, newPlain, GetAccessExpiresSeconds());
    }

    public async Task LogoutAsync(RefreshRequest req)
    {
        var incomingHash = Sha256(req.RefreshToken);

        var rt = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == incomingHash);
        if (rt == null) return;

        rt.RevokedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    private string CreateJwt(AppUser user)
    {
        var jwt = _config.GetSection("Jwt");

        var issuer = jwt.GetValue<string>("Issuer") ?? "BackendPOS";
        var audience = jwt.GetValue<string>("Audience") ?? "BackendPOS";
        var keyText = jwt.GetValue<string>("Key") ?? throw new InvalidOperationException("Jwt:Key missing");
        var expiresMinutes = jwt.GetValue<int?>("ExpiresMinutes") ?? 30;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyText));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private int GetAccessExpiresSeconds()
    {
        var jwt = _config.GetSection("Jwt");
        var minutes = jwt.GetValue<int?>("ExpiresMinutes") ?? 30;
        return minutes * 60;
    }

    private static (string plain, string hash) CreateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var plain = Base64UrlEncoder.Encode(bytes);
        var hash = Sha256(plain);
        return (plain, hash);
    }

    private static string Sha256(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
