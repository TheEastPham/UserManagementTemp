# ========================================================================
# Monitoring Stack Setup Script
# ========================================================================

param(
    [switch]$Start,
    [switch]$Stop,
    [switch]$Restart,
    [switch]$Status,
    [switch]$Logs,
    [string]$Service = "",
    [switch]$Help
)

# Colors for output
function Write-Info { param([string]$Message) Write-Host $Message -ForegroundColor Cyan }
function Write-Success { param([string]$Message) Write-Host $Message -ForegroundColor Green }
function Write-Warning { param([string]$Message) Write-Host $Message -ForegroundColor Yellow }
function Write-Error { param([string]$Message) Write-Host $Message -ForegroundColor Red }

if ($Help) {
    Write-Info "📊 Monitoring Stack Management"
    Write-Info "==============================="
    Write-Info ""
    Write-Info "USAGE:"
    Write-Info "  .\setup-monitoring.ps1 [OPTIONS]"
    Write-Info ""
    Write-Info "OPTIONS:"
    Write-Info "  -Start                    Start the monitoring stack"
    Write-Info "  -Stop                     Stop the monitoring stack"
    Write-Info "  -Restart                  Restart the monitoring stack"
    Write-Info "  -Status                   Show status of monitoring services"
    Write-Info "  -Logs                     Show logs from monitoring services"
    Write-Info "  -Service [name]           Target specific service for logs/status"
    Write-Info "  -Help                     Show this help message"
    Write-Info ""
    Write-Info "SERVICES:"
    Write-Info "  • prometheus             Metrics collection"
    Write-Info "  • grafana               Visualization dashboard" 
    Write-Info "  • jaeger                Distributed tracing"
    Write-Info "  • otel-collector        OpenTelemetry collector"
    Write-Info ""
    Write-Info "ENDPOINTS:"
    Write-Info "  • Prometheus: http://localhost:9090"
    Write-Info "  • Grafana: http://localhost:3001 (admin/admin)"
    Write-Info "  • Jaeger: http://localhost:16686"
    exit 0
}

Write-Info "📊 Base Platform - Monitoring Stack"
Write-Info "===================================="

# Get script directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
$rootPath = Split-Path -Parent $scriptPath

# Set working directory
Set-Location $rootPath

# Check if Docker is running
try {
    docker info > $null 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "❌ Docker is not running. Please start Docker and try again."
        exit 1
    }
} catch {
    Write-Error "❌ Docker is not available. Please install Docker and try again."
    exit 1
}

# Start monitoring stack
if ($Start) {
    Write-Info "🚀 Starting monitoring stack..."
    
    try {
        docker-compose -f docker-compose.monitoring.yml up -d
        if ($LASTEXITCODE -eq 0) {
            Write-Success "✅ Monitoring stack started successfully"
            Write-Info ""
            Write-Info "📍 Available endpoints:"
            Write-Info "  • Prometheus: http://localhost:9090"
            Write-Info "  • Grafana: http://localhost:3001 (admin/admin)"
            Write-Info "  • Jaeger: http://localhost:16686"
            Write-Info ""
            Write-Warning "ℹ️  It may take a few minutes for all services to be fully ready."
        } else {
            Write-Error "❌ Failed to start monitoring stack"
            exit 1
        }
    } catch {
        Write-Error "❌ Error starting monitoring stack: $($_.Exception.Message)"
        exit 1
    }
}

# Stop monitoring stack
if ($Stop) {
    Write-Info "🛑 Stopping monitoring stack..."
    
    try {
        docker-compose -f docker-compose.monitoring.yml down
        if ($LASTEXITCODE -eq 0) {
            Write-Success "✅ Monitoring stack stopped successfully"
        } else {
            Write-Error "❌ Failed to stop monitoring stack"
            exit 1
        }
    } catch {
        Write-Error "❌ Error stopping monitoring stack: $($_.Exception.Message)"
        exit 1
    }
}

# Restart monitoring stack
if ($Restart) {
    Write-Info "🔄 Restarting monitoring stack..."
    
    try {
        docker-compose -f docker-compose.monitoring.yml restart
        if ($LASTEXITCODE -eq 0) {
            Write-Success "✅ Monitoring stack restarted successfully"
        } else {
            Write-Error "❌ Failed to restart monitoring stack"
            exit 1
        }
    } catch {
        Write-Error "❌ Error restarting monitoring stack: $($_.Exception.Message)"
        exit 1
    }
}

# Show status
if ($Status) {
    Write-Info "📋 Monitoring stack status:"
    
    try {
        if ([string]::IsNullOrEmpty($Service)) {
            docker-compose -f docker-compose.monitoring.yml ps
        } else {
            docker-compose -f docker-compose.monitoring.yml ps $Service
        }
    } catch {
        Write-Error "❌ Error getting status: $($_.Exception.Message)"
        exit 1
    }
}

# Show logs
if ($Logs) {
    Write-Info "📝 Monitoring stack logs:"
    
    try {
        if ([string]::IsNullOrEmpty($Service)) {
            docker-compose -f docker-compose.monitoring.yml logs --tail=50 -f
        } else {
            docker-compose -f docker-compose.monitoring.yml logs --tail=50 -f $Service
        }
    } catch {
        Write-Error "❌ Error getting logs: $($_.Exception.Message)"
        exit 1
    }
}

# If no action specified, show status
if (-not ($Start -or $Stop -or $Restart -or $Status -or $Logs)) {
    Write-Warning "⚠️ No action specified. Use -Help for usage information."
    Write-Info ""
    Write-Info "📋 Current status:"
    docker-compose -f docker-compose.monitoring.yml ps 2>$null
}

Write-Success "🎉 Monitoring script completed!"
