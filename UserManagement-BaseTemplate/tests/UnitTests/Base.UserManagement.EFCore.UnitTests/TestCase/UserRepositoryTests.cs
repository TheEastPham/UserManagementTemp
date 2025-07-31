using Base.UserManagement.EFCore.Data;
using Base.UserManagement.EFCore.Entities.User;
using Base.UserManagement.EFCore.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Base.UserManagement.EFCore.UnitTests.TestCase;

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

    public void Dispose()
    {
        _context.Dispose();
    }
}