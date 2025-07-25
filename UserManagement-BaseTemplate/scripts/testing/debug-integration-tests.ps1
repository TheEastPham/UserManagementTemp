# Debug Integration Test Environment
param(
    [switch]$CheckPorts,
    [switch]$CheckDocker,
    [switch]$CheckImages,
    [switch]$CleanAll,
    [switch]$ShowLogs,
    [switch]$TestConnections,
    [switch]$All
)

Write-Host "Integration Test Environment Debugger" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Green

# Ensure we're in the correct directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir
Set-Location $RootDir

if ($All) {
    $CheckPorts = $true
    $CheckDocker = $true
    $CheckImages = $true
    $ShowLogs = $true
    $TestConnections = $true
}

# Check Docker status
if ($CheckDocker -or $All) {
    Write-Host "`n=== Docker Status ===" -ForegroundColor Cyan
    
    try {
        $dockerVersion = docker --version
        Write-Host "Docker: $dockerVersion" -ForegroundColor Green
    } catch {
        Write-Host "Docker: NOT AVAILABLE" -ForegroundColor Red
        Write-Host "Please install Docker Desktop and ensure it's running." -ForegroundColor Yellow
        return
    }
    
    try {
        $composeVersion = docker-compose --version
        Write-Host "Docker Compose: $composeVersion" -ForegroundColor Green
    } catch {
        Write-Host "Docker Compose: NOT AVAILABLE" -ForegroundColor Red
        Write-Host "Please install Docker Compose." -ForegroundColor Yellow
        return
    }
    
    # Check Docker daemon
    try {
        docker info | Out-Null
        Write-Host "Docker Daemon: RUNNING" -ForegroundColor Green
    } catch {
        Write-Host "Docker Daemon: NOT RUNNING" -ForegroundColor Red
        Write-Host "Please start Docker Desktop." -ForegroundColor Yellow
        return
    }
    
    # Check Docker resources
    Write-Host "`nDocker System Info:" -ForegroundColor Gray
    docker system df
}

# Check port availability
if ($CheckPorts -or $All) {
    Write-Host "`n=== Port Availability ===" -ForegroundColor Cyan
    
    $ports = @(1433, 6379)
    foreach ($port in $ports) {
        try {
            $connection = Test-NetConnection -ComputerName localhost -Port $port -WarningAction SilentlyContinue
            if ($connection.TcpTestSucceeded) {
                Write-Host "Port ${port}: IN USE" -ForegroundColor Red
                
                # Try to find what's using the port
                try {
                    $process = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue | Select-Object -First 1
                    if ($process) {
                        $processInfo = Get-Process -Id $process.OwningProcess -ErrorAction SilentlyContinue
                        Write-Host "  Used by: $($processInfo.ProcessName) (PID: $($processInfo.Id))" -ForegroundColor Yellow
                    }
                } catch {
                    Write-Host "  Unable to determine process using port ${port}" -ForegroundColor Yellow
                }
            } else {
                Write-Host "Port ${port}: AVAILABLE" -ForegroundColor Green
            }
        } catch {
            Write-Host "Port ${port}: AVAILABLE" -ForegroundColor Green
        }
    }
}

# Check Docker images
if ($CheckImages -or $All) {
    Write-Host "`n=== Docker Images ===" -ForegroundColor Cyan
    
    $requiredImages = @(
        "mcr.microsoft.com/mssql/server:2022-latest",
        "redis:7-alpine"
    )
    
    foreach ($image in $requiredImages) {
        try {
            $imageInfo = docker images $image --format "table {{.Repository}}:{{.Tag}}\t{{.Size}}\t{{.CreatedAt}}"
            if ($imageInfo -and $imageInfo.Count -gt 1) {
                Write-Host "${image}: AVAILABLE" -ForegroundColor Green
                Write-Host "  $($imageInfo[1])" -ForegroundColor Gray
            } else {
                Write-Host "${image}: NOT AVAILABLE" -ForegroundColor Yellow
                Write-Host "  Will be downloaded when needed" -ForegroundColor Gray
            }
        } catch {
            Write-Host "${image}: NOT AVAILABLE" -ForegroundColor Yellow
        }
    }
}

