using System.ComponentModel.DataAnnotations;

namespace UserManagement.Domain.DTOs.Auth;

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password,
    bool RememberMe = false
);
