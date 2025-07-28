using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Base.UserManagement.EFCore.Data;

namespace Base.UserManagement.EFCore
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<UserManagementDbContext>
    {
        public UserManagementDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UserManagementDbContext>();
            
            // Use default connection string for design time
            var connectionString = "Server=localhost;Database=baseUserManagement;User Id=sa;Password=DevPassword123!;TrustServerCertificate=true;";
            optionsBuilder.UseSqlServer(connectionString);

            return new UserManagementDbContext(optionsBuilder.Options);
        }
    }
}
