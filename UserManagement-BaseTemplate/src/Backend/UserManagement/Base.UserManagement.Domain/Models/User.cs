namespace Base.UserManagement.Domain.Models;

/// <summary>
/// User domain model for business logic
/// </summary>
public class User
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Avatar { get; set; }
    public string? TimeZone { get; set; }
    public string? Language { get; set; } = "vi-VN";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();
    public UserProfile? Profile { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
}
