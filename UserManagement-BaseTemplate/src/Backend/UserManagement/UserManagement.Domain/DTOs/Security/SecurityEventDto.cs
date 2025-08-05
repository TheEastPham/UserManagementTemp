namespace UserManagement.Domain.DTOs.Security;

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
