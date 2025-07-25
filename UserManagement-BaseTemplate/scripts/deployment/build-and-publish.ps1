# ===========================# Restore packages
# ==============================================================================
Write-Host "📦 Restoring NuGet packages..." -ForegroundColor Yellow
Set-Location $srcPath
dotnet restore Base.sln
if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Package restore failed"
    exit 1
}
# base Platform - Build & Publish Script
# ====================================

param(
    [string]$Configuration = "Release",
    [string]$OutputPath = ".\publish",
    [switch]$SkipTests,
    [switch]$Docker
)

Write-Host "🚀 Building base Platform..." -ForegroundColor Green

# Get script directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
$rootPath = Split-Path -Parent $scriptPath
$srcPath = Join-Path $rootPath "src\Backend"

# Clean previous build
if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Recurse -Force
    Write-Host "✅ Cleaned previous build" -ForegroundColor Green
}

# Create output directories
New-Item -ItemType Directory -Path "$OutputPath\gateway" -Force | Out-Null
New-Item -ItemType Directory -Path "$OutputPath\usermanagement" -Force | Out-Null

# ==============================================================================
# 1. Restore packages
# ==============================================================================
Write-Host "📦 Restoring NuGet packages..." -ForegroundColor Yellow
Set-Location $srcPath
dotnet restore base.sln
if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Package restore failed"
    exit 1
}

# ==============================================================================
# 2. Run tests (if not skipped)
# ==============================================================================
if (-not $SkipTests) {
    Write-Host "🧪 Running tests..." -ForegroundColor Yellow
    $testPath = Join-Path $rootPath "tests"
    dotnet test $testPath --configuration $Configuration --no-restore --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "⚠️ Some tests failed, but continuing with build..."
    } else {
        Write-Host "✅ All tests passed" -ForegroundColor Green
    }
}

# ==============================================================================
# 3. Build projects
# ==============================================================================
Write-Host "🔨 Building projects..." -ForegroundColor Yellow

$projects = @(
    @{
        Name = "API Gateway"
        Path = "Gateways\Base.ApiGateway\Base.ApiGateway.csproj"
        Output = "gateway"
    },
    @{
        Name = "User Management"
        Path = "UserManagement\Base.UserManagement.API\Base.UserManagement.API.csproj"
        Output = "usermanagement"
    }
)

foreach ($project in $projects) {
    Write-Host "  Building $($project.Name)..." -ForegroundColor Cyan
    
    $projectPath = Join-Path $srcPath $project.Path
    $outputPath = Join-Path $rootPath "$OutputPath\$($project.Output)"
    
    dotnet publish $projectPath `
        --configuration $Configuration `
        --output $outputPath `
        --no-restore `
        --verbosity quiet `
        --self-contained false `
        --runtime win-x64
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "❌ Build failed for $($project.Name)"
        exit 1
    }
    
    Write-Host "✅ $($project.Name) built successfully" -ForegroundColor Green
}

# ==============================================================================
# 4. Copy configuration files
# ==============================================================================
Write-Host "📄 Copying configuration files..." -ForegroundColor Yellow

# Copy appsettings for each service
$configFiles = @(
    @{
        Source = Join-Path $srcPath "Gateways\Base.ApiGateway\appsettings.json"
        Dest = Join-Path $rootPath "$OutputPath\gateway\appsettings.json"
    },
    @{
        Source = Join-Path $srcPath "Gateways\Base.ApiGateway\ocelot.json"
        Dest = Join-Path $rootPath "$OutputPath\gateway\ocelot.json"
    },
    @{
        Source = Join-Path $srcPath "UserManagement\Base.UserManagement.API\appsettings.json"
        Dest = Join-Path $rootPath "$OutputPath\usermanagement\appsettings.json"
    }
)

foreach ($config in $configFiles) {
    if (Test-Path $config.Source) {
        Copy-Item $config.Source $config.Dest -Force
        Write-Host "✅ Copied $(Split-Path $config.Source -Leaf)" -ForegroundColor Green
    }
}

# ==============================================================================
# 5. Create deployment package
# ==============================================================================
Write-Host "📦 Creating deployment package..." -ForegroundColor Yellow

