using BackendPOS.Application.DTOs.Auth;

namespace BackendPOS.Application.Services;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequest req);
    Task<TokenResponse> LoginAsync(LoginRequest req, string? ip, string? userAgent);
    Task<TokenResponse> RefreshAsync(RefreshRequest req, string? ip, string? userAgent);
    Task LogoutAsync(RefreshRequest req);
}