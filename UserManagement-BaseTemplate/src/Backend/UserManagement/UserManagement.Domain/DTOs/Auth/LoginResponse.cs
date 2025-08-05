using UserManagement.Domain.DTOs.User;

namespace UserManagement.Domain.DTOs.Auth;

public record LoginResponse(
    bool Success,
    string Message,
    string? AccessToken = null,
    string? RefreshToken = null,
    DateTime? ExpiresAt = null,
    UserDto? User = null
);
