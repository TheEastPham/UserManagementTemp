using Base.UserManagement.EFCore.Data;
using Base.UserManagement.EFCore.Entities.User;
using Base.UserManagement.EFCore.Repositories;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace Base.UserManagement.EFCore.UnitTests.TestCase;

public class SystemRoleRepositoryTests : IDisposable
{
    private readonly UserManagementDbContext _context;
    private readonly SystemRoleRepository _systemRoleRepository;

    public SystemRoleRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<UserManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new UserManagementDbContext(options);
        _systemRoleRepository = new SystemRoleRepository(_context);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var systemRoles = new List<SystemRoleEntity>
        {
            new SystemRoleEntity
            {
                Id = 1,
                Name = "SystemAdmin",
                Description = "System Administrator with full access",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new SystemRoleEntity
            {
                Id = 2,
                Name = "ContentAdmin",
                Description = "Content Administrator",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new SystemRoleEntity
            {
                Id = 3,
                Name = "Member",
                Description = "Regular member",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new SystemRoleEntity
            {
                Id = 4,
                Name = "InactiveRole",
                Description = "Inactive role",
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.SystemRoles.AddRange(systemRoles);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllSystemRoles()
    {
        // Act
        var roles = await _systemRoleRepository.GetAllAsync();

        // Assert
        roles.Should().NotBeNull();
        roles.Should().HaveCount(4);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnSystemRole()
    {
        // Arrange
        var roleId = 1;

        // Act
        var role = await _systemRoleRepository.GetByIdAsync(roleId);

        // Assert
        role.Should().NotBeNull();
        role!.Id.Should().Be(roleId);
        role.Name.Should().Be("SystemAdmin");
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var roleId = 999;

        // Act
        var role = await _systemRoleRepository.GetByIdAsync(roleId);

        // Assert
        role.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_WithValidName_ShouldReturnSystemRole()
    {
        // Arrange
        var roleName = "ContentAdmin";

        // Act
        var role = await _systemRoleRepository.GetByNameAsync(roleName);

        // Assert
        role.Should().NotBeNull();
        role!.Name.Should().Be(roleName);
        role.Description.Should().Be("Content Administrator");
    }

    [Fact]
    public async Task GetByNameAsync_WithInvalidName_ShouldReturnNull()
    {
        // Arrange
        var roleName = "NonExistentRole";

        // Act
        var role = await _systemRoleRepository.GetByNameAsync(roleName);

        // Assert
        role.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveRolesAsync_ShouldReturnOnlyActiveRoles()
    {
        // Act
        var activeRoles = await _systemRoleRepository.GetActiveRolesAsync();

        // Assert
        activeRoles.Should().NotBeNull();
        activeRoles.Should().HaveCount(3);
        activeRoles.Should().OnlyContain(r => r.IsActive);
        activeRoles.Should().Contain(r => r.Name == "SystemAdmin");
        activeRoles.Should().Contain(r => r.Name == "ContentAdmin");
        activeRoles.Should().Contain(r => r.Name == "Member");
    }

    [Fact]
    public async Task AddAsync_ShouldAddSystemRoleToDatabase()
    {
        // Arrange
        var newRole = new SystemRoleEntity
        {
            Name = "NewRole",
            Description = "A new test role",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await _systemRoleRepository.AddAsync(newRole);
        await _systemRoleRepository.SaveChangesAsync();

        // Assert
        var savedRole = await _systemRoleRepository.GetByNameAsync("NewRole");
        savedRole.Should().NotBeNull();
        savedRole!.Description.Should().Be("A new test role");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateSystemRoleInDatabase()
    {
        // Arrange
        var role = await _systemRoleRepository.GetByNameAsync("Member");
        role!.Description = "Updated member description";

        // Act
        _systemRoleRepository.Update(role);
        await _systemRoleRepository.SaveChangesAsync();

        // Assert
        var updatedRole = await _systemRoleRepository.GetByNameAsync("Member");
        updatedRole.Should().NotBeNull();
        updatedRole!.Description.Should().Be("Updated member description");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveSystemRoleFromDatabase()
    {
        // Arrange
        var role = await _systemRoleRepository.GetByNameAsync("InactiveRole");

        // Act
        _systemRoleRepository.Delete(role!);
        await _systemRoleRepository.SaveChangesAsync();

        // Assert
        var deletedRole = await _systemRoleRepository.GetByNameAsync("InactiveRole");
        deletedRole.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
