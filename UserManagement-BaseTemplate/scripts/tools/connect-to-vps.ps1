# ========================================================================
# Local Development with VPS Database Script
# ========================================================================
# Run this on your local laptop to connect to VPS development environment

param(
    [Parameter(Mandatory=$true)]
    [string]$VpsIp,
    [string]$SqlPassword = "VpsDevStrong!Pass123",
    [int]$SqlPort = 1433,
    [int]$RedisPort = 6379,
    [int]$AppPort = 5001,  # Local app port (different from VPS)
    [switch]$TestConnection,
    [switch]$RunApp,
    [switch]$RunMigrations,
    [switch]$Help
)

function Write-Info { param([string]$Message) Write-Host $Message -ForegroundColor Cyan }
function Write-Success { param([string]$Message) Write-Host $Message -ForegroundColor Green }
function Write-Warning { param([string]$Message) Write-Host $Message -ForegroundColor Yellow }
function Write-Error { param([string]$Message) Write-Host $Message -ForegroundColor Red }

if ($Help) {
    Write-Info "ðŸŒ Local Development with VPS Database"
    Write-Info "======================================"
    Write-Info ""
    Write-Info "This script helps you develop locally while using VPS database"
    Write-Info ""
    Write-Info "USAGE:"
    Write-Info "  .\connect-to-vps.ps1 -VpsIp YOUR_VPS_IP [OPTIONS]"
    Write-Info ""
    Write-Info "OPTIONS:"
    Write-Info "  -VpsIp          VPS IP address (required)"
    Write-Info "  -SqlPassword    SQL Server password (default: VpsDevStrong!Pass123)"
    Write-Info "  -SqlPort        SQL Server port (default: 1433)"
    Write-Info "  -RedisPort      Redis port (default: 6379)"
    Write-Info "  -AppPort        Local app port (default: 5001)"
    Write-Info "  -TestConnection Test connection to VPS services"
    Write-Info "  -RunApp         Start local app connected to VPS"
    Write-Info "  -RunMigrations  Run database migrations on VPS"
    Write-Info "  -Help           Show this help"
    Write-Info ""
    Write-Info "EXAMPLES:"
    Write-Info "  .\connect-to-vps.ps1 -VpsIp 192.168.1.100 -TestConnection"
    Write-Info "  .\connect-to-vps.ps1 -VpsIp 192.168.1.100 -RunApp"
    Write-Info "  .\connect-to-vps.ps1 -VpsIp 192.168.1.100 -RunMigrations"
    exit 0
}

Write-Info "ðŸŒ Local Development with VPS Database"
Write-Info "======================================"
Write-Info "VPS IP: $VpsIp"
Write-Info "SQL Server: $VpsIp`:$SqlPort"
Write-Info "Redis: $VpsIp`:$RedisPort"
Write-Info "Local App: http://localhost:$AppPort"
Write-Info ""

# Test VPS connection
if ($TestConnection) {
    Write-Info "ðŸ” Testing VPS connections..."
    
    # Test SQL Server
    Write-Info "Testing SQL Server connection..."
    try {
        $connectionString = "Server=$VpsIp,$SqlPort;Database=master;User Id=sa;Password=$SqlPassword;TrustServerCertificate=true;Connection Timeout=10;"
        $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
        $connection.Open()
        $connection.Close()
        Write-Success "âœ… SQL Server connection successful"
    } catch {
        Write-Error "âŒ SQL Server connection failed: $($_.Exception.Message)"
    }
    
    # Test Redis
    Write-Info "Testing Redis connection..."
    try {
        $tcpClient = New-Object System.Net.Sockets.TcpClient
        $tcpClient.Connect($VpsIp, $RedisPort)
        $tcpClient.Close()
        Write-Success "âœ… Redis connection successful"
    } catch {
        Write-Error "âŒ Redis connection failed: $($_.Exception.Message)"
    }
    
    # Test VPS application
    Write-Info "Testing VPS application..."
    try {
        $response = Invoke-WebRequest -Uri "http://$VpsIp`:5000/health" -TimeoutSec 10 -ErrorAction Stop
        Write-Success "âœ… VPS application is running (Status: $($response.StatusCode))"
    } catch {
        Write-Warning "âš ï¸ VPS application not accessible: $($_.Exception.Message)"
    }
}

# Run database migrations
if ($RunMigrations) {
    Write-Info "ðŸ—„ï¸ Running database migrations on VPS..."
    
    # Check if we're in the right directory
    if (!(Test-Path "src/Backend/UserManagement/UserManagement.csproj")) {
        Write-Error "âŒ Please run this script from the project root directory"
        exit 1
    }
    
    # Set connection string environment variable
    $connectionString = "Server=$VpsIp,$SqlPort;Database=baseUserManagement_Learning;User Id=sa;Password=$SqlPassword;TrustServerCertificate=true;"
    $env:ConnectionStrings__DefaultConnection = $connectionString
    
    try {
        Set-Location "src/Backend/UserManagement/UserManagement.API"
        dotnet ef database update --connection $connectionString
        if ($LASTEXITCODE -eq 0) {
            Write-Success "âœ… Database migrations completed successfully"
        } else {
            Write-Error "âŒ Database migrations failed"
            exit 1
        }
    } catch {
        Write-Error "âŒ Migration error: $($_.Exception.Message)"
        exit 1
    } finally {
        Set-Location "../../.."
    }
}

# Run local application
if ($RunApp) {
    Write-Info "ðŸš€ Starting local application connected to VPS database..."
    
    # Check if we're in the right directory
    if (!(Test-Path "src/Backend/UserManagement/UserManagement.csproj")) {
        Write-Error "âŒ Please run this script from the project root directory"
        exit 1
    }
    
    # Set environment variables
    $env:ASPNETCORE_ENVIRONMENT = "VpsLearning"
    $env:ASPNETCORE_URLS = "http://localhost:$AppPort"
    $env:ConnectionStrings__DefaultConnection = "Server=$VpsIp,$SqlPort;Database=baseUserManagement_Learning;User Id=sa;Password=$SqlPassword;TrustServerCertificate=true;"
    $env:ConnectionStrings__Redis = "$VpsIp`:$RedisPort"
    
    Write-Info "ðŸ”§ Environment configured:"
    Write-Info "   Database: $VpsIp`:$SqlPort"
    Write-Info "   Redis: $VpsIp`:$RedisPort"
    Write-Info "   Local URL: http://localhost:$AppPort"
    Write-Info ""
    Write-Success "ðŸŒŸ Application starting... Press Ctrl+C to stop"
    Write-Info ""
    
    try {
        Set-Location "src/Backend/UserManagement/UserManagement.API"
        dotnet run --urls "http://localhost:$AppPort"
    } catch {
        Write-Error "âŒ Application startup failed: $($_.Exception.Message)"
        exit 1
    } finally {
        Set-Location "../../../.."
    }
}

# If no action specified, show status
if (!$TestConnection -and !$RunApp -and !$RunMigrations) {
    Write-Info "ðŸ“‹ Available commands:"
    Write-Info "  -TestConnection    Test VPS connectivity"
    Write-Info "  -RunMigrations     Run database migrations"
    Write-Info "  -RunApp           Start local app with VPS database"
    Write-Info "  -Help             Show detailed help"
    Write-Info ""
    Write-Info "ðŸ’¡ Example: .\connect-to-vps.ps1 -VpsIp $VpsIp -TestConnection"
}
