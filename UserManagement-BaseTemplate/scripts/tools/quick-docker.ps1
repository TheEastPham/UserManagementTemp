# ====================================
# Quick Docker Build Script
# ====================================

param(
    [switch]$Build,
    [switch]$Run,
    [switch]$Stop,
    [switch]$Logs,
    [switch]$Clean,
    [switch]$Help
)

# Colors for output
function Write-Info { param([string]$Message) Write-Host $Message -ForegroundColor Cyan }
function Write-Success { param([string]$Message) Write-Host $Message -ForegroundColor Green }
function Write-Warning { param([string]$Message) Write-Host $Message -ForegroundColor Yellow }
function Write-Error { param([string]$Message) Write-Host $Message -ForegroundColor Red }

if ($Help) {
    Write-Info "Quick Docker Build & Run Script"
    Write-Info "=================================="
    Write-Info ""
    Write-Info "USAGE:"
    Write-Info "  .\quick-docker.ps1 [OPTIONS]"
    Write-Info ""
    Write-Info "OPTIONS:"
    Write-Info "  -Build                    Build Docker images"
    Write-Info "  -Run                      Run the application stack"
    Write-Info "  -Stop                     Stop the application stack"
    Write-Info "  -Logs                     Show application logs"
    Write-Info "  -Clean                    Clean up containers and images"
    Write-Info "  -Help                     Show this help message"
    exit 0
}

Write-Info "Base Platform - Quick Docker Management"
Write-Info "=========================================="

# Get script directory and navigate to project root
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
$rootPath = Split-Path -Parent (Split-Path -Parent $scriptPath)

# Set working directory to project root
Set-Location $rootPath

# Check if Docker is running
try {
    docker info > $null 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Docker is not running. Please start Docker and try again."
        exit 1
    }
} catch {
    Write-Error "Docker is not available. Please install Docker and try again."
    exit 1
}

# Build Docker images
if ($Build) {
    Write-Info "Building Docker images..."
    
    try {
        # Build UserManagement API
        Write-Info "  Building UserManagement API..."
        docker build -f docker/Dockerfile.usermanagement -t base/usermanagement:latest .
        if ($LASTEXITCODE -eq 0) {
            Write-Success "UserManagement API image built successfully"
        } else {
            Write-Error "Failed to build UserManagement API image"
            exit 1
        }
        
        # Build API Gateway
        Write-Info "  Building API Gateway..."
        docker build -f docker/Dockerfile.gateway -t base/gateway:latest .
        if ($LASTEXITCODE -eq 0) {
            Write-Success "API Gateway image built successfully"
        } else {
            Write-Error "Failed to build API Gateway image"
            exit 1
        }
        
        # Build Admin Portal
        Write-Info "  Building Admin Portal..."
        docker build -f docker/Dockerfile.adminportal -t base/adminportal:latest .
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Admin Portal image built successfully"
        } else {
            Write-Error "Failed to build Admin Portal image"
            exit 1
        }
        
        Write-Success "All Docker images built successfully!"
        
        # Show images
        Write-Info ""
        Write-Info "Built images:"
        docker images | Where-Object { $_ -match "base/" }
        
    } catch {
        Write-Error "Build failed: $($_.Exception.Message)"
        exit 1
    }
}

# Run application stack
if ($Run) {
    Write-Info "Starting application stack..."
    
    try {
        docker-compose -f docker-compose.local.yml up -d
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Application stack started successfully"
            Write-Info ""
            Write-Info "Services running:"
            Write-Info "  SQL Server: localhost:1433"
            Write-Info "  Redis: localhost:6379"
            Write-Info "  UserManagement API: http://localhost:5001"
            Write-Info "  API Gateway: http://localhost:5000"
            Write-Info "  Admin Portal: http://localhost:3000"
            Write-Info "  Health Checks:"
            Write-Info "    - UserManagement: http://localhost:5001/health"
            Write-Info "    - Gateway: http://localhost:5000/health"
            Write-Warning ""
            Write-Warning "It may take a few minutes for all services to be fully ready."
        } else {
            Write-Error "Failed to start application stack"
            exit 1
        }
    } catch {
        Write-Error "Error starting application: $($_.Exception.Message)"
        exit 1
    }
}

# Stop application stack
if ($Stop) {
    Write-Info "Stopping application stack..."
    
    try {
        docker-compose -f docker-compose.local.yml down
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Application stack stopped successfully"
        } else {
            Write-Error "Failed to stop application stack"
            exit 1
        }
    } catch {
        Write-Error "Error stopping application: $($_.Exception.Message)"
        exit 1
    }
}

# Show logs
if ($Logs) {
    Write-Info "Application logs:"
    
    try {
        docker-compose -f docker-compose.local.yml logs --tail=50 -f
    } catch {
        Write-Error "Error getting logs: $($_.Exception.Message)"
        exit 1
    }
}

# Clean up
if ($Clean) {
    Write-Warning "Cleaning up containers and images..."
    
    try {
        # Stop and remove containers
        docker-compose -f docker-compose.local.yml down -v --remove-orphans
        
        # Remove images
        $images = docker images -q "base/*"
        if ($images) {
            docker rmi $images -f
            Write-Success "Removed Base platform images"
        }
        
        # Prune dangling images
        docker image prune -f
        
        Write-Success "Cleanup completed"
    } catch {
        Write-Error "Error during cleanup: $($_.Exception.Message)"
        exit 1
    }
}

# If no action specified, show status
if (-not ($Build -or $Run -or $Stop -or $Logs -or $Clean)) {
    Write-Warning "No action specified. Use -Help for usage information."
    Write-Info ""
    Write-Info "Current containers:"
    docker-compose -f docker-compose.local.yml ps 2>$null
    Write-Info ""
    Write-Info "Built images:"
    docker images | Where-Object { $_ -match "base/" } 2>$null
}

Write-Success "Docker script completed!"

# Return to original directory
Set-Location $scriptPath
