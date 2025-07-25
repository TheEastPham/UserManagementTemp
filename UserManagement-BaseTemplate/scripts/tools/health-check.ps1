# ========================================
# Quick Health Check Script
# ========================================

param(
    [string]$BaseUrl = "localhost",
    [int]$GatewayPort = 5000,
    [int]$UserMgmtPort = 5001,
    [switch]$Detailed,
    [switch]$Continuous,
    [int]$Interval = 30
)

Write-Host "🔍 base Platform Health Check" -ForegroundColor Green
Write-Host "===================================" -ForegroundColor Cyan

function Test-Endpoint {
    param($Url, $ServiceName)
    
    try {
        $response = Invoke-RestMethod -Uri $Url -TimeoutSec 10 -ErrorAction Stop
        
        if ($response.status -eq "Healthy") {
            Write-Host "✅ $ServiceName - Healthy" -ForegroundColor Green
            if ($Detailed -and $response.checks) {
                foreach ($check in $response.checks) {
                    $status = if ($check.status -eq "Healthy") { "✅" } else { "❌" }
                    Write-Host "    $status $($check.name): $($check.status)" -ForegroundColor White
                }
            }
            return $true
        } else {
            Write-Host "⚠️ $ServiceName - $($response.status)" -ForegroundColor Yellow
            return $false
        }
    } catch {
        Write-Host "❌ $ServiceName - Failed: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Run-HealthCheck {
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "`n[$timestamp] Checking services..." -ForegroundColor Cyan
    
    $services = @(
        @{
            Name = "API Gateway"
            Url = "http://$BaseUrl`:$GatewayPort/api/health$(if ($Detailed) { '/detailed' })"
        },
        @{
            Name = "User Management"
            Url = "http://$BaseUrl`:$UserMgmtPort/api/health$(if ($Detailed) { '/detailed' })"
        }
    )
    
    $allHealthy = $true
    foreach ($service in $services) {
        $healthy = Test-Endpoint -Url $service.Url -ServiceName $service.Name
        if (-not $healthy) {
            $allHealthy = $false
        }
    }
    
    if ($allHealthy) {
        Write-Host "`n🎉 All services are healthy!" -ForegroundColor Green
    } else {
        Write-Host "`n⚠️ Some services have issues!" -ForegroundColor Yellow
    }
    
    return $allHealthy
}

# Run health check
if ($Continuous) {
    Write-Host "🔄 Running continuous health check (every $Interval seconds)..." -ForegroundColor Yellow
    Write-Host "Press Ctrl+C to stop`n" -ForegroundColor Gray
    
    while ($true) {
        Run-HealthCheck
        Start-Sleep -Seconds $Interval
    }
} else {
    Run-HealthCheck
}
