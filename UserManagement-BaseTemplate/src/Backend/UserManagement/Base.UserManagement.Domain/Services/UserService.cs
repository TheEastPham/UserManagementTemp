using AutoMapper;
using Base.UserManagement.Domain.DTOs;
using Base.UserManagement.Domain.Models;
using Base.UserManagement.Domain.Services.Interfaces;
using Base.UserManagement.EFCore.Entities;
using Base.UserManagement.EFCore.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Base.UserManagement.Domain.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<UserEntity> _userManager;
    private readonly RoleManager<RoleEntity> _roleManager;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        UserManager<UserEntity> userManager,
        RoleManager<RoleEntity> roleManager,
        IMapper mapper,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _roleManager = roleManager;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<GetUsersResponse> GetUsersAsync(GetUsersRequest request)
    {
        try
        {
            var userEntities = await _userRepository.GetAllAsync(request.Page, request.PageSize, request.Role);
            var totalCount = await _userRepository.CountAsync(request.Role);

            var users = new List<UserDto>();
            foreach (var userEntity in userEntities)
            {
                var roles = await _userManager.GetRolesAsync(userEntity);
                var user = _mapper.Map<User>(userEntity);
                user.Roles = roles.ToList();
                
                var userDto = _mapper.Map<UserDto>(user);
                users.Add(userDto);
            }

            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            return new GetUsersResponse(users, totalCount, request.Page, request.PageSize, totalPages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users for page {Page}", request.Page);
            throw;
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(string id)
    {
        try
        {
            var userEntity = await _userRepository.GetByIdAsync(id);
            if (userEntity == null) return null;

            var roles = await _userManager.GetRolesAsync(userEntity);
            var user = _mapper.Map<User>(userEntity);
            user.Roles = roles.ToList();

            return _mapper.Map<UserDto>(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", id);
            throw;
        }
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        try
        {
            var userEntity = await _userRepository.GetByEmailAsync(email);
            if (userEntity == null) return null;

            var roles = await _userManager.GetRolesAsync(userEntity);
            var user = _mapper.Map<User>(userEntity);
            user.Roles = roles.ToList();

            return _mapper.Map<UserDto>(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by email {Email}", email);
            throw;
        }
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            var userEntity = new UserEntity
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth,
                TimeZone = request.TimeZone,
                Language = request.Language,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(userEntity, request.Password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            // Assign default Member role
            await _userManager.AddToRoleAsync(userEntity, "Member");

            var roles = await _userManager.GetRolesAsync(userEntity);
            var user = _mapper.Map<User>(userEntity);
            user.Roles = roles.ToList();

            return _mapper.Map<UserDto>(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {Email}", request.Email);
            throw;
        }
    }

    public async Task<UserDto> UpdateUserAsync(UpdateUserRequest request)
    {
        try
        {
            var userEntity = await _userRepository.GetByIdAsync(request.Id);
            if (userEntity == null)
            {
                throw new InvalidOperationException($"User with ID {request.Id} not found");
            }

            userEntity.FirstName = request.FirstName;
            userEntity.LastName = request.LastName;
            userEntity.DateOfBirth = request.DateOfBirth;
            userEntity.Avatar = request.Avatar;
            userEntity.TimeZone = request.TimeZone;
            userEntity.Language = request.Language;
            userEntity.UpdatedAt = DateTime.UtcNow;
            
            // Update phone number and email if provided
            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                userEntity.PhoneNumber = request.PhoneNumber;
            }
            
            if (!string.IsNullOrEmpty(request.Email) && request.Email != userEntity.Email)
            {
                // Email update should be handled carefully - may require verification
                userEntity.Email = request.Email;
                userEntity.NormalizedEmail = request.Email.ToUpperInvariant();
                userEntity.UserName = request.Email;
                userEntity.NormalizedUserName = request.Email.ToUpperInvariant();
            }

            await _userRepository.UpdateAsync(userEntity);

            var roles = await _userManager.GetRolesAsync(userEntity);
            var user = _mapper.Map<User>(userEntity);
            user.Roles = roles.ToList();

            return _mapper.Map<UserDto>(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", request.Id);
            throw;
        }
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        try
        {
            return await _userRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            throw;
        }
    }

    public async Task<bool> AssignRoleAsync(AssignRoleRequest request)
    {
        try
        {
            if (!request.IsValidRole())
            {
                return false;
            }

            var userEntity = await _userRepository.GetByIdAsync(request.UserId);
            if (userEntity == null) return false;

            var result = await _userManager.AddToRoleAsync(userEntity, request.RoleName);
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role {RoleName} to user {UserId}", request.RoleName, request.UserId);
            throw;
        }
    }

    public async Task<bool> RemoveRoleAsync(string userId, string roleName)
    {
        try
        {
            var userEntity = await _userRepository.GetByIdAsync(userId);
            if (userEntity == null) return false;

            var result = await _userManager.RemoveFromRoleAsync(userEntity, roleName);
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role {RoleName} from user {UserId}", roleName, userId);
            throw;
        }
    }

    public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        try
        {
            var userEntity = await _userManager.FindByIdAsync(userId);
            if (userEntity == null)
            {
                throw new InvalidOperationException($"User with ID {userId} not found.");
            }

            var result = await _userManager.ChangePasswordAsync(userEntity, currentPassword, newPassword);
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            throw;
        }
    }
}
