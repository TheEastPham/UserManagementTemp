using Base.UserManagement.EFCore.Data;
using Base.UserManagement.EFCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace Base.UserManagement.EFCore.Repositories;

public interface ISystemRoleRepository
{
    Task<IEnumerable<SystemRoleEntity>> GetAllAsync();
    Task<SystemRoleEntity?> GetByIdAsync(int id);
    Task<SystemRoleEntity?> GetByNameAsync(string name);
    Task<SystemRoleEntity> CreateAsync(SystemRoleEntity role);
    Task<SystemRoleEntity> UpdateAsync(SystemRoleEntity role);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(string name, int? excludeId = null);
}

public class SystemRoleRepository : ISystemRoleRepository
{
    private readonly UserManagementDbContext _context;

    public SystemRoleRepository(UserManagementDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SystemRoleEntity>> GetAllAsync()
    {
        return await _context.SystemRoles
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<SystemRoleEntity?> GetByIdAsync(int id)
    {
        return await _context.SystemRoles
            .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);
    }

    public async Task<SystemRoleEntity?> GetByNameAsync(string name)
    {
        return await _context.SystemRoles
            .FirstOrDefaultAsync(r => r.Name == name && r.IsActive);
    }

    public async Task<SystemRoleEntity> CreateAsync(SystemRoleEntity role)
    {
        role.CreatedAt = DateTime.UtcNow;
        _context.SystemRoles.Add(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task<SystemRoleEntity> UpdateAsync(SystemRoleEntity role)
    {
        role.UpdatedAt = DateTime.UtcNow;
        _context.SystemRoles.Update(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var role = await _context.SystemRoles.FindAsync(id);
        if (role == null) return false;

        // Soft delete
        role.IsActive = false;
        role.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(string name, int? excludeId = null)
    {
        var query = _context.SystemRoles.Where(r => r.Name == name);
        
        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}
