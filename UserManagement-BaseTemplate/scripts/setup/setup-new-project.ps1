# Setup script for new project
param(
    [Parameter(Mandatory=$true)]
    [string]$ProjectName,
    [string]$CompanyName = "MyCompany",
    [string]$Namespace = ""
)

if ([string]::IsNullOrEmpty($Namespace)) {
    $Namespace = "$CompanyName.$ProjectName"
}

Write-Host "Setting up new project: $ProjectName" -ForegroundColor Green
Write-Host "Company: $CompanyName" -ForegroundColor Yellow
Write-Host "Namespace: $Namespace" -ForegroundColor Yellow

# Replace placeholders in all files
Get-ChildItem -Recurse -File | ForEach-Object {
    try {
        $content = Get-Content $_.FullName -Raw -ErrorAction SilentlyContinue
        if ($content) {
            $updated = $content 
                -replace "{{ProjectName}}", $ProjectName 
                -replace "{{CompanyName}}", $CompanyName 
                -replace "{{Namespace}}", $Namespace 
                -replace "base\.UserManagement", "$Namespace.UserManagement" 
                -replace "base-UserManagement", "$ProjectName-UserManagement"
            
            if ($updated -ne $content) {
                Set-Content $_.FullName $updated -NoNewline
                Write-Host "Updated: $($_.FullName)" -ForegroundColor Gray
            }
        }
    }
    catch {
        Write-Warning "Could not process file: $($_.FullName)"
    }
}

# Rename directories
Get-ChildItem -Recurse -Directory | Where-Object { $_.Name -like "*{{ProjectName}}*" -or $_.Name -like "*base*" } | ForEach-Object {
    $newName = $_.Name -replace "{{ProjectName}}", $ProjectName -replace "base\.UserManagement", "$Namespace.UserManagement"
    if ($newName -ne $_.Name) {
        Rename-Item $_.FullName $newName
        Write-Host "Renamed directory: $($_.Name) -> $newName" -ForegroundColor Green
    }
}

# Rename files
Get-ChildItem -Recurse -File | Where-Object { $_.Name -like "*{{ProjectName}}*" -or $_.Name -like "*base*" } | ForEach-Object {
    $newName = $_.Name -replace "{{ProjectName}}", $ProjectName -replace "base\.UserManagement", "$Namespace.UserManagement"
    if ($newName -ne $_.Name) {
        Rename-Item $_.FullName $newName
        Write-Host "Renamed file: $($_.Name) -> $newName" -ForegroundColor Green
    }
}

Write-Host "
Project setup completed!" -ForegroundColor Green
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Copy .env.template to .env and configure" -ForegroundColor White
Write-Host "2. Run: docker-compose up -d" -ForegroundColor White
Write-Host "3. Run: .\scripts\setup-database.ps1" -ForegroundColor White
Write-Host "4. Start your development!" -ForegroundColor White
