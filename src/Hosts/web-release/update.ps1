#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Update Budget application to a new version
.DESCRIPTION
    This script pulls new images and restarts services
.PARAMETER Version
    Version tag to update to (updates .env file)
.EXAMPLE
    .\update.ps1
    .\update.ps1 -Version "1.0.1"
#>

param(
    [string]$Version
)

$ErrorActionPreference = "Stop"

Write-Host "Budget Application - Update" -ForegroundColor Cyan
Write-Host "===========================" -ForegroundColor Cyan
Write-Host ""

# Check if .env exists
if (-not (Test-Path ".env")) {
    Write-Host "❌ .env file not found!" -ForegroundColor Red
    exit 1
}

# Update version in .env if specified
if ($Version) {
    Write-Host "Updating version to: $Version" -ForegroundColor Cyan
    $env_content = Get-Content ".env"
    $updated = $false
    
    $new_content = $env_content | ForEach-Object {
        if ($_ -match "^\s*IMAGE_VERSION\s*=") {
            $updated = $true
            "IMAGE_VERSION=$Version"
        }
        else {
            $_
        }
    }
    
    if (-not $updated) {
        $new_content += "IMAGE_VERSION=$Version"
    }
    
    $new_content | Set-Content ".env"
    Write-Host "✓ Version updated in .env" -ForegroundColor Green
    Write-Host ""
}

# Pull new images
Write-Host "Pulling images..." -ForegroundColor Cyan
docker-compose pull

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to pull images" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Images pulled successfully" -ForegroundColor Green
Write-Host ""

# Restart services
Write-Host "Restarting services..." -ForegroundColor Cyan
docker-compose up -d

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to restart services" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "✓ Update completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Monitor the services with: docker-compose ps" -ForegroundColor Cyan
Write-Host "View logs with: .\logs.ps1 -Follow" -ForegroundColor Cyan
