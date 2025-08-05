using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UserManagement.EFCore.Data;
using UserManagement.Domain.DTOs.Auth;
using UserManagement.Domain.DTOs.Account;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace UserManagement.IntegrationTests;

public class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real database context
                services.RemoveAll(typeof(DbContextOptions<UserManagementDbContext>));
                
                // Add In-Memory database for testing
                services.AddDbContext<UserManagementDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });

                // Build the service provider
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context
                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<UserManagementDbContext>();

                // Ensure the database is created
                db.Database.EnsureCreated();
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var registerRequest = new RegisterRequest(
            Email: "test@example.com",
            Password: "Test123!@#",
            ConfirmPassword: "Test123!@#",
            FirstName: "Test",
            LastName: "User",
            PhoneNumber: "1234567890",
            Language: "en-US"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<RegisterResponse>(responseContent, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });
        
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Register_WithMismatchedPasswords_ShouldReturnBadRequest()
    {
        // Arrange
        var registerRequest = new RegisterRequest(
            Email: "test2@example.com",
            Password: "Test123!@#",
            ConfirmPassword: "DifferentPassword!",
            FirstName: "Test",
            LastName: "User",
            PhoneNumber: "1234567890",
            Language: "en-US"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest("nonexistent@example.com", "WrongPassword!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RegisterAndLogin_WithValidData_ShouldReturnTokens()
    {
        // Arrange
        var email = "logintest@example.com";
        var password = "Test123!@#";
        
        var registerRequest = new RegisterRequest(
            Email: email,
            Password: password,
            ConfirmPassword: password,
            FirstName: "Login",
            LastName: "Test",
            PhoneNumber: "1234567890",
            Language: "en-US"
        );

        var loginRequest = new LoginRequest(email, password);

        // Act & Assert - Register
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act & Assert - Login
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<LoginResponse>(loginContent, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        loginResult.Should().NotBeNull();
        loginResult!.Success.Should().BeTrue();
        loginResult.AccessToken.Should().NotBeNull();
        loginResult.AccessToken!.Should().NotBeNullOrEmpty();
    }
}