#!/usr/bin/env pwsh
<#
.SYNOPSIS
    View logs from Budget application services
.DESCRIPTION
    This script displays logs from all or specific services
.PARAMETER Service
    Specific service to view logs for (budget-server, budget-client, nginx, postgres, pgbackups)
.PARAMETER Follow
    Follow log output
.EXAMPLE
    .\logs.ps1
    .\logs.ps1 -Service budget-server
    .\logs.ps1 -Service nginx -Follow
#>

param(
    [string]$Service,
    [switch]$Follow
)

$ErrorActionPreference = "Stop"

$follow_flag = if ($Follow) { "-f" } else { "" }

if ($Service) {
    Write-Host "Viewing logs for: $Service" -ForegroundColor Cyan
    if ($follow_flag) {
        docker-compose logs $follow_flag $Service
    }
    else {
        docker-compose logs $Service
    }
}
else {
    Write-Host "Viewing logs for all services" -ForegroundColor Cyan
    if ($follow_flag) {
        docker-compose logs $follow_flag
    }
    else {
        docker-compose logs
    }
}
