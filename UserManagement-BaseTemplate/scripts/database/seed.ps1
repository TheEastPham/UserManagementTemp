# Secure Database Seeding Setup Script
param(
    [string]$AdminEmail = "",
    [SecureString]$AdminPassword,
    [switch]$Force,
    [string]$Environment = "Development"
)

Write-Host "🔒 Secure base Database Seeding Script" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green

# Validate inputs
if ([string]::IsNullOrEmpty($AdminEmail)) {
    $AdminEmail = Read-Host "Enter admin email"
}

if ($null -eq $AdminPassword) {
    $AdminPassword = Read-Host "Enter admin password" -AsSecureString
}

# Convert SecureString to plain text for environment variable
$AdminPasswordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($AdminPassword))

$projectPath = $PSScriptRoot
$userManagementPath = Join-Path $projectPath "..\src\Backend\UserManagement"

# Check if project exists
if (-not (Test-Path $userManagementPath)) {
    Write-Error "UserManagement project not found at: $userManagementPath"
    exit 1
}

# Set environment variables securely
$env:ASPNETCORE_ENVIRONMENT = $Environment
$env:base_ADMIN_EMAIL = $AdminEmail
$env:base_ADMIN_PASSWORD = $AdminPasswordPlain

Write-Host "Environment: $Environment" -ForegroundColor Yellow
Write-Host "Admin Email: $AdminEmail" -ForegroundColor Yellow
Write-Host "Admin Password: [HIDDEN]" -ForegroundColor Yellow

try {
    Push-Location $userManagementPath
    
    # Build the project first
    Write-Host "🔨 Building UserManagement project..." -ForegroundColor Yellow
    dotnet build --configuration Debug
    
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed"
    }
    
    # Check if we need to create/update database
    Write-Host "🔌 Checking database..." -ForegroundColor Yellow
    
    if ($Force) {
        Write-Host "⚠️  Force flag detected - Dropping existing database..." -ForegroundColor Red
        dotnet ef database drop --force
    }
    
    # Create/Update database
    Write-Host "📊 Creating/Updating database..." -ForegroundColor Yellow
    dotnet ef database update
    
    if ($LASTEXITCODE -ne 0) {
        throw "Database update failed"
    }
    
    Write-Host "✅ Database created/updated successfully!" -ForegroundColor Green
    
    # Run the application briefly to trigger automatic seeding
    Write-Host "🌱 Starting application to trigger database seeding..." -ForegroundColor Yellow
    Write-Host "   Application will run seeding and then you can test login" -ForegroundColor Cyan
    
    # Start the application - it will seed automatically
    dotnet run --environment $Environment
    
} catch {
    Write-Error "❌ Seeding failed: $_"
    exit 1
} finally {
    # Clear sensitive environment variables
    $env:base_ADMIN_EMAIL = $null
    $env:base_ADMIN_PASSWORD = $null
    Pop-Location
}

Write-Host "✅ Secure database setup completed!" -ForegroundColor Green
