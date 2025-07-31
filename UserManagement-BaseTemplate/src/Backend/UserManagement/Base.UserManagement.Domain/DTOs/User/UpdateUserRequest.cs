using System.ComponentModel.DataAnnotations;

namespace Base.UserManagement.Domain.DTOs.User;

public record UpdateUserRequest(
    [Required] string Id,
    [Required] string FirstName,
    [Required] string LastName,
    DateTime? DateOfBirth = null,
    string? Avatar = null,
    string? TimeZone = null,
    string? Language = null,
    string? PhoneNumber = null,
    string? Email = null
);
