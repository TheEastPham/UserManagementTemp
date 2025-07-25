using Base.UserManagement.EFCore.Data;
using Base.UserManagement.EFCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace Base.UserManagement.EFCore.Repositories;

public interface ISecurityEventRepository
{
    Task<SecurityEventEntity> CreateAsync(SecurityEventEntity securityEvent);
    Task<IEnumerable<SecurityEventEntity>> GetByUserIdAsync(string userId, int page = 1, int pageSize = 20);
    Task<IEnumerable<SecurityEventEntity>> GetAllAsync(int page = 1, int pageSize = 20, string? eventType = null);
    Task<int> CountAsync(string? userId = null, string? eventType = null);
}

public class SecurityEventRepository : ISecurityEventRepository
{
    private readonly UserManagementDbContext _context;

    public SecurityEventRepository(UserManagementDbContext context)
    {
        _context = context;
    }

    public async Task<SecurityEventEntity> CreateAsync(SecurityEventEntity securityEvent)
    {
        securityEvent.Timestamp = DateTime.UtcNow;
        _context.SecurityEvents.Add(securityEvent);
        await _context.SaveChangesAsync();
        return securityEvent;
    }

    public async Task<IEnumerable<SecurityEventEntity>> GetByUserIdAsync(string userId, int page = 1, int pageSize = 20)
    {
        return await _context.SecurityEvents
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<SecurityEventEntity>> GetAllAsync(int page = 1, int pageSize = 20, string? eventType = null)
    {
        var query = _context.SecurityEvents.AsQueryable();

        if (!string.IsNullOrEmpty(eventType))
        {
            query = query.Where(e => e.EventType == eventType);
        }

        return await query
            .OrderByDescending(e => e.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(string? userId = null, string? eventType = null)
    {
        var query = _context.SecurityEvents.AsQueryable();

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(e => e.UserId == userId);
        }

        if (!string.IsNullOrEmpty(eventType))
        {
            query = query.Where(e => e.EventType == eventType);
        }

        return await query.CountAsync();
    }
}
