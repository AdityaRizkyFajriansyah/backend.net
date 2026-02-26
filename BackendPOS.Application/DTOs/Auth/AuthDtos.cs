namespace BackendPOS.Application.DTOs.Auth;

public record RegisterRequest(string Username, string Password, string Role);
public record LoginRequest(string Username, string Password);
public record TokenResponse(string AccessToken, string RefreshToken, int ExpiresInSeconds);
public record RefreshRequest(string RefreshToken);