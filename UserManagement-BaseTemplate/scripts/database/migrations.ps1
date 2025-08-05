# ========================================================================
# Simple Local Development Database Migration Script
# ========================================================================

param(
    [switch]$CreateInitialMigration,
    [switch]$AddMigration,
    [string]$MigrationName = "",
    [switch]$UpdateDatabase,
    [switch]$SeedData,
    [switch]$ResetDatabase,
    [switch]$ShowMigrations,
    [switch]$UseDockerSql,
    [switch]$Help
)

# Colors for output
function Write-Info { param([string]$Message) Write-Host $Message -ForegroundColor Cyan }
function Write-Success { param([string]$Message) Write-Host $Message -ForegroundColor Green }
function Write-Warning { param([string]$Message) Write-Host $Message -ForegroundColor Yellow }
function Write-Error { param([string]$Message) Write-Host $Message -ForegroundColor Red }

if ($Help) {
    Write-Info "ðŸ—„ï¸ Local Development Database Migration Tool"
    Write-Info "============================================="
    Write-Info ""
    Write-Info "USAGE:"
    Write-Info "  .\setup-local-migrations-simple.ps1 [OPTIONS]"
    Write-Info ""
    Write-Info "OPTIONS:"
    Write-Info "  -CreateInitialMigration    Create the initial migration"
    Write-Info "  -AddMigration             Add a new migration"
    Write-Info "  -MigrationName [name]     Name for the new migration"
    Write-Info "  -UpdateDatabase           Apply migrations to database"
    Write-Info "  -SeedData                 Seed database with test data"
    Write-Info "  -ResetDatabase            Drop and recreate database"
    Write-Info "  -ShowMigrations           List all migrations"
    Write-Info "  -UseDockerSql             Use Docker SQL Server instead of LocalDB"
    Write-Info "  -Help                     Show this help message"
    exit 0
}

Write-Info "ðŸ—„ï¸ Local Development Database Migration Tool"
Write-Info "============================================="

# Navigate to UserManagement project
$projectPath = "src/Backend/UserManagement/UserManagement.API"
if (!(Test-Path $projectPath)) {
    Write-Error "âŒ UserManagement project not found at: $projectPath"
    exit 1
}

Set-Location $projectPath

# Set connection string based on SQL type
if ($UseDockerSql) {
    Write-Info "ðŸ³ Using Docker SQL Server 2022"
    $connectionString = "Server=localhost,1433;Database=baseUserManagement;User Id=sa;Password=DevPassword123!;TrustServerCertificate=true;"
} else {
    Write-Info "ðŸ—„ï¸ Using LocalDB"
    $connectionString = "Server=(localdb)\mssqllocaldb;Database=baseUserManagement;Trusted_Connection=true;MultipleActiveResultSets=true"
}

Write-Info "Connection String: $connectionString"

# Set environment variable
$env:ConnectionStrings__DefaultConnection = $connectionString

# Check if EF Core tools are installed
try {
    $efVersion = dotnet ef --version
    Write-Success "âœ… Entity Framework Core tools found: $efVersion"
} catch {
    Write-Error "âŒ Entity Framework Core tools not found. Install with: dotnet tool install --global dotnet-ef"
    exit 1
}

# Reset Database
if ($ResetDatabase) {
    Write-Warning "ðŸ—‘ï¸ Resetting database..."
    try {
        dotnet ef database drop --force
        Write-Success "âœ… Database dropped"
    } catch {
        Write-Warning "âš ï¸ Database drop failed or database doesn't exist"
    }
}

# Create Initial Migration
if ($CreateInitialMigration) {
    Write-Info "ðŸ“ Creating initial migration..."
    
    # Check if migrations already exist
    if (Test-Path "Migrations") {
        Write-Warning "âš ï¸ Migrations folder already exists. Skipping initial migration."
    } else {
        try {
            dotnet ef migrations add InitialCreate
            if ($LASTEXITCODE -eq 0) {
                Write-Success "âœ… Initial migration created successfully"
            } else {
                Write-Error "âŒ Failed to create initial migration"
                exit 1
            }
        } catch {
            Write-Error "âŒ Error creating initial migration: $($_.Exception.Message)"
            exit 1
        }
    }
}

# Add Migration
if ($AddMigration) {
    if ([string]::IsNullOrWhiteSpace($MigrationName)) {
        Write-Error "âŒ Migration name is required when using -AddMigration"
        exit 1
    }
    
    Write-Info "ðŸ“ Adding migration: $MigrationName"
    try {
        dotnet ef migrations add $MigrationName
        if ($LASTEXITCODE -eq 0) {
            Write-Success "âœ… Migration '$MigrationName' added successfully"
        } else {
            Write-Error "âŒ Failed to add migration '$MigrationName'"
            exit 1
        }
    } catch {
        Write-Error "âŒ Error adding migration: $($_.Exception.Message)"
        exit 1
    }
}

# Update Database
if ($UpdateDatabase) {
    Write-Info "ðŸ”„ Updating database..."
    try {
        dotnet ef database update --connection $connectionString
        if ($LASTEXITCODE -eq 0) {
            Write-Success "âœ… Database updated successfully"
        } else {
            Write-Error "âŒ Failed to update database"
            exit 1
        }
    } catch {
        Write-Error "âŒ Error updating database: $($_.Exception.Message)"
        exit 1
    }
}

# Show Migrations
if ($ShowMigrations) {
    Write-Info "ðŸ“‹ Showing migrations..."
    try {
        dotnet ef migrations list --connection $connectionString
    } catch {
        Write-Error "âŒ Error showing migrations: $($_.Exception.Message)"
        exit 1
    }
}

# Seed Data
if ($SeedData) {
    Write-Info "ðŸŒ± Seeding database with test data..."
    
    # For now, just show that we would seed data
    Write-Info "ðŸ“‹ Seeding would be implemented here"
    Write-Success "Database seeded (placeholder)"
}

Write-Success "Migration script completed successfully!"
