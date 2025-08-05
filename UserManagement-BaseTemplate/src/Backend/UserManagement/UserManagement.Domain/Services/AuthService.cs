using AutoMapper;
using UserManagement.Domain.DTOs.Auth;
using UserManagement.Domain.DTOs.Account;
using UserManagement.Domain.DTOs.User;
using UserManagement.Domain.Models;
using UserManagement.Domain.Services.Interfaces;
using UserManagement.EFCore.Entities.User;
using UserManagement.EFCore.Entities.Security;
using UserManagement.EFCore.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace UserManagement.Domain.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<UserEntity> _userManager;
    private readonly SignInManager<UserEntity> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    private readonly IEmailService _emailService;
    private readonly IEmailVerificationTokenRepository _tokenRepository;
    private readonly ITokenGeneratorService _tokenGenerator;

    public AuthService(
        UserManager<UserEntity> userManager,
        SignInManager<UserEntity> signInManager,
        IConfiguration configuration,
        IMapper mapper,
        ILogger<AuthService> logger,
        IEmailService emailService,
        IEmailVerificationTokenRepository tokenRepository,
        ITokenGeneratorService tokenGenerator)
    {
        _userManager = userManager;
        _signInManager = signInManager;
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

            // Check if email is confirmed
            if (!user.EmailConfirmed)
            {
                return new LoginResponse(false, "Email not verified. Please check your email and verify your account.");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                return new LoginResponse(false, "Invalid email or password");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var tokens = await GenerateTokensAsync(user, roles);

            // Update refresh token in database
            user.RefreshToken = tokens.RefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var userModel = _mapper.Map<User>(user);
            userModel.Roles = roles.ToList();
            var userDto = _mapper.Map<UserDto>(userModel);

            return new LoginResponse(
                true,
                "Login successful",
                tokens.AccessToken,
                tokens.RefreshToken,
                tokens.ExpiresAt,
                userDto
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Email}", request.Email);
            return new LoginResponse(false, "An error occurred during login");
        }
    }

    public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        try
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new SecurityTokenException("Invalid refresh token");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var tokens = await GenerateTokensAsync(user, roles);

            // Update refresh token
            user.RefreshToken = tokens.RefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return tokens;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            throw;
        }
    }

    public async Task<bool> LogoutAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user {UserId}", userId);
            return false;
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
                EmailConfirmed = false, // Set to false initially
                FirstName = request.FirstName,
                LastName = request.LastName,
                Language = request.Language,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                // Assign default Member role
                await _userManager.AddToRoleAsync(user, "Member");
                
                // Generate verification token and send email
                await GenerateAndSendVerificationEmailAsync(user);
                
                return new RegisterResponse(true, "Registration successful. Please check your email to verify your account.", user.Id, true);
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

    private Task<TokenResponse> GenerateTokensAsync(UserEntity user, IList<string> roles)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expiryInDays = int.Parse(jwtSettings["ExpiryInDays"] ?? "7");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var expiresAt = DateTime.UtcNow.AddDays(expiryInDays);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = Guid.NewGuid().ToString();

        return Task.FromResult(new TokenResponse(accessToken, refreshToken, expiresAt));
    }

    public async Task<VerifyEmailResponse> VerifyEmailAsync(VerifyEmailRequest request)
    {
        try
        {
            var token = await _tokenRepository.GetByTokenAsync(request.Token);
            if (token == null)
            {
                return new VerifyEmailResponse(false, "Invalid or expired verification token");
            }

            if (token.Email != request.Email)
            {
                return new VerifyEmailResponse(false, "Email does not match the token");
            }

            var user = await _userManager.FindByIdAsync(token.UserId);
            if (user == null)
            {
                return new VerifyEmailResponse(false, "User not found");
            }

            // Update user email confirmed status
            user.EmailConfirmed = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Mark token as used
            await _tokenRepository.MarkAsUsedAsync(token.Id);

            // Send welcome email
            await _emailService.SendWelcomeEmailAsync(user.Email!, user.FirstName ?? "User");

            _logger.LogInformation("Email verified successfully for user {UserId}", user.Id);
            return new VerifyEmailResponse(true, "Email verified successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying email for token {Token}", request.Token);
            return new VerifyEmailResponse(false, "An error occurred while verifying email");
        }
    }

    public async Task<ResendVerificationResponse> ResendVerificationEmailAsync(ResendVerificationRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return new ResendVerificationResponse(false, "User not found");
            }

            if (user.EmailConfirmed)
            {
                return new ResendVerificationResponse(false, "Email is already verified");
            }

            // Delete existing tokens for this user
            await _tokenRepository.DeleteByUserIdAsync(user.Id);

            // Generate and send new verification email
            await GenerateAndSendVerificationEmailAsync(user);

            return new ResendVerificationResponse(true, "Verification email sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending verification email to {Email}", request.Email);
            return new ResendVerificationResponse(false, "An error occurred while sending verification email");
        }
    }

    private async Task GenerateAndSendVerificationEmailAsync(UserEntity user)
    {
        try
        {
            // Generate verification token
            var verificationToken = _tokenGenerator.GenerateVerificationToken(user.Email!);
            
            var tokenEntity = new EmailVerificationTokenEntity
            {
                UserId = user.Id,
                Token = verificationToken,
                Email = user.Email!,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            };

            await _tokenRepository.CreateAsync(tokenEntity);

            // Send verification email
            await _emailService.SendEmailVerificationAsync(user.Email!, user.FirstName ?? "User", verificationToken);

            _logger.LogInformation("Verification email sent to {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating and sending verification email for user {UserId}", user.Id);
            throw;
        }
    }
}
