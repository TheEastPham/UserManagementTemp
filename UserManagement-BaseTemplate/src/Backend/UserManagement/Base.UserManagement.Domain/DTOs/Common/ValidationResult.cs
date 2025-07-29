namespace Base.UserManagement.Domain.DTOs.Common;

public record ValidationResult(
    bool IsValid,
    IList<string> Errors
);
