using Microsoft.AspNetCore.Identity;

namespace UserManagement.EFCore.Entities.User;

/// <summary>
/// Role entity for database
/// </summary>
public class RoleEntity : IdentityRole
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
