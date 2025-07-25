# Setup Integration Test Environment
param(
    [switch]$SkipDockerCheck,
    [switch]$StartServices,
    [switch]$StopServices,
    [switch]$ShowStatus,
    [SecureString]$SqlPassword
)

# Convert SecureString to plain text for display purposes
$SqlPasswordPlain = if ($SqlPassword) { 
    [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($SqlPassword))
} else { 
    "IntegrationTest123!" 
}

Write-Host "Integration Test Environment Setup" -ForegroundColor Green

# Check if Docker is available
if (-not $SkipDockerCheck) {
    Write-Host "Checking Docker availability..." -ForegroundColor Yellow
    
    try {
        docker --version | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw "Docker command failed"
        }
        Write-Host "Docker is available" -ForegroundColor Green
    } catch {
        Write-Host "Docker is not available or not running!" -ForegroundColor Red
        Write-Host "Please install Docker Desktop and ensure it's running." -ForegroundColor Yellow
        exit 1
    }
    
    try {
        docker-compose --version | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw "Docker Compose command failed"
        }
        Write-Host "Docker Compose is available" -ForegroundColor Green
    } catch {
        Write-Host "Docker Compose is not available!" -ForegroundColor Red
        Write-Host "Please install Docker Compose." -ForegroundColor Yellow
        exit 1
    }
}

# Ensure we're in the correct directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir
Set-Location $RootDir

# Show current status
if ($ShowStatus) {
    Write-Host "Current integration test services status:" -ForegroundColor Cyan
    docker-compose -f docker-compose.integration-tests.yml ps
    exit 0
}

# Stop services
if ($StopServices) {
    Write-Host "Stopping integration test services..." -ForegroundColor Yellow
    docker-compose -f docker-compose.integration-tests.yml down -v --remove-orphans
    Write-Host "Services stopped and volumes removed" -ForegroundColor Green
    exit 0
}

# Start services
if ($StartServices) {
    Write-Host "Starting integration test services..." -ForegroundColor Yellow
    
    # Check if docker-compose file exists
    if (-not (Test-Path "docker-compose.integration-tests.yml")) {
        Write-Host "ERROR: docker-compose.integration-tests.yml not found!" -ForegroundColor Red
        Write-Host "Current directory: $(Get-Location)" -ForegroundColor Gray
        Write-Host "Please ensure you're running from the project root directory." -ForegroundColor Yellow
        exit 1
    }
    
    # Clean up any existing containers first
    Write-Host "Cleaning up existing containers..." -ForegroundColor Gray
    docker-compose -f docker-compose.integration-tests.yml down -v --remove-orphans 2>$null
    
    # Start SQL Server and Redis
    Write-Host "Starting SQL Server and Redis containers..." -ForegroundColor Yellow
    $startResult = docker-compose -f docker-compose.integration-tests.yml up -d sqlserver redis
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Failed to start containers!" -ForegroundColor Red
        Write-Host "Docker Compose output:" -ForegroundColor Gray
        Write-Host $startResult -ForegroundColor Gray
        Write-Host ""
        Write-Host "Checking Docker Compose logs..." -ForegroundColor Yellow
        docker-compose -f docker-compose.integration-tests.yml logs
        exit 1
    }
    
    # Wait for services to be healthy
    Write-Host "Waiting for services to be healthy..." -ForegroundColor Yellow
    $maxWait = 120
    $elapsed = 0
    
    do {
        # Check container status more thoroughly
        Write-Host "Checking container status..." -ForegroundColor Gray
        $containerStatus = docker-compose -f docker-compose.integration-tests.yml ps
        Write-Host $containerStatus -ForegroundColor Gray
        
        # Check health status using docker inspect
        $sqlHealthy = $false
        $redisHealthy = $false
        
        try {
            $sqlHealth = docker inspect --format='{{.State.Health.Status}}' base-sqlserver-1 2>$null
            $redisHealth = docker inspect --format='{{.State.Health.Status}}' base-redis-1 2>$null
            
            $sqlHealthy = $sqlHealth -eq "healthy"
            $redisHealthy = $redisHealth -eq "healthy"
        } catch {
            Write-Host "Unable to check health status, retrying..." -ForegroundColor Yellow
        }
        
        if ($sqlHealthy -and $redisHealthy) {
            Write-Host "All services are healthy!" -ForegroundColor Green
            break
        }
        
        # Show what's not healthy yet
        if (-not $sqlHealthy) {
            Write-Host "SQL Server health: $sqlHealth" -ForegroundColor Yellow
        }
        if (-not $redisHealthy) {
            Write-Host "Redis health: $redisHealth" -ForegroundColor Yellow
        }
        
        Write-Host "Waiting for services to be healthy... ($elapsed/$maxWait seconds)" -ForegroundColor Yellow
        Start-Sleep -Seconds 5
        $elapsed += 5
        
    } while ($elapsed -lt $maxWait)
    
    if ($elapsed -ge $maxWait) {
        Write-Host "Timeout waiting for services to be healthy!" -ForegroundColor Red
        Write-Host ""
        Write-Host "=== Debugging Information ===" -ForegroundColor Cyan
        Write-Host "Container Status:" -ForegroundColor Yellow
        docker-compose -f docker-compose.integration-tests.yml ps
        Write-Host ""
        Write-Host "SQL Server Logs:" -ForegroundColor Yellow
        docker-compose -f docker-compose.integration-tests.yml logs --tail=20 sqlserver
        Write-Host ""
        Write-Host "Redis Logs:" -ForegroundColor Yellow
        docker-compose -f docker-compose.integration-tests.yml logs --tail=20 redis
        Write-Host ""
        Write-Host "=== Troubleshooting Tips ===" -ForegroundColor Cyan
        Write-Host "1. Check if ports 1433 and 6379 are available" -ForegroundColor Gray
        Write-Host "2. Ensure Docker Desktop has enough resources allocated" -ForegroundColor Gray
        Write-Host "3. Try running: docker-compose -f docker-compose.integration-tests.yml down -v" -ForegroundColor Gray
        Write-Host "4. Then retry the setup" -ForegroundColor Gray
        exit 1
    }
    
    Write-Host "Services are running and healthy!" -ForegroundColor Green
    Write-Host "SQL Server: localhost:1433 (SA/$SqlPasswordPlain)" -ForegroundColor Gray
    Write-Host "Redis: localhost:6379" -ForegroundColor Gray
    
    exit 0
}

# Default: Show usage
Write-Host "Usage:" -ForegroundColor Yellow
Write-Host "  Setup environment and start services:" -ForegroundColor Gray
Write-Host "    ./scripts/setup-integration-tests.ps1 -StartServices" -ForegroundColor Gray
Write-Host ""
Write-Host "  Stop services:" -ForegroundColor Gray  
Write-Host "    ./scripts/setup-integration-tests.ps1 -StopServices" -ForegroundColor Gray
Write-Host ""
Write-Host "  Show status:" -ForegroundColor Gray
Write-Host "    ./scripts/setup-integration-tests.ps1 -ShowStatus" -ForegroundColor Gray
Write-Host ""
Write-Host "  Run integration tests:" -ForegroundColor Gray
Write-Host "    ./scripts/run-integration-tests.ps1" -ForegroundColor Gray
Write-Host ""
Write-Host "Connection strings for local development:" -ForegroundColor Cyan
Write-Host "  SQL Server: Server=localhost,1433;Database=baseTest;User Id=SA;Password=$SqlPasswordPlain;TrustServerCertificate=True;MultipleActiveResultSets=true" -ForegroundColor Gray
Write-Host "  Redis: localhost:6379" -ForegroundColor Gray
