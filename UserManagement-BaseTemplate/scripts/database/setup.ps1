# ==============================================================================
# Database Migration and Seeding Script for base Platform
# ==============================================================================

param(
    [Parameter(Mandatory=$true)]
    [string]$ConnectionString,
    
    [string]$Environment = "Production",
    [switch]$SeedData,
    [switch]$ForceRecreate
)

Write-Host "🗄️ Database Migration Script for base Platform" -ForegroundColor Green

# ==============================================================================
# 1. Prerequisites Check
# ==============================================================================
Write-Host "📋 Checking Prerequisites..." -ForegroundColor Yellow

# Check if EF Core tools are installed
try {
    $efVersion = dotnet ef --version
    Write-Host "✅ Entity Framework Core tools found: $efVersion" -ForegroundColor Green
} catch {
    Write-Error "❌ Entity Framework Core tools not found. Install with: dotnet tool install --global dotnet-ef"
    exit 1
}

# Test database connection
try {
    $testQuery = "SELECT 1"
    Invoke-Sqlcmd -Query $testQuery -ConnectionString $ConnectionString -QueryTimeout 10
    Write-Host "✅ Database connection successful" -ForegroundColor Green
} catch {
    Write-Error "❌ Database connection failed: $($_.Exception.Message)"
    exit 1
}

# ==============================================================================
# 2. Navigate to project directory
# ==============================================================================
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
$rootPath = Split-Path -Parent $scriptPath
$userMgmtPath = Join-Path $rootPath "src\Backend\UserManagement"

if (!(Test-Path $userMgmtPath)) {
    Write-Error "❌ User Management project not found at: $userMgmtPath"
    exit 1
}

Set-Location $userMgmtPath

# ==============================================================================
# 3. Database Operations
# ==============================================================================
if ($ForceRecreate) {
    Write-Host "🗑️ Dropping and recreating database..." -ForegroundColor Yellow
    
    $dbName = ($ConnectionString -split "Database=")[1] -split ";")[0]
    $masterConnectionString = $ConnectionString -replace "Database=$dbName", "Database=master"
    
    $dropDbQuery = @"
IF EXISTS (SELECT name FROM sys.databases WHERE name = '$dbName')
BEGIN
    ALTER DATABASE [$dbName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [$dbName];
END
CREATE DATABASE [$dbName];
"@
    
    try {
        Invoke-Sqlcmd -Query $dropDbQuery -ConnectionString $masterConnectionString
        Write-Host "✅ Database recreated" -ForegroundColor Green
    } catch {
        Write-Error "❌ Database recreation failed: $($_.Exception.Message)"
        exit 1
    }
}

# ==============================================================================
# 4. Run Migrations
# ==============================================================================
Write-Host "🔄 Running Entity Framework migrations..." -ForegroundColor Yellow

# Set environment variable for connection string
$env:ConnectionStrings__DefaultConnection = $ConnectionString

try {
    # Check if migrations exist
    $migrationsPath = Join-Path $userMgmtPath "Migrations"
    if (!(Test-Path $migrationsPath)) {
        Write-Host "📝 No migrations found, creating initial migration..." -ForegroundColor Cyan
        dotnet ef migrations add InitialCreate
        if ($LASTEXITCODE -ne 0) {
            Write-Error "❌ Failed to create initial migration"
            exit 1
        }
    }
    
    # Apply migrations
    Write-Host "⏳ Applying migrations to database..." -ForegroundColor Cyan
    dotnet ef database update --connection $ConnectionString
    if ($LASTEXITCODE -ne 0) {
        Write-Error "❌ Migration failed"
        exit 1
    }
    
    Write-Host "✅ Migrations applied successfully" -ForegroundColor Green
    
} catch {
    Write-Error "❌ Migration error: $($_.Exception.Message)"
    exit 1
}

# ==============================================================================
# 5. Seed Data (if requested)
# ==============================================================================
if ($SeedData) {
    Write-Host "🌱 Seeding initial data..." -ForegroundColor Yellow
    
    $seedDataQuery = @"
-- Check if data already exists
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Administrator')
BEGIN
    -- Insert default roles
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES 
        (NEWID(), 'Administrator', 'ADMINISTRATOR', NEWID()),
        (NEWID(), 'User', 'USER', NEWID()),
        (NEWID(), 'Moderator', 'MODERATOR', NEWID());
        
    PRINT 'Default roles created';
END
ELSE
BEGIN
    PRINT 'Roles already exist, skipping role seeding';
END

-- Check if admin user exists
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'admin@base.com')
BEGIN
    DECLARE @adminId UNIQUEIDENTIFIER = NEWID();
    DECLARE @adminRoleId UNIQUEIDENTIFIER = (SELECT Id FROM AspNetRoles WHERE Name = 'Administrator');
    
    -- Insert admin user (password will need to be set via registration)
    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
    VALUES (
        @adminId,
        'admin@base.com',
        'ADMIN@base.COM',
        'admin@base.com',
        'ADMIN@base.COM',
        1,
        NEWID(),
        NEWID(),
        0,
        0,
        1,
        0
    );
    
    -- Assign admin role
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES (@adminId, @adminRoleId);
    
    PRINT 'Admin user created (email: admin@base.com)';
    PRINT 'Please set password via the registration/reset password process';
