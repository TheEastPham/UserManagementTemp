namespace UserManagement.Domain.DTOs.Account;

public record VerifyEmailResponse(
    bool Success,
    string Message
);
