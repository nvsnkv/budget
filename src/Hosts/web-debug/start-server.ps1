#!/usr/bin/env pwsh
# Start the .NET Budget Server with watch mode on the host machine
# Prerequisites: Docker Compose must be running (postgres and dev-certs services)

param(
    [string]$CertsPath = ".\certs"
)

# Set error action preference
$ErrorActionPreference = "Stop"

Write-Host "Starting Budget Server..." -ForegroundColor Green

# Create certs directory if it doesn't exist
if (-not (Test-Path $CertsPath)) {
    New-Item -ItemType Directory -Path $CertsPath -Force | Out-Null
}

# Get absolute path for certs
$CertsAbsPath = (Resolve-Path $CertsPath).Path

# Check if certs exist, if not extract from Docker volume
$certFile = Join-Path $CertsAbsPath "aspnetapp.pfx"
if (-not (Test-Path $certFile)) {
    Write-Host "Extracting certificates from Docker volume..." -ForegroundColor Yellow
    
    # Ensure dev-certs container has run
    docker compose up dev-certs --build
    
    # Extract certs from Docker volume
    docker compose run --rm dev-certs sh -c "cp /https/* /tmp/" 2>&1 | Out-Null
    
    # Use a temporary container to copy files from the volume
    $containerId = docker create -v "web-debug_certs:/https" alpine
    docker cp "${containerId}:/https/." $CertsAbsPath
    docker rm $containerId | Out-Null
    
    Write-Host "Certificates extracted to $CertsAbsPath" -ForegroundColor Green
}

# Load Yandex Auth credentials from server.env
$serverEnvPath = Join-Path $PSScriptRoot "server.env"
if (Test-Path $serverEnvPath) {
    Get-Content $serverEnvPath | ForEach-Object {
        if ($_ -match '^\s*([^#][^=]*?)\s*=\s*(.+?)\s*$') {
            $key = $matches[1].Trim()
            $value = $matches[2].Trim()
            [Environment]::SetEnvironmentVariable($key, $value, "Process")
            Write-Host "Loaded: $key" -ForegroundColor Cyan
        }
    }
} else {
    Write-Host "Warning: server.env not found at $serverEnvPath" -ForegroundColor Yellow
}

# Set environment variables
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ConnectionStrings__BudgetContext = "Host=localhost;Port=20000;Database=budgetdb;Username=postgres;Password=postgres"
$env:ConnectionStrings__IdentityContext = "Host=localhost;Port=20000;Database=budgetdb;Username=postgres;Password=postgres"
$env:ASPNETCORE_Kestrel__Certificates__Default__Path = $certFile
$env:ASPNETCORE_Kestrel__Certificates__Default__Password = "dev-password-do-not-use-in-production"

# Navigate to server directory
$serverPath = Join-Path $PSScriptRoot "..\NVs.Budget.Hosts.Web.Server"
Set-Location $serverPath

Write-Host "`nEnvironment Configuration:" -ForegroundColor Cyan
Write-Host "  Database: localhost:20000/budgetdb" -ForegroundColor White
Write-Host "  HTTPS Certificate: $certFile" -ForegroundColor White
Write-Host "  Launch Profile: https (7237, 5153)" -ForegroundColor White
Write-Host "`nStarting dotnet watch..." -ForegroundColor Green
Write-Host "----------------------------------------`n" -ForegroundColor Gray

# Run dotnet watch
dotnet watch run --project NVs.Budget.Hosts.Web.Server.csproj --launch-profile https

