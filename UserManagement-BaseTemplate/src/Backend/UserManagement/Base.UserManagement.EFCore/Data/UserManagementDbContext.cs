using Base.UserManagement.EFCore.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Base.UserManagement.EFCore.Data;

public class UserManagementDbContext : IdentityDbContext<UserEntity, RoleEntity, string>
{
    public UserManagementDbContext(DbContextOptions<UserManagementDbContext> options) : base(options)
    {
    }

    public DbSet<UserProfileEntity> UserProfiles { get; set; }
    public DbSet<SecurityEventEntity> SecurityEvents { get; set; }
    public DbSet<SystemRoleEntity> SystemRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure UserEntity
        builder.Entity<UserEntity>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Avatar).HasMaxLength(500);
            entity.Property(e => e.TimeZone).HasMaxLength(50);
            entity.Property(e => e.Language).HasMaxLength(10);
            entity.Property(e => e.RefreshToken).HasMaxLength(500);
        });

        // Configure RoleEntity
        builder.Entity<RoleEntity>(entity =>
        {
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // Configure UserProfileEntity
        builder.Entity<UserProfileEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Bio).HasMaxLength(1000);
            entity.Property(e => e.Preferences).HasMaxLength(2000);
            
            // Relationship with UserEntity
            entity.HasOne(e => e.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<UserProfileEntity>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure SecurityEventEntity
        builder.Entity<SecurityEventEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.Timestamp);
            entity.Property(e => e.EventType).HasMaxLength(100);
            entity.Property(e => e.UserEmail).HasMaxLength(256);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.Severity).HasMaxLength(20);
            entity.Property(e => e.Details).HasMaxLength(4000);
        });

        // Configure SystemRoleEntity
        builder.Entity<SystemRoleEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(200);
        });

        // Seed default data
        SeedDefaultRoles(builder);
        SeedSystemRoles(builder);
    }

    private static void SeedDefaultRoles(ModelBuilder builder)
    {
        var roles = new[]
        {
            new RoleEntity
            {
                Id = "1",
                Name = "SystemAdmin",
                NormalizedName = "SYSTEMADMIN",
                Description = "System Administrator with full system access",
                CreatedAt = DateTime.UtcNow
            },
            new RoleEntity
            {
                Id = "2",
                Name = "ContentAdmin",
                NormalizedName = "CONTENTADMIN",
                Description = "Content Administrator with content management permissions",
                CreatedAt = DateTime.UtcNow
            },
            new RoleEntity
            {
                Id = "3",
                Name = "Member",
                NormalizedName = "MEMBER",
                Description = "Regular user with basic permissions",
                CreatedAt = DateTime.UtcNow
            }
        };

        builder.Entity<RoleEntity>().HasData(roles);
    }

    private static void SeedSystemRoles(ModelBuilder builder)
    {
        var systemRoles = new[]
        {
            new SystemRoleEntity
            {
                Id = 1,
                Name = "SystemAdmin",
                Description = "Full system access - can manage all aspects of the system",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new SystemRoleEntity
            {
                Id = 2,
                Name = "ContentAdmin",
                Description = "Content management access - can manage content and moderate users",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new SystemRoleEntity
            {
                Id = 3,
                Name = "Member",
                Description = "Regular user access - standard user permissions",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        builder.Entity<SystemRoleEntity>().HasData(systemRoles);
    }
}
