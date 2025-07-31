namespace Base.UserManagement.Domain.DTOs.Auth;

public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);
