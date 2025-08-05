﻿# ========================================================================
# VPS Development Environment Setup Script
# ========================================================================
# Run this script on your VPS to setup development environment

param(
    [string]$SqlPassword = "VpsDevStrong!Pass123",
    [string]$DevelopmentPort = "5000",
    [switch]$InstallDocker,
    [switch]$SetupFirewall,
    [switch]$CreateAppDirectories,
    [switch]$All
)

if ($All) {
    $InstallDocker = $true
    $SetupFirewall = $true
    $CreateAppDirectories = $true
}

Write-Host "ðŸ—ï¸ Setting up VPS Development Environment" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green

# Check if running as Administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Error "âŒ This script must be run as Administrator"
    exit 1
}

# Install Docker Desktop
if ($InstallDocker) {
    Write-Host "ðŸ³ Installing Docker..." -ForegroundColor Cyan
    
    # Download Docker Desktop
    $dockerUrl = "https://desktop.docker.com/win/main/amd64/Docker%20Desktop%20Installer.exe"
    $dockerInstaller = "$env:TEMP\DockerDesktopInstaller.exe"
    
    Write-Host "ðŸ“¥ Downloading Docker Desktop..."
    Invoke-WebRequest -Uri $dockerUrl -OutFile $dockerInstaller
    
    Write-Host "ðŸ”§ Installing Docker Desktop..."
    Start-Process -FilePath $dockerInstaller -ArgumentList "install", "--quiet" -Wait
    
    Write-Host "âœ… Docker installed. Please restart VPS and run this script again."
    Write-Host "â„¹ï¸ After restart, enable Docker WSL2 backend if prompted."
}

# Setup Firewall Rules
if ($SetupFirewall) {
    Write-Host "ðŸ”¥ Configuring Firewall..." -ForegroundColor Cyan
    
    # Development Application
    New-NetFirewallRule -DisplayName "Development App" -Direction Inbound -Protocol TCP -LocalPort $DevelopmentPort -Action Allow -ErrorAction SilentlyContinue
    
    # SQL Server Development
    New-NetFirewallRule -DisplayName "SQL Server Dev" -Direction Inbound -Protocol TCP -LocalPort 1433 -Action Allow -ErrorAction SilentlyContinue
    
    # Redis Development
    New-NetFirewallRule -DisplayName "Redis Dev" -Direction Inbound -Protocol TCP -LocalPort 6379 -Action Allow -ErrorAction SilentlyContinue
    
    # PowerShell Remoting for deployment
    New-NetFirewallRule -DisplayName "PowerShell Remoting" -Direction Inbound -Protocol TCP -LocalPort 5985,5986 -Action Allow -ErrorAction SilentlyContinue
    
    Write-Host "âœ… Firewall rules configured"
}

# Create Application Directories
if ($CreateAppDirectories) {
    Write-Host "ðŸ“ Creating Application Directories..." -ForegroundColor Cyan
    
    $directories = @(
        "C:\Development",
        "C:\Development\base",
        "C:\Development\Logs",
        "C:\Development\Backups",
        "C:\Development\Scripts"
    )
    
    foreach ($dir in $directories) {
        if (!(Test-Path $dir)) {
            New-Item -ItemType Directory -Path $dir -Force
            Write-Host "âœ… Created: $dir"
        }
    }
}

# Create Docker Compose for Development
Write-Host "ðŸ³ Creating Docker Compose configuration..." -ForegroundColor Cyan

$dockerComposeContent = @"
version: '3.8'

services:
  # Development SQL Server
  sql-server-dev:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: base-sql-dev
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=$SqlPassword
      - MSSQL_PID=Developer
    ports:
      - "1433:1433"
    volumes:
      - sql-dev-data:/var/opt/mssql
    restart: unless-stopped
    healthcheck:
      test: ["CMD-SHELL", "/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $SqlPassword -C -Q 'SELECT 1'"]
      interval: 10s
      timeout: 5s
      retries: 5

  # Development Redis
  redis-dev:
    image: redis:7-alpine
    container_name: base-redis-dev
    ports:
      - "6379:6379"
    volumes:
      - redis-dev-data:/data
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 5s
      timeout: 3s
      retries: 5

volumes:
  sql-dev-data:
  redis-dev-data:

networks:
  default:
    name: base-dev-network
"@

$dockerComposeContent | Out-File -FilePath "C:\Development\docker-compose.yml" -Encoding UTF8

# Create deployment script
Write-Host "ðŸ“œ Creating deployment scripts..." -ForegroundColor Cyan

$deploymentScript = @"
# deployment-script.ps1
param(
    `$ApplicationPath = "C:\Development\base"
)

Write-Host "ðŸš€ Starting deployment..." -ForegroundColor Green

# Stop existing application
Write-Host "â¹ï¸ Stopping existing application..."
Get-Process -Name "UserManagement" -ErrorAction SilentlyContinue | Stop-Process -Force

# Wait a moment
Start-Sleep -Seconds 5

# Start new application
Write-Host "â–¶ï¸ Starting application..."
Set-Location "`$ApplicationPath"

# Set environment variables
`$env:ASPNETCORE_ENVIRONMENT = "VpsDevelopment"
`$env:ASPNETCORE_URLS = "http://0.0.0.0:$DevelopmentPort"

# Start application in background
Start-Process -FilePath "dotnet" -ArgumentList "UserManagement.dll" -WorkingDirectory "`$ApplicationPath" -WindowStyle Hidden

# Health check
Write-Host "ðŸ¥ Performing health check..."
Start-Sleep -Seconds 10

try {
    `$response = Invoke-WebRequest -Uri "http://localhost:$DevelopmentPort/health" -TimeoutSec 30
    if (`$response.StatusCode -eq 200) {
        Write-Host "âœ… Deployment successful!" -ForegroundColor Green
        Write-Host "ðŸŒ Application available at: http://YOUR_VPS_IP:$DevelopmentPort" -ForegroundColor Yellow
    }
} catch {
    Write-Host "âŒ Health check failed: `$(`$_.Exception.Message)" -ForegroundColor Red
    Write-Host "ðŸ“‹ Check application logs for details" -ForegroundColor Yellow
}
"@

