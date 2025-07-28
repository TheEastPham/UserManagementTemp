using Base.UserManagement.Domain.DTOs;

namespace Base.UserManagement.Domain.Services.Interfaces;

public interface IRoleService
{
    Task<IEnumerable<RoleDto>> GetAllRolesAsync();
    Task<RoleDto?> GetRoleByIdAsync(string id);
    Task<RoleDto?> GetRoleByNameAsync(string name);
    Task<RoleDto> CreateRoleAsync(CreateRoleRequest request);
    Task<RoleDto> UpdateRoleAsync(UpdateRoleRequest request);
    Task<bool> DeleteRoleAsync(string id);
    Task<bool> IsValidRoleAsync(string roleName);
}
