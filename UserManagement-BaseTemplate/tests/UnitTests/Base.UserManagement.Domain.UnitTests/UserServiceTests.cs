using Base.UserManagement.Domain.Services;
using Base.UserManagement.Domain.DTOs.User;
using Base.UserManagement.Domain.DTOs.Role;
using Base.UserManagement.EFCore.Entities.User;
using Base.UserManagement.EFCore.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using AutoMapper;

namespace Base.UserManagement.Domain.UnitTests;

public class UserServiceTests
{
    private readonly Mock<UserManager<UserEntity>> _mockUserManager;
    private readonly Mock<RoleManager<RoleEntity>> _mockRoleManager;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        // Setup UserManager mock
        var mockUserStore = new Mock<IUserStore<UserEntity>>();
        _mockUserManager = new Mock<UserManager<UserEntity>>(
            mockUserStore.Object, null, null, null, null, null, null, null, null);

        // Setup RoleManager mock
        var mockRoleStore = new Mock<IRoleStore<RoleEntity>>();
        _mockRoleManager = new Mock<RoleManager<RoleEntity>>(
            mockRoleStore.Object, null, null, null, null);

        _mockUserRepository = new Mock<IUserRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<UserService>>();

        _userService = new UserService(
            _mockUserRepository.Object,
            _mockUserManager.Object,
            _mockRoleManager.Object,
            _mockMapper.Object,
            _mockLogger.Object);
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
        var response = new GetUsersResponse(userDtos, 2, 1, 20, 1);

        _mockUserRepository.Setup(x => x.GetAllAsync(1, 20, null))
            .ReturnsAsync(users);
        _mockUserRepository.Setup(x => x.CountAsync(null))
            .ReturnsAsync(2);
        _mockMapper.Setup(x => x.Map<IEnumerable<UserDto>>(users))
            .Returns(userDtos);

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
        var user = new UserEntity { Id = userId, FirstName = "John", LastName = "Doe", Email = "john@test.com" };
        var userDto = new UserDto(userId, "john@test.com", "John", "Doe", "John Doe", null, null, null, "en", DateTime.UtcNow, DateTime.UtcNow, true, null, new List<string>());

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
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