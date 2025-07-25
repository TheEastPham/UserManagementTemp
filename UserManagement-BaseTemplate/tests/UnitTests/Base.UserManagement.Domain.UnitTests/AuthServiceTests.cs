using Base.UserManagement.Domain.Services;
using Base.UserManagement.Domain.DTOs;
using Base.UserManagement.EFCore.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using FluentAssertions;

namespace Base.UserManagement.Domain.UnitTests;

public class AuthServiceTests
{
    private readonly Mock<UserManager<UserEntity>> _mockUserManager;
    private readonly Mock<SignInManager<UserEntity>> _mockSignInManager;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        // Setup UserManager mock
        var mockUserStore = new Mock<IUserStore<UserEntity>>();
        _mockUserManager = new Mock<UserManager<UserEntity>>(
            mockUserStore.Object, null, null, null, null, null, null, null, null);

        // Setup SignInManager mock
        var mockHttpContextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var mockClaimsFactory = new Mock<IUserClaimsPrincipalFactory<UserEntity>>();
        _mockSignInManager = new Mock<SignInManager<UserEntity>>(
            _mockUserManager.Object, mockHttpContextAccessor.Object, mockClaimsFactory.Object, null, null, null, null);

        // Setup Configuration mock
        _mockConfiguration = new Mock<IConfiguration>();
        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.Setup(x => x["SecretKey"]).Returns("ThisIsAVerySecretKeyThatIs32CharsLong!");
        jwtSection.Setup(x => x["Issuer"]).Returns("TestIssuer");
        jwtSection.Setup(x => x["Audience"]).Returns("TestAudience");
        jwtSection.Setup(x => x["ExpiryInDays"]).Returns("7");
        
        _mockConfiguration.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSection.Object);

        _authService = new AuthService(
            _mockUserManager.Object,
            _mockSignInManager.Object,
            _mockConfiguration.Object);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccessWithToken()
    {
        // Arrange
        var loginRequest = new LoginRequest("test@example.com", "Password123!");
        var user = new UserEntity 
        { 
            Id = "1", 
            Email = "test@example.com", 
            UserName = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginRequest.Email))
            .ReturnsAsync(user);
        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, loginRequest.Password, false))
            .ReturnsAsync(SignInResult.Success);
        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Member" });

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Token.Should().NotBeNull();
        result.Token!.AccessToken.Should().NotBeNullOrEmpty();
        result.Token.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ShouldReturnFailure()
    {
        // Arrange
        var loginRequest = new LoginRequest("invalid@example.com", "Password123!");

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginRequest.Email))
            .ReturnsAsync((UserEntity?)null);

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid email or password");
        result.Token.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldReturnFailure()
    {
        // Arrange
        var loginRequest = new LoginRequest("test@example.com", "WrongPassword!");
        var user = new UserEntity 
        { 
            Id = "1", 
            Email = "test@example.com", 
            UserName = "test@example.com"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginRequest.Email))
            .ReturnsAsync(user);
        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, loginRequest.Password, false))
            .ReturnsAsync(SignInResult.Failed);

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid email or password");
        result.Token.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var registerRequest = new RegisterRequest(
            Email: "newuser@example.com",
            Password: "Password123!",
            ConfirmPassword: "Password123!",
            FirstName: "New",
            LastName: "User",
            PhoneNumber: "1234567890",
            Language: "en-US"
        );

        _mockUserManager.Setup(x => x.FindByEmailAsync(registerRequest.Email))
            .ReturnsAsync((UserEntity?)null);
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<UserEntity>(), registerRequest.Password))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.RegisterAsync(registerRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("User registered successfully");
    }

    [Fact]
    public async Task RegisterAsync_WithMismatchedPasswords_ShouldReturnFailure()
    {
        // Arrange
        var registerRequest = new RegisterRequest(
            Email: "newuser@example.com",
            Password: "Password123!",
            ConfirmPassword: "DifferentPassword!",
            FirstName: "New",
            LastName: "User",
            PhoneNumber: "1234567890",
            Language: "en-US"
        );

        // Act
        var result = await _authService.RegisterAsync(registerRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Passwords do not match");
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldReturnFailure()
    {
        // Arrange
        var registerRequest = new RegisterRequest(
            Email: "existing@example.com",
            Password: "Password123!",
            ConfirmPassword: "Password123!",
            FirstName: "New",
            LastName: "User",
            PhoneNumber: "1234567890",
            Language: "en-US"
        );

        var existingUser = new UserEntity { Email = registerRequest.Email };

        _mockUserManager.Setup(x => x.FindByEmailAsync(registerRequest.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _authService.RegisterAsync(registerRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Email already exists");
    }
}
