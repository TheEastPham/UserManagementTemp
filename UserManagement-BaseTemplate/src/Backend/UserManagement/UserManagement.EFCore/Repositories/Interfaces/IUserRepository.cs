using UserManagement.EFCore.Entities.User;

namespace UserManagement.EFCore.Repositories.Interfaces;

public interface IUserRepository
{
    Task<UserEntity?> GetByIdAsync(string id);
    Task<UserEntity?> GetByEmailAsync(string email);
    Task<IEnumerable<UserEntity>> GetAllAsync(int page = 1, int pageSize = 20, string? role = null);
    Task<UserEntity> CreateAsync(UserEntity user);
    Task<UserEntity> UpdateAsync(UserEntity user);
    Task<bool> DeleteAsync(string id);
    Task<int> CountAsync(string? role = null);
}
