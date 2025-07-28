using Base.UserManagement.Domain.DTOs;

namespace Base.UserManagement.Domain.Services.Interfaces;

public interface IUserService
{
    Task<GetUsersResponse> GetUsersAsync(GetUsersRequest request);
    Task<UserDto?> GetUserByIdAsync(string id);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<UserDto> CreateUserAsync(CreateUserRequest request);
    Task<UserDto> UpdateUserAsync(UpdateUserRequest request);
    Task<bool> DeleteUserAsync(string id);
    Task<bool> AssignRoleAsync(AssignRoleRequest request);
    Task<bool> RemoveRoleAsync(string userId, string roleName);
    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
}
