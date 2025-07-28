using Base.UserManagement.EFCore.Data;
using Base.UserManagement.EFCore.Repositories;
using Base.UserManagement.EFCore.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Base.UserManagement.EFCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserManagementEFCore(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<UserManagementDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Add Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISecurityEventRepository, SecurityEventRepository>();

        return services;
    }
}
