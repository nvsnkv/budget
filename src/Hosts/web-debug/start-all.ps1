#!/usr/bin/env pwsh
# Master script to start all Budget application services
# This script starts Docker dependencies and runs server/client on the host

# Set error action preference
$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Budget Application - Development Mode" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Navigate to script directory
Set-Location $PSScriptRoot

# Step 1: Start Docker services (postgres and dev-certs)
Write-Host "[1/4] Starting Docker services (postgres, dev-certs)..." -ForegroundColor Yellow
docker compose up -d postgres dev-certs
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to start Docker services!" -ForegroundColor Red
    exit 1
}
Write-Host "Docker services started successfully!`n" -ForegroundColor Green

# Step 2: Wait for postgres to be ready
Write-Host "[2/4] Waiting for PostgreSQL to be ready..." -ForegroundColor Yellow
$maxAttempts = 30
$attempt = 0
$ready = $false

while (-not $ready -and $attempt -lt $maxAttempts) {
    $attempt++
    try {
        $result = docker exec (docker compose ps -q postgres) pg_isready -U postgres 2>&1
        if ($result -match "accepting connections") {
            $ready = $true
        }
    }
    catch {
        # Ignore errors during connection attempts
    }
    
    if (-not $ready) {
        Write-Host "  Attempt $attempt/$maxAttempts - Waiting..." -ForegroundColor Gray
        Start-Sleep -Seconds 1
    }
}

if (-not $ready) {
    Write-Host "PostgreSQL failed to start within timeout!" -ForegroundColor Red
    exit 1
}
Write-Host "PostgreSQL is ready!`n" -ForegroundColor Green

# Step 3: Extract certificates if needed
Write-Host "[3/4] Setting up SSL certificates..." -ForegroundColor Yellow
$certsPath = Join-Path $PSScriptRoot "certs"
if (-not (Test-Path $certsPath)) {
    New-Item -ItemType Directory -Path $certsPath -Force | Out-Null
}

$certFile = Join-Path $certsPath "aspnetapp.pfx"
if (-not (Test-Path $certFile)) {
    Write-Host "  Extracting certificates from Docker volume..." -ForegroundColor Gray
    
    # Wait for dev-certs to complete
    Start-Sleep -Seconds 2
    
    # Copy certs from Docker volume
    $containerId = docker create -v "web-debug_certs:/https" alpine
    docker cp "${containerId}:/https/." $certsPath | Out-Null
    docker rm $containerId | Out-Null
    
    Write-Host "  Certificates extracted successfully!" -ForegroundColor Green
} else {
    Write-Host "  Certificates already exist." -ForegroundColor Green
}
Write-Host ""

# Step 4: Start server and client
Write-Host "[4/4] Starting server and client..." -ForegroundColor Yellow
Write-Host @"

================================================================================
SERVICES STARTING
================================================================================

The following services will start in separate windows:
  - .NET Server (https://localhost:7237, http://localhost:5153)
  - Angular Client (http://localhost:4200)

Docker Services Running:
  - PostgreSQL (localhost:20000)

Press Ctrl+C in any window to stop that service.
To stop all services, close all windows and run: docker compose down

================================================================================

"@ -ForegroundColor Cyan

$serverScript = Join-Path $PSScriptRoot "start-server.ps1"
 wt --window 0 sp -H pwsh $serverScript

$clientScript = Join-Path $PSScriptRoot "start-client.ps1"
pwsh $clientScript

