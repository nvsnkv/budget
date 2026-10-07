#!/usr/bin/env pwsh
# Stop all Budget application services

Write-Host "Stopping all Budget application services..." -ForegroundColor Yellow

# Navigate to script directory
Set-Location $PSScriptRoot

# Stop Docker services
Write-Host "Stopping Docker containers..." -ForegroundColor Cyan
docker compose down

Write-Host "`nDocker services stopped!" -ForegroundColor Green
Write-Host "Note: Server and client processes in other windows need to be stopped manually (Ctrl+C)." -ForegroundColor Yellow

