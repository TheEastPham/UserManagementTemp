using Base.UserManagement.Domain.DTOs.Role;
using Base.UserManagement.Domain.Services.Interfaces;
using Base.UserManagement.EFCore.Entities.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Base.UserManagement.Domain.Services;

public class RoleService : IRoleService
{
    private readonly RoleManager<RoleEntity> _roleManager;
    private readonly ILogger<RoleService> _logger;

    public RoleService(
        RoleManager<RoleEntity> roleManager,
        ILogger<RoleService> logger)
    {
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
    {
        try
        {
            var roles = await _roleManager.Roles
                .Where(r => r.IsActive)
                .ToListAsync();

            return roles.Select(r => new RoleDto(
                r.Id,
                r.Name!,
                r.Description,
                r.IsActive,
                r.CreatedAt
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all roles");
            throw;
        }
    }

    public async Task<RoleDto?> GetRoleByIdAsync(string id)
    {
        try
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return null;

            return new RoleDto(
                role.Id,
                role.Name!,
                role.Description,
                role.IsActive,
                role.CreatedAt
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role {RoleId}", id);
            throw;
        }
    }

    public async Task<RoleDto?> GetRoleByNameAsync(string name)
    {
        try
        {
            var role = await _roleManager.FindByNameAsync(name);
            if (role == null) return null;

            return new RoleDto(
                role.Id,
                role.Name!,
                role.Description,
                role.IsActive,
                role.CreatedAt
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role {RoleName}", name);
            throw;
        }
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleRequest request)
    {
        try
        {
            // Check if role already exists
            var existingRole = await _roleManager.FindByNameAsync(request.Name);
            if (existingRole != null)
            {
                throw new InvalidOperationException($"Role with name '{request.Name}' already exists");
            }

            var role = new RoleEntity
            {
                Name = request.Name,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            return new RoleDto(
                role.Id,
                role.Name,
                role.Description,
                role.IsActive,
                role.CreatedAt
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role {RoleName}", request.Name);
            throw;
        }
    }

    public async Task<RoleDto> UpdateRoleAsync(UpdateRoleRequest request)
    {
        try
        {
            var role = await _roleManager.FindByIdAsync(request.Id);
            if (role == null)
            {
                throw new InvalidOperationException($"Role with ID {request.Id} not found");
            }

            // Check if new name conflicts with existing role (excluding current role)
            if (role.Name != request.Name)
            {
                var nameConflict = await _roleManager.FindByNameAsync(request.Name);
                if (nameConflict != null && nameConflict.Id != request.Id)
                {
                    throw new InvalidOperationException($"Role with name '{request.Name}' already exists");
                }
            }

            role.Name = request.Name;
            role.Description = request.Description;
            role.IsActive = request.IsActive;

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to update role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            return new RoleDto(
                role.Id,
                role.Name,
                role.Description,
                role.IsActive,
                role.CreatedAt
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role {RoleId}", request.Id);
            throw;
        }
    }

    public async Task<bool> DeleteRoleAsync(string id)
    {
        try
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return false;

            // Check if it's a predefined system role
            var systemRoles = new[] { "SystemAdmin", "ContentAdmin", "Member" };
            if (systemRoles.Contains(role.Name))
            {
                throw new InvalidOperationException($"Cannot delete predefined system role '{role.Name}'");
            }

            var result = await _roleManager.DeleteAsync(role);
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role {RoleId}", id);
            throw;
        }
    }

    public async Task<bool> IsValidRoleAsync(string roleName)
    {
        try
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            return role != null && role.IsActive;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating role name {RoleName}", roleName);
            return false;
        }
    }
}
