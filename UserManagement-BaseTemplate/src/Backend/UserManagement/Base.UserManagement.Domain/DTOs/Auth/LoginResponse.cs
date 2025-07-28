using Base.UserManagement.Domain.DTOs.User;

namespace Base.UserManagement.Domain.DTOs.Auth;

public record LoginResponse(
    bool Success,
    string Message,
    string? AccessToken = null,
    string? RefreshToken = null,
    DateTime? ExpiresAt = null,
    UserDto? User = null
);
