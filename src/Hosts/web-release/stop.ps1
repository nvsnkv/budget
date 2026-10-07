#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Stop the Budget application
.DESCRIPTION
    This script stops all services and optionally removes volumes
.PARAMETER RemoveVolumes
    Remove all volumes (database data will be lost!)
.EXAMPLE
    .\stop.ps1
    .\stop.ps1 -RemoveVolumes
#>

param(
    [switch]$RemoveVolumes
)

$ErrorActionPreference = "Stop"

Write-Host "Budget Application - Stopping Services" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

if ($RemoveVolumes) {
    Write-Host "⚠️  WARNING: This will remove all volumes including database data!" -ForegroundColor Red
    $confirm = Read-Host "Are you sure? Type 'yes' to confirm"
    
    if ($confirm -ne "yes") {
        Write-Host "Cancelled" -ForegroundColor Yellow
        exit 0
    }
    
    Write-Host "Stopping services and removing volumes..." -ForegroundColor Yellow
    docker-compose down -v
}
else {
    Write-Host "Stopping services..." -ForegroundColor Cyan
    docker-compose down
}

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to stop services" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "✓ Services stopped successfully" -ForegroundColor Green
