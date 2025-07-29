using System.ComponentModel.DataAnnotations;

namespace Base.UserManagement.Domain.DTOs.Role;

public record AssignRoleRequest(
    [Required] string UserId,
    [Required] string RoleName
)
{
    public bool IsValidRole()
    {
        var validRoles = new[] { "SystemAdmin", "ContentAdmin", "Member" };
        return validRoles.Contains(RoleName);
    }
};
