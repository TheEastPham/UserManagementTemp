using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Base.UserManagement.EFCore.Data;
using Base.UserManagement.Domain.DTOs.User;
using Base.UserManagement.Domain.DTOs.Role;
using Base.UserManagement.Domain.DTOs.Auth;
using Base.UserManagement.Domain.DTOs.Account;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Base.UserManagement.IntegrationTests;

public class UsersControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public UsersControllerIntegrationTests(WebApplicationFactory<Program> factory)
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
                    options.UseInMemoryDatabase("InMemoryDbForUsersTest");
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

    private async Task<string> GetAuthTokenAsync()
    {
        // Register and login to get auth token
        var registerRequest = new RegisterRequest(
            Email: "admin@test.com",
            Password: "Admin123!@#",
            ConfirmPassword: "Admin123!@#",
            FirstName: "Admin",
            LastName: "User",
            PhoneNumber: "1234567890",
            Language: "en-US"
        );

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest("admin@test.com", "Admin123!@#");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<LoginResponse>(loginContent, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        return loginResult!.AccessToken!;
    }

    [Fact]
    public async Task GetUsers_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUsers_WithAuth_ShouldReturnUsers()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var users = JsonSerializer.Deserialize<List<UserDto>>(content, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        users.Should().NotBeNull();
        users.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetUserById_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // First, get the user ID from the users list
        var usersResponse = await _client.GetAsync("/api/users");
        var usersContent = await usersResponse.Content.ReadAsStringAsync();
        var users = JsonSerializer.Deserialize<List<UserDto>>(usersContent, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        var userId = users!.First().Id;

        // Act
        var response = await _client.GetAsync($"/api/users/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<UserDto>(content, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        user.Should().NotBeNull();
        user!.Id.Should().Be(userId);
    }

    [Fact]
    public async Task GetUserById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var invalidId = "invalid-user-id";

        // Act
        var response = await _client.GetAsync($"/api/users/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateUser_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateUserRequest(
            Email: "newuser@test.com",
            FirstName: "New",
            LastName: "User",
            Password: "9876543210",
            Language: "vi-VN"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<LoginResponse>(content, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CreateUser_WithExistingEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateUserRequest(
            Email: "admin@test.com", // This email already exists from auth setup
            FirstName: "Duplicate",
            LastName: "User",
            Password: "9876543210",
            Language: "vi-VN"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SearchUsers_WithQuery_ShouldReturnMatchingUsers()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/users/search?query=admin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var users = JsonSerializer.Deserialize<List<UserDto>>(content, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        users.Should().NotBeNull();
        users.Should().HaveCountGreaterThan(0);
        users!.Should().Contain(u => u.Email.Contains("admin", StringComparison.OrdinalIgnoreCase));
    }
}
