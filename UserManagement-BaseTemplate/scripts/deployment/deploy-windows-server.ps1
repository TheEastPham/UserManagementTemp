# ==============================================================================
# Windows Server 2019 Deployment Script for base Platform
# ==============================================================================

param(
    [Parameter(Mandatory=$true)]
    [string]$ServerIP,
    
    [Parameter(Mandatory=$true)]
    [string]$SqlServerPassword,
    
    [Parameter(Mandatory=$true)]
    [string]$JwtSecretKey,
    
    [string]$RedisPassword = "",
    [string]$Environment = "Production"
)

Write-Host "🚀 Starting base Platform Deployment..." -ForegroundColor Green

# ==============================================================================
# 1. Prerequisites Check
# ==============================================================================
Write-Host "📋 Checking Prerequisites..." -ForegroundColor Yellow

# Check if running as Administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Error "❌ This script must be run as Administrator!"
    exit 1
}

# Check .NET 8 installation
try {
    $dotnetVersion = dotnet --version
    if ($dotnetVersion -notlike "8.*") {
        throw "Wrong .NET version"
    }
    Write-Host "✅ .NET 8 is installed: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Error "❌ .NET 8 is not installed or not found in PATH"
    exit 1
}

# ==============================================================================
# 2. Create Directory Structure
# ==============================================================================
Write-Host "📁 Creating Directory Structure..." -ForegroundColor Yellow

$deployPaths = @(
    "C:\inetpub\base\gateway",
    "C:\inetpub\base\usermanagement",
    "C:\base\logs",
    "C:\base\backups",
    "C:\base\temp"
)

foreach ($path in $deployPaths) {
    if (!(Test-Path $path)) {
        New-Item -ItemType Directory -Path $path -Force
        Write-Host "✅ Created: $path" -ForegroundColor Green
    } else {
        Write-Host "ℹ️ Already exists: $path" -ForegroundColor Blue
    }
}

# ==============================================================================
# 3. Configure IIS
# ==============================================================================
Write-Host "🌐 Configuring IIS..." -ForegroundColor Yellow

Import-Module WebAdministration

# Remove default website
if (Get-Website -Name "Default Web Site" -ErrorAction SilentlyContinue) {
    Remove-Website -Name "Default Web Site"
    Write-Host "✅ Removed Default Web Site" -ForegroundColor Green
}

# Create Application Pools
$appPools = @(
    @{Name = "baseGateway"; Path = "C:\inetpub\base\gateway"; Port = 5000},
    @{Name = "baseUserManagement"; Path = "C:\inetpub\base\usermanagement"; Port = 5001}
)

foreach ($pool in $appPools) {
    # Remove existing
    if (Get-IISAppPool -Name $pool.Name -ErrorAction SilentlyContinue) {
        Remove-WebAppPool -Name $pool.Name
    }
    
    # Create new
    New-WebAppPool -Name $pool.Name -Force
    Set-ItemProperty -Path "IIS:\AppPools\$($pool.Name)" -Name "processModel.identityType" -Value "ApplicationPoolIdentity"
    Set-ItemProperty -Path "IIS:\AppPools\$($pool.Name)" -Name "managedRuntimeVersion" -Value ""
    Set-ItemProperty -Path "IIS:\AppPools\$($pool.Name)" -Name "recycling.periodicRestart.time" -Value "00:00:00"
    
    # Remove existing website
    if (Get-Website -Name $pool.Name -ErrorAction SilentlyContinue) {
        Remove-Website -Name $pool.Name
    }
    
    # Create website
    New-Website -Name $pool.Name -Port $pool.Port -PhysicalPath $pool.Path -ApplicationPool $pool.Name
    Write-Host "✅ Created IIS site: $($pool.Name) on port $($pool.Port)" -ForegroundColor Green
}

# ==============================================================================
# 4. Configure Firewall
# ==============================================================================
Write-Host "🔥 Configuring Firewall..." -ForegroundColor Yellow

$firewallRules = @(
    @{Name = "base Gateway HTTP"; Port = 5000},
    @{Name = "base UserManagement HTTP"; Port = 5001},
    @{Name = "SQL Server"; Port = 1433},
    @{Name = "Redis"; Port = 6379}
)

foreach ($rule in $firewallRules) {
    if (Get-NetFirewallRule -DisplayName $rule.Name -ErrorAction SilentlyContinue) {
        Remove-NetFirewallRule -DisplayName $rule.Name
    }
    New-NetFirewallRule -DisplayName $rule.Name -Direction Inbound -Protocol TCP -LocalPort $rule.Port -Action Allow
    Write-Host "✅ Firewall rule created: $($rule.Name) - Port $($rule.Port)" -ForegroundColor Green
}

