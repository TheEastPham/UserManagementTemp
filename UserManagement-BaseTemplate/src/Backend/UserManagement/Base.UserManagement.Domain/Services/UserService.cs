using AutoMapper;
using Base.UserManagement.Domain.DTOs.User;
using Base.UserManagement.Domain.DTOs.Role;
using Base.UserManagement.Domain.Models;
using Base.UserManagement.Domain.Services.Interfaces;
using Base.UserManagement.EFCore.Entities.User;
using Base.UserManagement.EFCore.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Base.UserManagement.Domain.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        UserManager<UserEntity> userManager,
        IMapper mapper,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _userManager = userManager;
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
                user.Roles = roles;
                
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
       ValidateArgument(request);
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
                IsActive = false
            };

            var result = await _userManager.CreateAsync(userEntity, request.Password);
            if (result == null || !result.Succeeded)
            {
                var errors = result?.Errors?.Select(e => e.Description) ?? new List<string> { "Unknown error occurred." };
                throw new InvalidOperationException($"Failed to create user: {string.Join(", ", errors)}");
            }

            // Assign default Member role
            //await _roleManager.AddToRoleAsync(userEntity, "Member");
            var user = _mapper.Map<User>(userEntity);
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
    
    private void ValidateArgument(CreateUserRequest request)
    {
        if (request == null)
            throw new ArgumentException(nameof(request));
        if (string.IsNullOrEmpty(request.Email))
            throw new ArgumentException(nameof(request.Email));
        if (string.IsNullOrEmpty(request.Password))
            throw new ArgumentException(nameof(request.Password));
        if (string.IsNullOrEmpty(request.FirstName))
            throw new ArgumentException(nameof(request.FirstName));
        if (request.LastName == null)
            throw new ArgumentException(nameof(request.LastName));
    }

    private User Generate(UserEntity entity)
    {
        return new User
        {
            Id = entity.Id,
            Email = entity.Email,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            DateOfBirth = entity.DateOfBirth,
            Avatar = entity.Avatar,
            TimeZone = entity.TimeZone,
            Language = entity.Language,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsActive = entity.IsActive,
            LastLoginAt = entity.LastLoginAt,
            Profile = entity.Profile != null ? new UserProfile
            {
                Id = entity.Profile.Id,
                UserId = entity.Profile.UserId,
                PhoneNumber = entity.Profile.PhoneNumber,
                Address = entity.Profile.Address,
                Bio = entity.Profile.Bio,
                Preferences = new Dictionary<string, object>(),
                UpdatedAt = entity.Profile.UpdatedAt
            } : null,
            Roles = new List<string>() 
        };
    }
}