$deploymentScript | Out-File -FilePath "C:\Development\Scripts\deploy.ps1" -Encoding UTF8

# Create start infrastructure script
$startInfraScript = @"
# start-infrastructure.ps1
Write-Host "ðŸ³ Starting Development Infrastructure..." -ForegroundColor Green

Set-Location "C:\Development"

# Start Docker containers
docker-compose up -d

# Wait for services to be ready
Write-Host "â³ Waiting for services to start..."
Start-Sleep -Seconds 30

# Check service status
Write-Host "ðŸ“‹ Service Status:" -ForegroundColor Yellow
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

# Test SQL Server connection
try {
    docker exec base-sql-dev /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SqlPassword" -C -Q "SELECT 'SQL Server Ready' as Status"
    Write-Host "âœ… SQL Server is ready" -ForegroundColor Green
} catch {
    Write-Host "âŒ SQL Server connection failed" -ForegroundColor Red
}

# Test Redis connection  
try {
    docker exec base-redis-dev redis-cli ping
    Write-Host "âœ… Redis is ready" -ForegroundColor Green
} catch {
    Write-Host "âŒ Redis connection failed" -ForegroundColor Red
}

Write-Host "ðŸŽ‰ Development infrastructure is ready!" -ForegroundColor Green
Write-Host "ðŸ“‹ Connection Info:" -ForegroundColor Yellow
Write-Host "   SQL Server: YOUR_VPS_IP:1433 (sa/$SqlPassword)"
Write-Host "   Redis: YOUR_VPS_IP:6379"
Write-Host "   App will be: YOUR_VPS_IP:$DevelopmentPort"
"@

$startInfraScript | Out-File -FilePath "C:\Development\Scripts\start-infrastructure.ps1" -Encoding UTF8

# Create PowerShell Remoting setup
Write-Host "ðŸ”§ Configuring PowerShell Remoting..." -ForegroundColor Cyan

try {
    Enable-PSRemoting -Force -SkipNetworkProfileCheck
    Set-WSManQuickConfig -Force
    
    # Configure authentication
    winrm set winrm/config/service/auth '@{Basic="true"}'
    winrm set winrm/config/service '@{AllowUnencrypted="true"}'
    
    Write-Host "âœ… PowerShell Remoting configured"
} catch {
    Write-Host "âš ï¸ PowerShell Remoting setup failed: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Create README
$readmeContent = @"
# VPS Development Environment

## ðŸš€ Quick Start

### 1. Start Infrastructure
```powershell
C:\Development\Scripts\start-infrastructure.ps1
```

### 2. Deploy Application (after GitHub Actions build)
```powershell
C:\Development\Scripts\deploy.ps1
```

## ðŸ“‹ Connection Information

### From Local Development:
- **SQL Server**: YOUR_VPS_IP:1433
  - User: sa
  - Password: $SqlPassword
  - Database: baseUserManagement_Learning

- **Redis**: YOUR_VPS_IP:6379

- **Application**: http://YOUR_VPS_IP:$DevelopmentPort

### Development Workflow:
1. Code on local laptop
2. Push to GitHub (develop branch)
3. GitHub Actions builds and deploys to VPS
4. Access application from any device

## ðŸ› ï¸ Management Commands

### Docker Commands:
```powershell
# Start services
docker-compose -f C:\Development\docker-compose.yml up -d

# Stop services  
docker-compose -f C:\Development\docker-compose.yml down

# View logs
docker logs base-sql-dev
docker logs base-redis-dev

# Check status
docker ps
```

### Application Commands:
```powershell
# Stop application
Get-Process -Name "UserManagement" | Stop-Process -Force

# Start application manually
cd C:\Development\base
dotnet UserManagement.dll --urls "http://0.0.0.0:$DevelopmentPort"
```

## ðŸ”§ Troubleshooting

### SQL Server Issues:
- Check container logs: `docker logs base-sql-dev`
- Test connection: `docker exec base-sql-dev /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SqlPassword" -C -Q "SELECT 1"`

### Application Issues:
- Check if port $DevelopmentPort is open
- Verify application files in C:\Development\base
- Check Windows Event Logs

### Network Issues:
- Verify firewall rules for ports 1433, 6379, $DevelopmentPort
- Test connectivity from local machine: `telnet YOUR_VPS_IP 1433`
"@

$readmeContent | Out-File -FilePath "C:\Development\README.md" -Encoding UTF8

Write-Host "ðŸŽ‰ VPS Development Environment Setup Complete!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""
Write-Host "ðŸ“‹ Next Steps:" -ForegroundColor Yellow
Write-Host "1. Replace 'YOUR_VPS_IP' in configuration files with actual VPS IP"
Write-Host "2. Run: C:\Development\Scripts\start-infrastructure.ps1"
Write-Host "3. Setup GitHub Actions secrets:"
Write-Host "   - VPS_HOST: Your VPS IP"
Write-Host "   - VPS_USER: Administrator (or deployment user)"
Write-Host "   - VPS_SSH_KEY: Private key for authentication"
Write-Host "4. Push code to 'develop' branch to trigger deployment"
Write-Host ""
Write-Host "ðŸ“– See C:\Development\README.md for detailed instructions"
