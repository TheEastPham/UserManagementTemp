namespace Base.UserManagement.Domain.DTOs.User;

public record UserDto(
    string Id,
    string Email,
    string? FirstName,
    string? LastName,
    string FullName,
    DateTime? DateOfBirth,
    string? Avatar,
    string? TimeZone,
    string? Language,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsActive,
    DateTime? LastLoginAt,
    IList<string> Roles
);
