using System.ComponentModel.DataAnnotations;

namespace Base.UserManagement.Domain.DTOs.User;

public record UpdateProfileRequest(
    [Required] string FirstName,
    [Required] string LastName,
    string? PhoneNumber = null
);
