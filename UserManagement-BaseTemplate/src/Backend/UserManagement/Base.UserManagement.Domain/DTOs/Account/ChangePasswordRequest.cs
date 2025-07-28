using System.ComponentModel.DataAnnotations;

namespace Base.UserManagement.Domain.DTOs.Account;

public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required, MinLength(8)] string NewPassword
);
