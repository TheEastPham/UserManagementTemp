using AutoMapper;
using Base.UserManagement.Domain.DTOs;
using Base.UserManagement.Domain.Models;
using Base.UserManagement.EFCore.Entities;
using Base.UserManagement.EFCore.Repositories;
using Microsoft.Extensions.Logging;

namespace Base.UserManagement.Domain.Services;

public interface ISystemRoleService
{
    Task<IEnumerable<SystemRoleDto>> GetAllRolesAsync();
    Task<SystemRoleDto?> GetRoleByIdAsync(int id);
    Task<SystemRoleDto?> GetRoleByNameAsync(string name);
    Task<SystemRoleDto> CreateRoleAsync(CreateSystemRoleRequest request);
    Task<SystemRoleDto> UpdateRoleAsync(UpdateSystemRoleRequest request);
    Task<bool> DeleteRoleAsync(int id);
    Task<bool> IsValidRoleAsync(string roleName);
}

public class SystemRoleService : ISystemRoleService
{
    private readonly ISystemRoleRepository _systemRoleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<SystemRoleService> _logger;

    public SystemRoleService(
        ISystemRoleRepository systemRoleRepository,
        IMapper mapper,
        ILogger<SystemRoleService> logger)
    {
        _systemRoleRepository = systemRoleRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<SystemRoleDto>> GetAllRolesAsync()
    {
        try
        {
            var roleEntities = await _systemRoleRepository.GetAllAsync();
            var roles = _mapper.Map<IEnumerable<SystemRole>>(roleEntities);
            return _mapper.Map<IEnumerable<SystemRoleDto>>(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all system roles");
            throw;
        }
    }

    public async Task<SystemRoleDto?> GetRoleByIdAsync(int id)
    {
        try
        {
            var roleEntity = await _systemRoleRepository.GetByIdAsync(id);
            if (roleEntity == null) return null;

            var role = _mapper.Map<SystemRole>(roleEntity);
            return _mapper.Map<SystemRoleDto>(role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving system role {RoleId}", id);
            throw;
        }
    }

    public async Task<SystemRoleDto?> GetRoleByNameAsync(string name)
    {
        try
        {
            var roleEntity = await _systemRoleRepository.GetByNameAsync(name);
            if (roleEntity == null) return null;

            var role = _mapper.Map<SystemRole>(roleEntity);
            return _mapper.Map<SystemRoleDto>(role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving system role {RoleName}", name);
            throw;
        }
    }

    public async Task<SystemRoleDto> CreateRoleAsync(CreateSystemRoleRequest request)
    {
        try
        {
            // Check if role already exists
            var existingRole = await _systemRoleRepository.GetByNameAsync(request.Name);
            if (existingRole != null)
            {
                throw new InvalidOperationException($"Role with name '{request.Name}' already exists");
            }

            var role = _mapper.Map<SystemRole>(request);
            var roleEntity = _mapper.Map<SystemRoleEntity>(role);

            var createdEntity = await _systemRoleRepository.CreateAsync(roleEntity);
            var createdRole = _mapper.Map<SystemRole>(createdEntity);

            return _mapper.Map<SystemRoleDto>(createdRole);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating system role {RoleName}", request.Name);
            throw;
        }
    }

    public async Task<SystemRoleDto> UpdateRoleAsync(UpdateSystemRoleRequest request)
    {
        try
        {
            var existingEntity = await _systemRoleRepository.GetByIdAsync(request.Id);
            if (existingEntity == null)
            {
                throw new InvalidOperationException($"Role with ID {request.Id} not found");
            }

            // Check if new name conflicts with existing role
            var nameConflict = await _systemRoleRepository.ExistsAsync(request.Name, request.Id);
            if (nameConflict)
            {
                throw new InvalidOperationException($"Role with name '{request.Name}' already exists");
            }

            var role = _mapper.Map<SystemRole>(request);
            var roleEntity = _mapper.Map<SystemRoleEntity>(role);

            var updatedEntity = await _systemRoleRepository.UpdateAsync(roleEntity);
            var updatedRole = _mapper.Map<SystemRole>(updatedEntity);

            return _mapper.Map<SystemRoleDto>(updatedRole);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating system role {RoleId}", request.Id);
            throw;
        }
    }

    public async Task<bool> DeleteRoleAsync(int id)
    {
        try
        {
            var roleEntity = await _systemRoleRepository.GetByIdAsync(id);
            if (roleEntity == null) return false;

            // Check if it's a predefined system role
            if (SystemRole.RoleNames.All.Contains(roleEntity.Name))
            {
                throw new InvalidOperationException($"Cannot delete predefined system role '{roleEntity.Name}'");
            }

            return await _systemRoleRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting system role {RoleId}", id);
            throw;
        }
    }

    public async Task<bool> IsValidRoleAsync(string roleName)
    {
        try
        {
            var roleEntity = await _systemRoleRepository.GetByNameAsync(roleName);
            return roleEntity != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating role name {RoleName}", roleName);
            return false;
        }
    }
}