# ==============================================================================
# 5. Database Setup
# ==============================================================================
Write-Host "🗄️ Setting up Database..." -ForegroundColor Yellow

$connectionString = "Server=localhost;Database=master;Integrated Security=true;TrustServerCertificate=true;"
$dbName = "baseUserManagement"
$appUser = "base_app"

try {
    # Test SQL Server connection
    $testQuery = "SELECT @@VERSION"
    Invoke-Sqlcmd -Query $testQuery -ConnectionString $connectionString
    Write-Host "✅ SQL Server connection successful" -ForegroundColor Green
    
    # Create database if not exists
    $createDbQuery = @"
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = '$dbName')
BEGIN
    CREATE DATABASE [$dbName];
    ALTER DATABASE [$dbName] SET RECOVERY SIMPLE;
END
"@
    Invoke-Sqlcmd -Query $createDbQuery -ConnectionString $connectionString
    Write-Host "✅ Database '$dbName' created/verified" -ForegroundColor Green
    
    # Create application user
    $createUserQuery = @"
USE [$dbName];
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = '$appUser')
BEGIN
    CREATE LOGIN [$appUser] WITH PASSWORD = '$SqlServerPassword';
END

IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = '$appUser')
BEGIN
    CREATE USER [$appUser] FOR LOGIN [$appUser];
    ALTER ROLE db_owner ADD MEMBER [$appUser];
END
"@
    Invoke-Sqlcmd -Query $createUserQuery -ConnectionString $connectionString
    Write-Host "✅ Application user '$appUser' created/verified" -ForegroundColor Green
    
} catch {
    Write-Error "❌ Database setup failed: $($_.Exception.Message)"
    exit 1
}

# ==============================================================================
# 6. Environment Variables
# ==============================================================================
Write-Host "🔧 Setting Environment Variables..." -ForegroundColor Yellow

$envVars = @{
    "ASPNETCORE_ENVIRONMENT" = $Environment
    "ConnectionStrings__DefaultConnection" = "Server=localhost;Database=$dbName;User Id=$appUser;Password=$SqlServerPassword;TrustServerCertificate=true;"
    "ConnectionStrings__Redis" = "localhost:6379"
    "JwtSettings__SecretKey" = $JwtSecretKey
    "JwtSettings__Issuer" = "basePlatform"
    "JwtSettings__Audience" = "baseUsers"
    "JwtSettings__ExpiryInDays" = "7"
    "Serilog__MinimumLevel" = "Information"
    "Serilog__WriteTo__0__Name" = "File"
    "Serilog__WriteTo__0__Args__path" = "C:\base\logs\app-.log"
    "Serilog__WriteTo__0__Args__rollingInterval" = "Day"
}

foreach ($var in $envVars.GetEnumerator()) {
    [Environment]::SetEnvironmentVariable($var.Key, $var.Value, "Machine")
    Write-Host "✅ Set environment variable: $($var.Key)" -ForegroundColor Green
}

# ==============================================================================
# 7. Install Redis (Optional)
# ==============================================================================
Write-Host "📦 Installing Redis..." -ForegroundColor Yellow

try {
    # Check if Redis is already installed
    $redisService = Get-Service -Name "Redis" -ErrorAction SilentlyContinue
    if ($redisService) {
        Write-Host "ℹ️ Redis service already exists" -ForegroundColor Blue
    } else {
        # Download and install Redis
        $redisUrl = "https://github.com/tporadowski/redis/releases/download/v5.0.14.1/Redis-x64-5.0.14.1.zip"
        $redisZip = "C:\base\temp\redis.zip"
        $redisPath = "C:\Redis"
        
        Invoke-WebRequest -Uri $redisUrl -OutFile $redisZip
        Expand-Archive -Path $redisZip -DestinationPath $redisPath -Force
        
        # Install as service
        & "$redisPath\redis-server.exe" --service-install --service-name "Redis" --port 6379
        Start-Service -Name "Redis"
        Write-Host "✅ Redis installed and started" -ForegroundColor Green
    }
} catch {
    Write-Warning "⚠️ Redis installation failed: $($_.Exception.Message)"
    Write-Host "ℹ️ You can install Redis manually later" -ForegroundColor Blue
}

# ==============================================================================
# 8. Create Deployment Scripts
# ==============================================================================
Write-Host "📜 Creating Deployment Helper Scripts..." -ForegroundColor Yellow

# Create deployment batch file
$deployBatch = @"
@echo off
echo Stopping IIS Application Pools...
%windir%\system32\inetsrv\appcmd.exe stop apppool /apppool.name:"baseGateway"
%windir%\system32\inetsrv\appcmd.exe stop apppool /apppool.name:"baseUserManagement"

