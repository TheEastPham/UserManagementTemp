using System.ComponentModel.DataAnnotations;

namespace UserManagement.Domain.DTOs.Role;

public record CreateRoleRequest(
    [Required, StringLength(50)] string Name,
    [StringLength(200)] string? Description = null
);
