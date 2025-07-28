using Base.UserManagement.EFCore.Data;
using Base.UserManagement.EFCore.Entities;
using Base.UserManagement.EFCore.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Base.UserManagement.EFCore.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManagementDbContext _context;

    public UserRepository(UserManagementDbContext context)
    {
        _context = context;
    }

    public async Task<UserEntity?> GetByIdAsync(string id)
    {
        return await _context.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<UserEntity?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<UserEntity>> GetAllAsync(int page = 1, int pageSize = 20, string? role = null)
    {
        var query = _context.Users
            .Include(u => u.Profile)
            .AsQueryable();

        if (!string.IsNullOrEmpty(role))
        {
            var usersInRole = await _context.UserRoles
                .Where(ur => _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == role))
                .Select(ur => ur.UserId)
                .ToListAsync();

            query = query.Where(u => usersInRole.Contains(u.Id));
        }

        return await query
            .OrderBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<UserEntity> CreateAsync(UserEntity user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<UserEntity> UpdateAsync(UserEntity user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> CountAsync(string? role = null)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrEmpty(role))
        {
            var usersInRole = await _context.UserRoles
                .Where(ur => _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == role))
                .Select(ur => ur.UserId)
                .ToListAsync();

            query = query.Where(u => usersInRole.Contains(u.Id));
        }

        return await query.CountAsync();
    }
}
