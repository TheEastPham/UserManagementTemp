namespace Base.UserManagement.Domain.DTOs.Account;

public record RegisterResponse(
    bool Success,
    string Message,
    string? UserId = null,
    bool RequiresEmailVerification = true
);
