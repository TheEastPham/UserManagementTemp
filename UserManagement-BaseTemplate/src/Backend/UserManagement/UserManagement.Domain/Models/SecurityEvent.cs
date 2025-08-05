namespace UserManagement.Domain.Models;

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
