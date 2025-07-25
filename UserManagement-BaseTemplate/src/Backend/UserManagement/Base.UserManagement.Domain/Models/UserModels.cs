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

/// <summary>
/// User profile domain model
/// </summary>
public class UserProfile
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Bio { get; set; }
    public Dictionary<string, object> Preferences { get; set; } = new();
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// System role domain model
/// </summary>
public class SystemRole
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Predefined role constants
    public static class RoleNames
    {
        public const string SystemAdmin = "SystemAdmin";
        public const string ContentAdmin = "ContentAdmin";
        public const string Member = "Member";
        
        public static readonly string[] All = { SystemAdmin, ContentAdmin, Member };
    }
}

/// <summary>
/// Security event domain model
/// </summary>
public class SecurityEvent
{
    public string Id { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public Dictionary<string, object> Details { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public string Severity { get; set; } = "Info";
}