$packagePath = Join-Path $rootPath "base-deployment.zip"
if (Test-Path $packagePath) {
    Remove-Item $packagePath -Force
}

# Create zip file
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($OutputPath, $packagePath)
Write-Host "✅ Deployment package created: $packagePath" -ForegroundColor Green

# ==============================================================================
# 6. Docker build (if requested)
# ==============================================================================
if ($Docker) {
    Write-Host "🐳 Building Docker images..." -ForegroundColor Yellow
    
    Set-Location $rootPath
    
    # Build User Management image
    docker build -f docker\Dockerfile.usermanagement -t base/usermanagement:latest .
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ User Management Docker image built" -ForegroundColor Green
    } else {
        Write-Warning "⚠️ Docker build failed for User Management"
    }
    
    # Build API Gateway image (create Dockerfile if needed)
    if (-not (Test-Path "docker\Dockerfile.gateway")) {
        $gatewayDockerfile = @"
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Backend/ApiGateway/base.ApiGateway.csproj", "src/Backend/ApiGateway/"]
COPY ["src/Backend/Shared/base.Shared.csproj", "src/Backend/Shared/"]
RUN dotnet restore "src/Backend/ApiGateway/base.ApiGateway.csproj"
COPY . .
WORKDIR "/src/src/Backend/ApiGateway"
RUN dotnet build "base.ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "base.ApiGateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "base.ApiGateway.dll"]
"@
        $gatewayDockerfile | Out-File -FilePath "docker\Dockerfile.gateway" -Encoding UTF8
    }
    
    docker build -f docker\Dockerfile.gateway -t base/gateway:latest .
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ API Gateway Docker image built" -ForegroundColor Green
    } else {
        Write-Warning "⚠️ Docker build failed for API Gateway"
    }
}

# ==============================================================================
# 7. Generate deployment info
# ==============================================================================
try {
    $gitCommit = git rev-parse HEAD 2>$null
} catch {
    $gitCommit = "Unknown"
}

try {
    $gitBranch = git rev-parse --abbrev-ref HEAD 2>$null
} catch {
    $gitBranch = "Unknown"
}

$deployInfo = @{
    BuildDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Configuration = $Configuration
    GitCommit = $gitCommit
    GitBranch = $gitBranch
    Version = "1.0.0"
    Services = @(
        @{
            Name = "API Gateway"
            Port = 5000
            HealthCheck = "/health"
            Path = "gateway"
        },
        @{
            Name = "User Management"
            Port = 5001
            HealthCheck = "/health"
            Path = "usermanagement"
        }
    )
}

$deployInfo | ConvertTo-Json -Depth 3 | Out-File -FilePath "$OutputPath\deployment-info.json" -Encoding UTF8

# ==============================================================================
# 8. Summary
# ==============================================================================
Write-Host "`n🎉 Build completed successfully!" -ForegroundColor Green
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host "📍 Build artifacts:" -ForegroundColor Yellow
Write-Host "   • Publish folder: $OutputPath" -ForegroundColor White
Write-Host "   • Deployment package: $packagePath" -ForegroundColor White

Write-Host "`n📍 Services built:" -ForegroundColor Yellow
foreach ($project in $projects) {
    Write-Host "   • $($project.Name): $OutputPath\$($project.Output)" -ForegroundColor White
}

Write-Host "`n📍 Next steps:" -ForegroundColor Yellow
Write-Host "   1. Copy the publish folder to your server" -ForegroundColor White
Write-Host "   2. Run the deployment script on the server" -ForegroundColor White
Write-Host "   3. Update database with migrations" -ForegroundColor White
Write-Host "   4. Test the health check endpoints" -ForegroundColor White

if ($Docker) {
    Write-Host "`n📍 Docker images:" -ForegroundColor Yellow
    Write-Host "   • base/gateway:latest" -ForegroundColor White
    Write-Host "   • base/usermanagement:latest" -ForegroundColor White
}

Write-Host "`n===========================================" -ForegroundColor Cyan

# Return to original directory
Set-Location $scriptPath
