using Base.UserManagement.EFCore.Entities.User;
using Base.UserManagement.EFCore.Entities.Security;
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
    public DbSet<EmailVerificationTokenEntity> EmailVerificationTokens { get; set; }

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

        // Configure EmailVerificationTokenEntity
        builder.Entity<EmailVerificationTokenEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.ExpiresAt);
            entity.Property(e => e.Token).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            
            // Relationship with UserEntity
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure RoleEntity
        builder.Entity<RoleEntity>(entity =>
        {
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // Seed default data
        SeedDefaultRoles(builder);
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
}