echo Backing up current deployment...
if exist "C:\base\backups\%date:~-4,4%%date:~-10,2%%date:~-7,2%" rmdir /s /q "C:\base\backups\%date:~-4,4%%date:~-10,2%%date:~-7,2%"
mkdir "C:\base\backups\%date:~-4,4%%date:~-10,2%%date:~-7,2%"
xcopy /s /e "C:\inetpub\base" "C:\base\backups\%date:~-4,4%%date:~-10,2%%date:~-7,2%\"

echo Copying new deployment files...
xcopy /s /e /y ".\publish\gateway\*" "C:\inetpub\base\gateway\"
xcopy /s /e /y ".\publish\usermanagement\*" "C:\inetpub\base\usermanagement\"

echo Starting IIS Application Pools...
%windir%\system32\inetsrv\appcmd.exe start apppool /apppool.name:"baseGateway"
%windir%\system32\inetsrv\appcmd.exe start apppool /apppool.name:"baseUserManagement"

echo Deployment completed!
pause
"@

$deployBatch | Out-File -FilePath "C:\base\deploy.bat" -Encoding ASCII
Write-Host "✅ Created deployment script: C:\base\deploy.bat" -ForegroundColor Green

# Create health check script
$healthCheckPs1 = @"
Write-Host "🔍 Checking base Platform Health..." -ForegroundColor Green

`$endpoints = @(
    "http://localhost:5000/api/health",
    "http://localhost:5001/api/health"
)

foreach (`$endpoint in `$endpoints) {
    try {
        `$response = Invoke-RestMethod -Uri `$endpoint -TimeoutSec 10
        if (`$response.status -eq "Healthy") {
            Write-Host "✅ `$endpoint - Healthy" -ForegroundColor Green
        } else {
            Write-Host "⚠️ `$endpoint - `$(`$response.status)" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "❌ `$endpoint - Failed: `$(`$_.Exception.Message)" -ForegroundColor Red
    }
}
"@

$healthCheckPs1 | Out-File -FilePath "C:\base\health-check.ps1" -Encoding UTF8
Write-Host "✅ Created health check script: C:\base\health-check.ps1" -ForegroundColor Green

# ==============================================================================
# 9. Final Setup
# ==============================================================================
Write-Host "🎯 Final Setup Steps..." -ForegroundColor Yellow

# Create logs directory structure
New-Item -ItemType Directory -Path "C:\base\logs\gateway" -Force
New-Item -ItemType Directory -Path "C:\base\logs\usermanagement" -Force

# Set permissions
$acl = Get-Acl "C:\base"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS_IUSRS", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.SetAccessRule($accessRule)
Set-Acl "C:\base" $acl
Write-Host "✅ Set permissions for IIS_IUSRS" -ForegroundColor Green

# ==============================================================================
# 10. Summary
# ==============================================================================
Write-Host "`n🎉 Deployment Setup Completed!" -ForegroundColor Green
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host "📍 Deployment Locations:" -ForegroundColor Yellow
Write-Host "   • API Gateway: C:\inetpub\base\gateway (Port 5000)" -ForegroundColor White
Write-Host "   • User Management: C:\inetpub\base\usermanagement (Port 5001)" -ForegroundColor White
Write-Host "   • Logs: C:\base\logs" -ForegroundColor White
Write-Host "   • Backups: C:\base\backups" -ForegroundColor White

Write-Host "`n📍 Health Check Endpoints:" -ForegroundColor Yellow
Write-Host "   • http://$ServerIP`:5000/api/health" -ForegroundColor White
Write-Host "   • http://$ServerIP`:5001/api/health" -ForegroundColor White

Write-Host "`n📍 Next Steps:" -ForegroundColor Yellow
Write-Host "   1. Deploy your application files to the created directories" -ForegroundColor White
Write-Host "   2. Run Entity Framework migrations: dotnet ef database update" -ForegroundColor White
Write-Host "   3. Test the health check endpoints" -ForegroundColor White
Write-Host "   4. Configure SSL certificates for production" -ForegroundColor White
Write-Host "   5. Set up monitoring and alerting" -ForegroundColor White

Write-Host "`n📍 Useful Commands:" -ForegroundColor Yellow
Write-Host "   • Check health: PowerShell -File 'C:\base\health-check.ps1'" -ForegroundColor White
Write-Host "   • Deploy: C:\base\deploy.bat" -ForegroundColor White
Write-Host "   • View logs: Get-Content 'C:\base\logs\app-*.log' -Tail 50" -ForegroundColor White

Write-Host "`n===========================================" -ForegroundColor Cyan