# Show current containers and logs
if ($ShowLogs -or $All) {
    Write-Host "`n=== Current Containers ===" -ForegroundColor Cyan
    
    if (Test-Path "docker-compose.integration-tests.yml") {
        $containers = docker-compose -f docker-compose.integration-tests.yml ps
        if ($containers) {
            Write-Host $containers -ForegroundColor Gray
            
            Write-Host "`n=== Recent Logs ===" -ForegroundColor Cyan
            Write-Host "SQL Server logs:" -ForegroundColor Yellow
            docker-compose -f docker-compose.integration-tests.yml logs --tail=10 sqlserver 2>$null
            
            Write-Host "`nRedis logs:" -ForegroundColor Yellow
            docker-compose -f docker-compose.integration-tests.yml logs --tail=10 redis 2>$null
        } else {
            Write-Host "No containers are currently running." -ForegroundColor Gray
        }
    } else {
        Write-Host "docker-compose.integration-tests.yml not found!" -ForegroundColor Red
    }
}

# Test connections
if ($TestConnections -or $All) {
    Write-Host "`n=== Testing Connections ===" -ForegroundColor Cyan
    
    # Test SQL Server
    Write-Host "Testing SQL Server connection..." -ForegroundColor Yellow
    try {
        $sqlConnection = Test-NetConnection -ComputerName localhost -Port 1433 -WarningAction SilentlyContinue
        if ($sqlConnection.TcpTestSucceeded) {
            Write-Host "SQL Server: CONNECTION OK" -ForegroundColor Green
        } else {
            Write-Host "SQL Server: CONNECTION FAILED" -ForegroundColor Red
        }
    } catch {
        Write-Host "SQL Server: CONNECTION FAILED" -ForegroundColor Red
    }
    
    # Test Redis
    Write-Host "Testing Redis connection..." -ForegroundColor Yellow
    try {
        $redisConnection = Test-NetConnection -ComputerName localhost -Port 6379 -WarningAction SilentlyContinue
        if ($redisConnection.TcpTestSucceeded) {
            Write-Host "Redis: CONNECTION OK" -ForegroundColor Green
        } else {
            Write-Host "Redis: CONNECTION FAILED" -ForegroundColor Red
        }
    } catch {
        Write-Host "Redis: CONNECTION FAILED" -ForegroundColor Red
    }
}

# Clean all Docker resources
if ($CleanAll) {
    Write-Host "`n=== Cleaning Docker Resources ===" -ForegroundColor Cyan
    
    Write-Host "Stopping all integration test containers..." -ForegroundColor Yellow
    docker-compose -f docker-compose.integration-tests.yml down -v --remove-orphans 2>$null
    
    Write-Host "Removing dangling images..." -ForegroundColor Yellow
    docker image prune -f
    
    Write-Host "Removing unused volumes..." -ForegroundColor Yellow
    docker volume prune -f
    
    Write-Host "Removing unused networks..." -ForegroundColor Yellow
    docker network prune -f
    
    Write-Host "Cleanup completed!" -ForegroundColor Green
}

# Show usage if no parameters
if (-not ($CheckPorts -or $CheckDocker -or $CheckImages -or $CleanAll -or $ShowLogs -or $TestConnections -or $All)) {
    Write-Host "`n=== Usage ===" -ForegroundColor Yellow
    Write-Host "  Check all issues:" -ForegroundColor Gray
    Write-Host "    .\scripts\debug-integration-tests.ps1 -All" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  Specific checks:" -ForegroundColor Gray
    Write-Host "    .\scripts\debug-integration-tests.ps1 -CheckDocker" -ForegroundColor Gray
    Write-Host "    .\scripts\debug-integration-tests.ps1 -CheckPorts" -ForegroundColor Gray
    Write-Host "    .\scripts\debug-integration-tests.ps1 -CheckImages" -ForegroundColor Gray
    Write-Host "    .\scripts\debug-integration-tests.ps1 -ShowLogs" -ForegroundColor Gray
    Write-Host "    .\scripts\debug-integration-tests.ps1 -TestConnections" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  Clean up:" -ForegroundColor Gray
    Write-Host "    .\scripts\debug-integration-tests.ps1 -CleanAll" -ForegroundColor Gray
}

Write-Host "`n=== Debug completed ===" -ForegroundColor Green
