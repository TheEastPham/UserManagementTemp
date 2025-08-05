using System.ComponentModel.DataAnnotations;

namespace UserManagement.Domain.DTOs.Role;

public record UpdateRoleRequest(
    [Required] string Id,
    [Required, StringLength(50)] string Name,
    [StringLength(200)] string? Description = null,
    bool IsActive = true
);
