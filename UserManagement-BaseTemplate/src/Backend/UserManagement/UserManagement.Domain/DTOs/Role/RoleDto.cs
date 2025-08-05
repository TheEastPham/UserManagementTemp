namespace UserManagement.Domain.DTOs.Role;

public record RoleDto(
    string Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt
);
