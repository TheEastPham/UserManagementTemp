namespace Base.UserManagement.Domain.DTOs.Account;

public record ResendVerificationResponse(
    bool Success,
    string Message
);
