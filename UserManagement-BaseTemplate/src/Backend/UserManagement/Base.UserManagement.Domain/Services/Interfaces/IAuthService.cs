using Base.UserManagement.Domain.DTOs;

namespace Base.UserManagement.Domain.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task<bool> LogoutAsync(string userId);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
}
