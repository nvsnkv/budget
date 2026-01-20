#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Start the Budget application in production mode
.DESCRIPTION
    This script starts all services using docker-compose
.PARAMETER Pull
    Pull latest images before starting
.EXAMPLE
    .\start.ps1
    .\start.ps1 -Pull
#>

param(
    [switch]$Pull
)

$ErrorActionPreference = "Stop"

Write-Host "Budget Application - Production Start" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Check if .env exists
if (-not (Test-Path ".env")) {
    Write-Host "❌ .env file not found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please create .env file from .env.example:" -ForegroundColor Yellow
    Write-Host "  cp .env.example .env" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Then edit .env and configure required variables:" -ForegroundColor Yellow
    Write-Host "  - POSTGRES_PASSWORD" -ForegroundColor Yellow
    Write-Host "  - BACKUP_PATH" -ForegroundColor Yellow
    Write-Host "  - DOCKER_REGISTRY" -ForegroundColor Yellow
    Write-Host "  - DOCKER_NAMESPACE" -ForegroundColor Yellow
    Write-Host "  - YANDEX_OAUTH_CLIENT_ID" -ForegroundColor Yellow
    Write-Host "  - YANDEX_OAUTH_CLIENT_SECRET" -ForegroundColor Yellow
    exit 1
}

# Validate required environment variables
Write-Host "✓ Validating environment configuration..." -ForegroundColor Green
$env_content = Get-Content ".env"
$required_vars = @(
    "POSTGRES_PASSWORD",
    "BACKUP_PATH",
    "DOCKER_REGISTRY",
    "DOCKER_NAMESPACE",
    "YANDEX_OAUTH_CLIENT_ID",
    "YANDEX_OAUTH_CLIENT_SECRET"
)

$missing_vars = @()
foreach ($var in $required_vars) {
    $found = $env_content | Where-Object { $_ -match "^\s*$var\s*=" -and $_ -notmatch "^\s*#" }
    if (-not $found) {
        $missing_vars += $var
    }
}

if ($missing_vars.Count -gt 0) {
    Write-Host "❌ Missing required environment variables:" -ForegroundColor Red
    foreach ($var in $missing_vars) {
        Write-Host "  - $var" -ForegroundColor Red
    }
    Write-Host ""
    Write-Host "Please update your .env file with these values." -ForegroundColor Yellow
    exit 1
}

# Validate backup path exists
$backup_path_line = $env_content | Where-Object { $_ -match "^\s*BACKUP_PATH\s*=" -and $_ -notmatch "^\s*#" } | Select-Object -First 1
if ($backup_path_line) {
    $backup_path = ($backup_path_line -split "=", 2)[1].Trim()
    if ($backup_path -and $backup_path -ne "/path/to/your/backups") {
        if (-not (Test-Path $backup_path)) {
            Write-Host "⚠️  Backup path does not exist: $backup_path" -ForegroundColor Yellow
            $create = Read-Host "Create it now? (y/n)"
            if ($create -eq "y") {
                New-Item -ItemType Directory -Path $backup_path -Force | Out-Null
                Write-Host "✓ Backup directory created" -ForegroundColor Green
            }
            else {
                Write-Host "❌ Cannot continue without backup directory" -ForegroundColor Red
                exit 1
            }
        }
    }
}

Write-Host "✓ Environment configuration valid" -ForegroundColor Green
Write-Host ""

# Pull images if requested
if ($Pull) {
    Write-Host "Pulling latest images..." -ForegroundColor Cyan
    docker-compose pull
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to pull images" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ Images pulled successfully" -ForegroundColor Green
    Write-Host ""
}

# Start services
Write-Host "Starting services..." -ForegroundColor Cyan
docker-compose up -d

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to start services" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "✓ Services started successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Waiting for services to be healthy..."

# Wait for services to be healthy
$max_wait = 60
$waited = 0
$all_healthy = $false

while ($waited -lt $max_wait -and -not $all_healthy) {
    Start-Sleep -Seconds 2
    $waited += 2
    
    $health = docker-compose ps --format json | ConvertFrom-Json
    $unhealthy = $health | Where-Object { $_.Health -ne "healthy" -and $_.Service -ne "cert-generator" }
    
    if ($unhealthy.Count -eq 0) {
        $all_healthy = $true
    }
    else {
        Write-Host "." -NoNewline
    }
}

Write-Host ""
Write-Host ""

if ($all_healthy) {
    Write-Host "✓ All services are healthy!" -ForegroundColor Green
}
else {
    Write-Host "⚠️  Some services may still be starting up" -ForegroundColor Yellow
    Write-Host "   Run 'docker-compose ps' to check status" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Application is available at:" -ForegroundColor Cyan

# Get ports from .env or use defaults
$client_port = ($env_content | Where-Object { $_ -match "^\s*CLIENT_PORT\s*=" } | Select-Object -First 1)
if ($client_port) {
    $client_port = ($client_port -split "=", 2)[1].Trim()
}
else {
    $client_port = "25000"
}

$server_port = ($env_content | Where-Object { $_ -match "^\s*SERVER_PORT\s*=" } | Select-Object -First 1)
if ($server_port) {
    $server_port = ($server_port -split "=", 2)[1].Trim()
}
else {
    $server_port = "25001"
}

Write-Host "  Client (Frontend): https://localhost:$client_port" -ForegroundColor Green
Write-Host "  Server (API):      https://localhost:$server_port" -ForegroundColor Green
Write-Host ""
Write-Host "Note: Accept the self-signed certificate warning in your browser" -ForegroundColor Yellow
Write-Host "Both services use Kestrel with HTTPS and shared SSL certificates" -ForegroundColor Cyan
Write-Host ""
Write-Host "To view logs: docker-compose logs -f" -ForegroundColor Cyan
Write-Host "To stop:      docker-compose down" -ForegroundColor Cyan
