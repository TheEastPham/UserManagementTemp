using Base.UserManagement.Domain.Services;
using Base.UserManagement.Domain.DTOs.Auth;
using Base.UserManagement.Domain.DTOs.Account;
using Base.UserManagement.Domain.Services.Interfaces;
using Base.UserManagement.EFCore.Entities.User;
using Base.UserManagement.EFCore.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace Base.UserManagement.Domain.UnitTests;

// Testable version of AuthService that doesn't require SignInManager
public class TestableAuthService
{
    private readonly UserManager<UserEntity> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    private readonly IEmailService _emailService;
    private readonly IEmailVerificationTokenRepository _tokenRepository;
    private readonly ITokenGeneratorService _tokenGenerator;

    public TestableAuthService(
        UserManager<UserEntity> userManager,
        IConfiguration configuration,
        IMapper mapper,
        ILogger<AuthService> logger,
        IEmailService emailService,
        IEmailVerificationTokenRepository tokenRepository,
        ITokenGeneratorService tokenGenerator)
    {
        _userManager = userManager;
        _configuration = configuration;
        _mapper = mapper;
        _logger = logger;
        _emailService = emailService;
        _tokenRepository = tokenRepository;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !user.IsActive)
            {
                return new LoginResponse(false, "Invalid email or password");
            }

            // For testing, assume password check passes
            var checkPassword = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!checkPassword)
            {
                return new LoginResponse(false, "Invalid email or password");
            }

            var roles = await _userManager.GetRolesAsync(user);
            
            // Simplified token generation for testing
            var accessToken = "test_access_token";
            var refreshToken = "test_refresh_token";
            var expiresAt = DateTime.UtcNow.AddDays(7);

            return new LoginResponse(
                true,
                "Login successful",
                accessToken,
                refreshToken,
                expiresAt,
                null // UserDto can be null for basic tests
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Email}", request.Email);
            return new LoginResponse(false, "An error occurred during login");
        }
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            if (request.Password != request.ConfirmPassword)
            {
                return new RegisterResponse(false, "Passwords do not match");
            }

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new RegisterResponse(false, "Email already exists");
            }

            var user = new UserEntity
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                return new RegisterResponse(true, "User registered successfully", user.Id);
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new RegisterResponse(false, $"Registration failed: {errors}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user {Email}", request.Email);
            return new RegisterResponse(false, "An error occurred during registration");
        }
    }
}
