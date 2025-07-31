using System.ComponentModel.DataAnnotations;

namespace Base.UserManagement.Domain.DTOs.Account;

public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password,
    [Required] string ConfirmPassword,
    [Required] string FirstName,
    [Required] string LastName,
    string? PhoneNumber = null,
    string Language = "vi-VN"
);
