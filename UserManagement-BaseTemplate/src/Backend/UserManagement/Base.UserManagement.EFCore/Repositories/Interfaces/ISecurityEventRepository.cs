using Base.UserManagement.EFCore.Entities;

namespace Base.UserManagement.EFCore.Repositories.Interfaces;

public interface ISecurityEventRepository
{
    Task<SecurityEventEntity> CreateAsync(SecurityEventEntity securityEvent);
    Task<IEnumerable<SecurityEventEntity>> GetByUserIdAsync(string userId, int page = 1, int pageSize = 20);
    Task<IEnumerable<SecurityEventEntity>> GetAllAsync(int page = 1, int pageSize = 20, string? eventType = null);
    Task<int> CountAsync(string? userId = null, string? eventType = null);
}
