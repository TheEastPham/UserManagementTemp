using Microsoft.AspNetCore.Identity;

namespace UserManagement.EFCore.Entities.User;

/// <summary>
/// User entity for database
/// </summary>
public class UserEntity : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Avatar { get; set; }
    public string? TimeZone { get; set; }
    public string? Language { get; set; } = "vi-VN";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    
    // Navigation properties
    public virtual UserProfileEntity? Profile { get; set; }
}
