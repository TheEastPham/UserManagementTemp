using System.ComponentModel.DataAnnotations;

namespace Base.UserManagement.Domain.DTOs;

// Auth DTOs
public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password,
    bool RememberMe = false
);

public record LoginResponse(
    bool Success,
    string Message,
    string? AccessToken = null,
    string? RefreshToken = null,
    DateTime? ExpiresAt = null,
    UserDto? User = null
);

public record RefreshTokenRequest(
    [Required] string RefreshToken
);

public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);

// Account DTOs
public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password,
    [Required] string ConfirmPassword,
    [Required] string FirstName,
    [Required] string LastName,
    string? PhoneNumber = null,
    string Language = "vi-VN"
);

public record RegisterResponse(
    bool Success,
    string Message,
    string? UserId = null
);

// User DTOs
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

public record CreateUserRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password,
    [Required] string FirstName,
    [Required] string LastName,
    DateTime? DateOfBirth = null,
    string? TimeZone = null,
    string Language = "vi-VN"
);

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

public record UpdateProfileRequest(
    [Required] string FirstName,
    [Required] string LastName,
    string? PhoneNumber = null
);

public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required, MinLength(8)] string NewPassword
);

public record GetUsersRequest(
    int Page = 1,
    int PageSize = 20,
    string? Role = null,
    string? SearchTerm = null
);

public record GetUsersResponse(
    IEnumerable<UserDto> Users,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public record AssignRoleRequest(
    [Required] string UserId,
    [Required] string RoleName
)
{
    public bool IsValidRole()
    {
        var validRoles = new[] { "SystemAdmin", "ContentAdmin", "Member" };
        return validRoles.Contains(RoleName);
    }
};

// Security Event DTO
public record SecurityEventDto(
    string Id,
    string EventType,
    string? UserId,
    string? UserEmail,
    string IpAddress,
    string UserAgent,
    Dictionary<string, object> Details,
    DateTime Timestamp,
    string Severity
);

// Role DTOs  
public record RoleDto(
    string Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateRoleRequest(
    [Required, StringLength(50)] string Name,
    [StringLength(200)] string? Description = null
);

public record UpdateRoleRequest(
    [Required] string Id,
    [Required, StringLength(50)] string Name,
    [StringLength(200)] string? Description = null,
    bool IsActive = true
);

// Validation Result
public record ValidationResult(
    bool IsValid,
    IList<string> Errors
);
