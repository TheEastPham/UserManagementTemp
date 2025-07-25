using Base.UserManagement.EFCore.Data;
using Base.UserManagement.EFCore.Entities;
using Base.UserManagement.EFCore.Repositories;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace Base.UserManagement.EFCore.UnitTests;

public class UserRepositoryTests : IDisposable
{
    private readonly UserManagementDbContext _context;
    private readonly UserRepository _userRepository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<UserManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new UserManagementDbContext(options);
        _userRepository = new UserRepository(_context);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var users = new List<UserEntity>
        {
            new UserEntity
            {
                Id = "1",
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com",
                UserName = "john@test.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new UserEntity
            {
                Id = "2",
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@test.com",
                UserName = "jane@test.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new UserEntity
            {
                Id = "3",
                FirstName = "Bob",
                LastName = "Wilson",
                Email = "bob@test.com",
                UserName = "bob@test.com",
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _context.Users.AddRange(users);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        // Act
        var users = await _userRepository.GetAllAsync();

        // Assert
        users.Should().NotBeNull();
        users.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var userId = "1";

        // Act
        var user = await _userRepository.GetByIdAsync(userId);

        // Assert
        user.Should().NotBeNull();
        user!.Id.Should().Be(userId);
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var userId = "invalid";

        // Act
        var user = await _userRepository.GetByIdAsync(userId);

        // Assert
        user.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveUsersAsync_ShouldReturnOnlyActiveUsers()
    {
        // Act
        var activeUsers = await _userRepository.GetActiveUsersAsync();

        // Assert
        activeUsers.Should().NotBeNull();
        activeUsers.Should().HaveCount(2);
        activeUsers.Should().OnlyContain(u => u.IsActive);
    }

    [Fact]
    public async Task SearchUsersAsync_WithFirstName_ShouldReturnMatchingUsers()
    {
        // Arrange
        var searchTerm = "John";

        // Act
        var users = await _userRepository.SearchUsersAsync(searchTerm);

        // Assert
        users.Should().NotBeNull();
        users.Should().HaveCount(1);
        users.First().FirstName.Should().Be("John");
    }

    [Fact]
    public async Task SearchUsersAsync_WithEmail_ShouldReturnMatchingUsers()
    {
        // Arrange
        var searchTerm = "jane@test.com";

        // Act
        var users = await _userRepository.SearchUsersAsync(searchTerm);

        // Assert
        users.Should().NotBeNull();
        users.Should().HaveCount(1);
        users.First().Email.Should().Be("jane@test.com");
    }

    [Fact]
    public async Task AddAsync_ShouldAddUserToDatabase()
    {
        // Arrange
        var newUser = new UserEntity
        {
            Id = "4",
            FirstName = "New",
            LastName = "User",
            Email = "new@test.com",
            UserName = "new@test.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        await _userRepository.AddAsync(newUser);
        await _userRepository.SaveChangesAsync();

        // Assert
        var savedUser = await _userRepository.GetByIdAsync("4");
        savedUser.Should().NotBeNull();
        savedUser!.Email.Should().Be("new@test.com");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUserInDatabase()
    {
        // Arrange
        var user = await _userRepository.GetByIdAsync("1");
        user!.FirstName = "UpdatedJohn";
        user.UpdatedAt = DateTime.UtcNow;

        // Act
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        // Assert
        var updatedUser = await _userRepository.GetByIdAsync("1");
        updatedUser.Should().NotBeNull();
        updatedUser!.FirstName.Should().Be("UpdatedJohn");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveUserFromDatabase()
    {
        // Arrange
        var user = await _userRepository.GetByIdAsync("3");

        // Act
        _userRepository.Delete(user!);
        await _userRepository.SaveChangesAsync();

        // Assert
        var deletedUser = await _userRepository.GetByIdAsync("3");
        deletedUser.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}