using UserManagement.Domain.Mappings;
using UserManagement.Domain.Services;
using UserManagement.Domain.Services.Interfaces;
using UserManagement.EFCore.Data;
using UserManagement.EFCore.Entities.User;
using UserManagement.EFCore.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UserManagement.Domain.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserManagementDomain(this IServiceCollection services, IConfiguration configuration)
    {
        // Add EFCore layer
        services.AddUserManagementEFCore(configuration);

        // Add Identity
        services.AddIdentity<UserEntity, RoleEntity>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 1;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;

            // SignIn settings
            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedPhoneNumber = false;
        }).AddEntityFrameworkStores<UserManagementDbContext>()
          .AddDefaultTokenProviders();

        // Add AutoMapper
        services.AddAutoMapper(typeof(UserMappingProfile));

        // Add Domain Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ITokenGeneratorService, TokenGeneratorService>();

        return services;
    }
}
