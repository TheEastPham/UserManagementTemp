namespace Base.UserManagement.Domain.Models;

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
