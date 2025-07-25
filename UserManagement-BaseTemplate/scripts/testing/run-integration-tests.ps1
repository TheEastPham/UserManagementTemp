# PowerShell script to run integration tests in Docker
param(
    [string]$Environment = "Testing",
    [string]$ComposeFile = "docker-compose.integration-tests.yml",
    [switch]$Rebuild,
    [switch]$KeepContainers,
    [switch]$ShowLogs,
    [string]$TestFilter = "FullyQualifiedName~Integration"
)

Write-Host "Starting Integration Tests in Docker..." -ForegroundColor Green
Write-Host "Environment: $Environment" -ForegroundColor Yellow
Write-Host "Test Filter: $TestFilter" -ForegroundColor Yellow

# Ensure we're in the correct directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir
Set-Location $RootDir

try {
    # Clean up any existing containers if rebuild is requested
    if ($Rebuild) {
        Write-Host "Cleaning up existing containers..." -ForegroundColor Yellow
        docker-compose -f $ComposeFile down -v --remove-orphans
        docker image prune -f
    }

    # Build and start the services
    Write-Host "Building and starting test services..." -ForegroundColor Yellow
    docker-compose -f $ComposeFile up --build -d sqlserver redis

    # Wait for services to be healthy
    Write-Host "Waiting for services to be healthy..." -ForegroundColor Yellow
    $maxWait = 120
    $elapsed = 0
    
    do {
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
        docker-compose -f $ComposeFile logs
        exit 1
    }

    # Create TestResults directory if it doesn't exist
    if (-not (Test-Path "TestResults")) {
        New-Item -ItemType Directory -Path "TestResults" -Force
    }

    # Run integration tests
    Write-Host "Running integration tests..." -ForegroundColor Yellow
    
    # Update the test filter in the environment
    $env:TEST_FILTER = $TestFilter
    
    docker-compose -f $ComposeFile run --rm integration-tests
    $testExitCode = $LASTEXITCODE

    if ($ShowLogs) {
        Write-Host "=== Integration Test Logs ===" -ForegroundColor Cyan
        docker-compose -f $ComposeFile logs integration-tests
    }

    # Display test results
    if (Test-Path "TestResults") {
        Write-Host "=== Test Results ===" -ForegroundColor Cyan
        Get-ChildItem -Path "TestResults" -Recurse -File | ForEach-Object {
            Write-Host "Found: $($_.FullName)" -ForegroundColor Gray
        }
        
        # Look for TRX files and display summary
        $trxFiles = Get-ChildItem -Path "TestResults" -Filter "*.trx" -Recurse
        if ($trxFiles) {
            Write-Host "Test result files:" -ForegroundColor Cyan
            $trxFiles | ForEach-Object {
                Write-Host "  - $($_.Name)" -ForegroundColor Gray
            }
        }
    }

    # Check test results
    if ($testExitCode -eq 0) {
        Write-Host "Integration tests passed!" -ForegroundColor Green
    } else {
        Write-Host "Integration tests failed!" -ForegroundColor Red
        
        # Show container logs on failure
        Write-Host "=== Container Logs ===" -ForegroundColor Yellow
        docker-compose -f $ComposeFile logs --tail=50
    }

} catch {
    Write-Host "Error running integration tests: $($_.Exception.Message)" -ForegroundColor Red
    $testExitCode = 1
} finally {
    # Clean up containers unless requested to keep them
    if (-not $KeepContainers) {
        Write-Host "Cleaning up containers..." -ForegroundColor Yellow
        docker-compose -f $ComposeFile down -v
    } else {
        Write-Host "Keeping containers running for debugging..." -ForegroundColor Yellow
        Write-Host "To clean up manually, run: docker-compose -f $ComposeFile down -v" -ForegroundColor Gray
    }
}

exit $testExitCode