END
ELSE
BEGIN
    PRINT 'Admin user already exists, skipping user seeding';
END

-- Create sample data for development environment
IF '$Environment' = 'Development'
BEGIN
    IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'user@base.com')
    BEGIN
        DECLARE @userId UNIQUEIDENTIFIER = NEWID();
        DECLARE @userRoleId UNIQUEIDENTIFIER = (SELECT Id FROM AspNetRoles WHERE Name = 'User');
        
        INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
        VALUES (
            @userId,
            'user@base.com',
            'USER@base.COM',
            'user@base.com',
            'USER@base.COM',
            1,
            NEWID(),
            NEWID(),
            0,
            0,
            1,
            0
        );
        
        INSERT INTO AspNetUserRoles (UserId, RoleId)
        VALUES (@userId, @userRoleId);
        
        PRINT 'Development user created (email: user@base.com)';
    END
END
"@
    
    try {
        Invoke-Sqlcmd -Query $seedDataQuery -ConnectionString $ConnectionString
        Write-Host "✅ Data seeding completed" -ForegroundColor Green
    } catch {
        Write-Warning "⚠️ Data seeding encountered errors: $($_.Exception.Message)"
    }
}

# ==============================================================================
# 6. Database Verification
# ==============================================================================
Write-Host "🔍 Verifying database setup..." -ForegroundColor Yellow

$verificationQueries = @(
    @{
        Name = "Tables"
        Query = "SELECT COUNT(*) as TableCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'"
        ExpectedMin = 5
    },
    @{
        Name = "Identity Tables"
        Query = "SELECT COUNT(*) as IdentityTableCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE 'AspNet%'"
        ExpectedMin = 7
    },
    @{
        Name = "Roles"
        Query = "SELECT COUNT(*) as RoleCount FROM AspNetRoles"
        ExpectedMin = 1
    }
)

foreach ($verification in $verificationQueries) {
    try {
        $result = Invoke-Sqlcmd -Query $verification.Query -ConnectionString $ConnectionString
        $count = $result[0]
        
        if ($count -ge $verification.ExpectedMin) {
            Write-Host "✅ $($verification.Name): $count" -ForegroundColor Green
        } else {
            Write-Warning "⚠️ $($verification.Name): $count (expected at least $($verification.ExpectedMin))"
        }
    } catch {
        Write-Warning "⚠️ Could not verify $($verification.Name): $($_.Exception.Message)"
    }
}

# ==============================================================================
# 7. Create Database Backup Script
# ==============================================================================
Write-Host "💾 Creating backup script..." -ForegroundColor Yellow

$dbName = ($ConnectionString -split "Database=")[1] -split ";")[0]
$backupScript = @"
-- Database Backup Script for $dbName
-- Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

DECLARE @BackupPath NVARCHAR(256) = 'C:\base\backups\$dbName' + '_' + FORMAT(GETDATE(), 'yyyyMMdd_HHmmss') + '.bak';

BACKUP DATABASE [$dbName] 
TO DISK = @BackupPath
WITH FORMAT, 
     INIT,
     COMPRESSION,
     CHECKSUM,
     STATS = 10;

PRINT 'Backup completed: ' + @BackupPath;

-- Verify backup
RESTORE VERIFYONLY FROM DISK = @BackupPath;
PRINT 'Backup verification completed successfully';
"@

$backupScriptPath = Join-Path $rootPath "scripts\backup-database.sql"
$backupScript | Out-File -FilePath $backupScriptPath -Encoding UTF8
Write-Host "✅ Backup script created: $backupScriptPath" -ForegroundColor Green

# ==============================================================================
# 8. Summary
# ==============================================================================
Write-Host "`n🎉 Database setup completed!" -ForegroundColor Green
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host "📍 Database Information:" -ForegroundColor Yellow
Write-Host "   • Database: $dbName" -ForegroundColor White
Write-Host "   • Environment: $Environment" -ForegroundColor White
Write-Host "   • Migrations: Applied" -ForegroundColor White

if ($SeedData) {
    Write-Host "   • Seed Data: Applied" -ForegroundColor White
    Write-Host "`n📍 Default Accounts:" -ForegroundColor Yellow
    Write-Host "   • Admin: admin@base.com (password must be set)" -ForegroundColor White
    if ($Environment -eq "Development") {
        Write-Host "   • User: user@base.com (password must be set)" -ForegroundColor White
    }
}

Write-Host "`n📍 Next Steps:" -ForegroundColor Yellow
Write-Host "   1. Set passwords for default accounts" -ForegroundColor White
Write-Host "   2. Configure regular database backups" -ForegroundColor White
Write-Host "   3. Set up database monitoring" -ForegroundColor White
Write-Host "   4. Review and adjust database permissions" -ForegroundColor White

Write-Host "`n📍 Useful Commands:" -ForegroundColor Yellow
Write-Host "   • Backup: sqlcmd -S localhost -i $backupScriptPath" -ForegroundColor White
Write-Host "   • Check migrations: dotnet ef migrations list" -ForegroundColor White
Write-Host "   • Add migration: dotnet ef migrations add <MigrationName>" -ForegroundColor White

Write-Host "`n===========================================" -ForegroundColor Cyan

# Return to original directory
Set-Location $scriptPath
