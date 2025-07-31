using Base.UserManagement.Domain.Services;
using Base.UserManagement.Domain.DTOs.User;
using Base.UserManagement.Domain.Models;
using Base.UserManagement.EFCore.Entities.User;
using Base.UserManagement.EFCore.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using AutoMapper;

namespace Base.UserManagement.Domain.UnitTests.TestCase;

public class UserServiceTests
{
    private readonly Mock<UserManager<UserEntity>> _mockUserManager;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        // Setup UserManager mock
        var mockUserStore = new Mock<IUserStore<UserEntity>>();
        _mockUserManager = new Mock<UserManager<UserEntity>>(
            mockUserStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _mockUserRepository = new Mock<IUserRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<UserService>>();

        _userService = new UserService(
            _mockUserRepository.Object,
            _mockUserManager.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateUserAsync_WithNullEmail_ShouldThrowArgumentException()
    {
        var createRequest = new CreateUserRequest(
            Email: null!,
            Password: "Password123!",
            FirstName: "Test",
            LastName: "User",
            Language: "en-US"
        );
        await Assert.ThrowsAsync<ArgumentException>(async () => await _userService.CreateUserAsync(createRequest));
    }

    [Fact]
    public async Task CreateUserAsync_WithEmptyPassword_ShouldThrowArgumentException()
    {
        var createRequest = new CreateUserRequest(
            Email: "test@example.com",
            Password: "",
            FirstName: "Test",
            LastName: "User",
            Language: "en-US"
        );
        await Assert.ThrowsAsync<ArgumentException>(async () => await _userService.CreateUserAsync(createRequest));
    }

    [Fact]
    public async Task CreateUserAsync_WithNullFirstName_ShouldThrowArgumentException()
    {
        var createRequest = new CreateUserRequest(
            Email: "test@example.com",
            Password: "Password123!",
            FirstName: null!,
            LastName: "User",
            Language: "en-US"
        );
        await Assert.ThrowsAsync<ArgumentException>(async () => await _userService.CreateUserAsync(createRequest));
    }

    [Fact]
    public async Task GetUserByIdAsync_RepositoryThrowsException_ShouldPropagateException()
    {
        var userId = "1";
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId)).ThrowsAsync(new Exception("Database error"));
        await Assert.ThrowsAsync<Exception>(async () => await _userService.GetUserByIdAsync(userId));
    }

    [Fact]
    public async Task CreateUserAsync_MapperThrowsException_ShouldPropagateException()
    {
        var createRequest = new CreateUserRequest(
            Email: "test@example.com",
            Password: "Password123!",
            FirstName: "Test",
            LastName: "User",
            Language: "en-US"
        );
        _mockMapper.Setup(x => x.Map<UserEntity>(createRequest)).Throws(new InvalidOperationException("Mapping error"));
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await _userService.CreateUserAsync(createRequest));
    }

    [Fact]
    public async Task GetUsersAsync_ShouldReturnMappedUsers()
    {
        // Arrange
        var users = new List<UserEntity>
        {
            new UserEntity { Id = "1", FirstName = "John", LastName = "Doe", Email = "john@test.com" },
            new UserEntity { Id = "2", FirstName = "Jane", LastName = "Smith", Email = "jane@test.com" }
        };

        var userDtos = new List<UserDto>
        {
            new UserDto("1", "john@test.com", "John", "Doe", "John Doe", null, null, null, "en", DateTime.UtcNow, DateTime.UtcNow, true, null, new List<string>()),
            new UserDto("2", "jane@test.com", "Jane", "Smith", "Jane Smith", null, null, null, "en", DateTime.UtcNow, DateTime.UtcNow, true, null, new List<string>())
        };

        var request = new GetUsersRequest(1, 20, null, null);

        _mockUserRepository.Setup(x => x.GetAllAsync(1, 20, null))
            .ReturnsAsync(users);
        _mockUserRepository.Setup(x => x.CountAsync(null))
            .ReturnsAsync(2);
        
        // Mock GetRolesAsync for each user
        _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<UserEntity>()))
            .ReturnsAsync(new List<string> { "Member" });
            
        // Mock mapper calls
        _mockMapper.Setup(x => x.Map<User>(It.IsAny<UserEntity>()))
            .Returns((UserEntity entity) => new User 
            { 
                Id = entity.Id, 
                Email = entity.Email, 
                FirstName = entity.FirstName, 
                LastName = entity.LastName,
                Roles = new List<string> { "Member" }
            });
            
        _mockMapper.Setup(x => x.Map<UserDto>(It.IsAny<User>()))
            .Returns((User user) => new UserDto(
                user.Id, user.Email!, user.FirstName!, user.LastName!, 
                $"{user.FirstName} {user.LastName}", null, null, null, "en", 
                DateTime.UtcNow, DateTime.UtcNow, true, null, user.Roles ?? new List<string>()));

        // Act
        var result = await _userService.GetUsersAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Users.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var userId = "1";
        var userEntity = new UserEntity { Id = userId, FirstName = "John", LastName = "Doe", Email = "john@test.com" };
        var user = new User
        {
            Id = userId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            Roles = new List<string> { "Member" }
        };
        var userDto = new UserDto(userId, "john@test.com", "John", "Doe", "John Doe", null, null, null, "en", DateTime.UtcNow, DateTime.UtcNow, true, null, new List<string>());

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(userEntity);
        var roles = new List<string> { "Admin", "Member" };
        _mockUserManager.Setup(x => x.GetRolesAsync(userEntity))
            .ReturnsAsync(roles);
        _mockMapper.Setup(x => x.Map<User>(userEntity))
            .Returns(user);
        _mockMapper.Setup(x => x.Map<UserDto>(user))
            .Returns(userDto);
        
        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(userDto);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var userId = "invalid";
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((UserEntity?)null);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateUserAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var createRequest = new CreateUserRequest(
            Email: "test@example.com",
            Password: "Password123!",
            FirstName: "Test",
            LastName: "User",
            Language: "en-US"
        );

        var user = new UserEntity
        {
            Email = createRequest.Email,
            UserName = createRequest.Email,
            FirstName = createRequest.FirstName,
            LastName = createRequest.LastName,
            Language = createRequest.Language
        };

        var userDto = new UserDto("1", createRequest.Email, createRequest.FirstName, createRequest.LastName, "Test User", null, null, null, createRequest.Language, DateTime.UtcNow, DateTime.UtcNow, true, null, new List<string>());

        _mockUserManager.Setup(x => x.FindByEmailAsync(createRequest.Email))
            .ReturnsAsync((UserEntity?)null);
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<UserEntity>(), createRequest.Password))
            .ReturnsAsync(IdentityResult.Success);
        _mockMapper.Setup(x => x.Map<UserEntity>(createRequest))
            .Returns(user);
        _mockMapper.Setup(x => x.Map<UserDto>(It.IsAny<UserEntity>()))
            .Returns(userDto);

        // Act
        var result = await _userService.CreateUserAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(userDto);
    }

    [Fact]
    public async Task CreateUserAsync_WithExistingEmail_ShouldReturnFailure()
    {
        // Arrange
        var createRequest = new CreateUserRequest(
            Email: "existing@example.com",
            Password: "Password123!",
            FirstName: "Test",
            LastName: "User",
            Language: "en-US"
        );

        var existingUser = new UserEntity { Email = createRequest.Email };

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<UserEntity>(), createRequest.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email already exists" }));

        // Act & Assert
        var act = async () => await _userService.CreateUserAsync(createRequest);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to create user: Email already exists");
    }
}