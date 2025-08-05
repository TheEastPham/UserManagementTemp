using System.ComponentModel.DataAnnotations;

namespace UserManagement.Domain.DTOs.User;

public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required, MinLength(8)] string NewPassword
);
