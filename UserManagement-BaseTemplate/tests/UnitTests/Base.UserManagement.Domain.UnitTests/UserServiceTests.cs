using Base.UserManagement.Domain.Services;
using Base.UserManagement.Domain.DTOs;
using Base.UserManagement.EFCore.Entities;
using Base.UserManagement.EFCore.Repositories;
using Microsoft.AspNetCore.Identity;
using Moq;
using FluentAssertions;
using AutoMapper;

namespace Base.UserManagement.Domain.UnitTests;

public class UserServiceTests
{
    private readonly Mock<UserManager<UserEntity>> _mockUserManager;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        // Setup UserManager mock
        var mockStore = new Mock<IUserStore<UserEntity>>();
        _mockUserManager = new Mock<UserManager<UserEntity>>(
            mockStore.Object, null, null, null, null, null, null, null, null);

        _mockUserRepository = new Mock<IUserRepository>();
        _mockMapper = new Mock<IMapper>();

        _userService = new UserService(
            _mockUserManager.Object,
            _mockUserRepository.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task GetAllUsersAsync_ShouldReturnMappedUsers()
    {
        // Arrange
        var users = new List<UserEntity>
        {
            new UserEntity { Id = "1", FirstName = "John", LastName = "Doe", Email = "john@test.com" },
            new UserEntity { Id = "2", FirstName = "Jane", LastName = "Smith", Email = "jane@test.com" }
        };

        var userDtos = new List<UserDto>
        {
            new UserDto { Id = "1", FirstName = "John", LastName = "Doe", Email = "john@test.com" },
            new UserDto { Id = "2", FirstName = "Jane", LastName = "Smith", Email = "jane@test.com" }
        };

        _mockUserRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(users);
        _mockMapper.Setup(x => x.Map<IEnumerable<UserDto>>(users))
            .Returns(userDtos);

        // Act
        var result = await _userService.GetAllUsersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(userDtos);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var userId = "1";
        var user = new UserEntity { Id = userId, FirstName = "John", LastName = "Doe", Email = "john@test.com" };
        var userDto = new UserDto { Id = userId, FirstName = "John", LastName = "Doe", Email = "john@test.com" };

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
            FirstName: "Test",
            LastName: "User",
            PhoneNumber: "1234567890",
            Language: "en-US"
        );

        var user = new UserEntity
        {
            Email = createRequest.Email,
            UserName = createRequest.Email,
            FirstName = createRequest.FirstName,
            LastName = createRequest.LastName,
            PhoneNumber = createRequest.PhoneNumber,
            Language = createRequest.Language
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(createRequest.Email))
            .ReturnsAsync((UserEntity?)null);
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<UserEntity>()))
            .ReturnsAsync(IdentityResult.Success);
        _mockMapper.Setup(x => x.Map<UserEntity>(createRequest))
            .Returns(user);

        // Act
        var result = await _userService.CreateUserAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("User created successfully");
    }

    [Fact]
    public async Task CreateUserAsync_WithExistingEmail_ShouldReturnFailure()
    {
        // Arrange
        var createRequest = new CreateUserRequest(
            Email: "existing@example.com",
            FirstName: "Test",
            LastName: "User",
            PhoneNumber: "1234567890",
            Language: "en-US"
        );

        var existingUser = new UserEntity { Email = createRequest.Email };

        _mockUserManager.Setup(x => x.FindByEmailAsync(createRequest.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _userService.CreateUserAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Email already exists");
    }
}