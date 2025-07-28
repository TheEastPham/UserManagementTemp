using System.ComponentModel.DataAnnotations;

namespace Base.UserManagement.Domain.DTOs.User;

public record CreateUserRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password,
    [Required] string FirstName,
    [Required] string LastName,
    DateTime? DateOfBirth = null,
    string? TimeZone = null,
    string Language = "vi-VN"
);
