#!/usr/bin/env pwsh
# Start the Angular Budget Client with watch mode on the host machine
# Prerequisites: Node.js and npm must be installed

# Set error action preference
$ErrorActionPreference = "Stop"

Write-Host "Starting Budget Client..." -ForegroundColor Green

# Navigate to client directory
$clientPath = Join-Path $PSScriptRoot "..\NVs.Budget.Hosts.Web.Client\budget-client"
Set-Location $clientPath

# Check if node_modules exists
if (-not (Test-Path "node_modules")) {
    Write-Host "node_modules not found. Installing dependencies..." -ForegroundColor Yellow
    npm install
    Write-Host "Dependencies installed successfully!" -ForegroundColor Green
}

# Set environment
$env:NODE_ENV = "development"

Write-Host "`nEnvironment Configuration:" -ForegroundColor Cyan
Write-Host "  Development Server: http://localhost:4200" -ForegroundColor White
Write-Host "  Watch Mode: Enabled" -ForegroundColor White
Write-Host "`nStarting Angular dev server..." -ForegroundColor Green
Write-Host "----------------------------------------`n" -ForegroundColor Gray

# Start Angular dev server with watch
npm start

