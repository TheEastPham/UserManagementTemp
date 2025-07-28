using Base.UserManagement.Domain.DTOs.Auth;
using Base.UserManagement.Domain.DTOs.Account;

namespace Base.UserManagement.Domain.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task<bool> LogoutAsync(string userId);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task<VerifyEmailResponse> VerifyEmailAsync(VerifyEmailRequest request);
    Task<ResendVerificationResponse> ResendVerificationEmailAsync(ResendVerificationRequest request);
}
