#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds and releases Docker images for Budget application (Client and Server)

.DESCRIPTION
    This script builds Docker images for both the client and server components,
    optionally tags them with version information, and pushes them to a Docker registry.

.PARAMETER Version
    Version tag for the images (e.g., "1.0.0", "latest"). Default is "latest"

.PARAMETER SkipBuild
    Skip building images and only push existing ones

.PARAMETER SkipPush
    Build images but skip pushing to registry

.PARAMETER ConfigureRegistry
    Force reconfiguration of Docker registry settings

.EXAMPLE
    .\Release-DockerImages.ps1
    Builds and pushes images with version "latest"

.EXAMPLE
    .\Release-DockerImages.ps1 -Version "1.2.3"
    Builds and pushes images with version "1.2.3"

.EXAMPLE
    .\Release-DockerImages.ps1 -SkipPush
    Builds images locally without pushing to registry
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [string]$Version = "latest",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipBuild,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipPush,
    
    [Parameter(Mandatory=$false)]
    [switch]$ConfigureRegistry
)

# Script configuration
$ErrorActionPreference = "Stop"
$ConfigFile = Join-Path $PSScriptRoot "docker-registry-config.json"

# Color functions for better output
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$ForegroundColor = "White"
    )
    Write-Host $Message -ForegroundColor $ForegroundColor
}

function Write-Success {
    param([string]$Message)
    Write-ColorOutput "✓ $Message" "Green"
}

function Write-Info {
    param([string]$Message)
    Write-ColorOutput "ℹ $Message" "Cyan"
}

function Write-Warning {
    param([string]$Message)
    Write-ColorOutput "⚠ $Message" "Yellow"
}

function Write-ErrorMessage {
    param([string]$Message)
    Write-ColorOutput "✗ $Message" "Red"
}

function Write-Step {
    param([string]$Message)
    Write-ColorOutput "`n==> $Message" "Magenta"
}

# Load or create registry configuration
function Get-RegistryConfig {
    if (Test-Path $ConfigFile) {
        try {
            $config = Get-Content $ConfigFile -Raw | ConvertFrom-Json
            return $config
        }
        catch {
            Write-Warning "Failed to read config file. Will create new configuration."
            return $null
        }
    }
    return $null
}

# Save registry configuration
function Save-RegistryConfig {
    param($Config)
    
    try {
        $Config | ConvertTo-Json | Set-Content $ConfigFile
        Write-Success "Registry configuration saved to: $ConfigFile"
    }
    catch {
        Write-ErrorMessage "Failed to save configuration: $_"
    }
}

# Configure Docker registry
function Initialize-RegistryConfig {
    Write-Step "Docker Registry Configuration"
    
    Write-Info "Please provide Docker registry information:"
    Write-Info "(Press Enter to skip and use local images only)"
    Write-Host ""
    
    $registry = Read-Host "Docker Registry URL (e.g., docker.io, ghcr.io, registry.example.com)"
    
    if ([string]::IsNullOrWhiteSpace($registry)) {
        Write-Warning "No registry configured. Images will only be built locally."
        return $null
    }
    
    $username = Read-Host "Registry Username"
    $securePassword = Read-Host "Registry Password" -AsSecureString
    
    # Convert SecureString to encrypted string for storage
    $encryptedPassword = ConvertFrom-SecureString $securePassword
    
    $namespace = Read-Host "Image Namespace/Repository (e.g., mycompany/budget)"
    
    $config = @{
        Registry = $registry
        Username = $username
        EncryptedPassword = $encryptedPassword
        Namespace = $namespace
        ConfiguredDate = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    }
    
    Save-RegistryConfig -Config $config
    return $config
}

# Docker login
function Connect-DockerRegistry {
    param($Config)
    
    if (-not $Config) {
        return $false
    }
    
    try {
        Write-Step "Logging into Docker Registry"
        
        $securePassword = ConvertTo-SecureString $Config.EncryptedPassword
        $credential = New-Object System.Management.Automation.PSCredential($Config.Username, $securePassword)
        $plainPassword = $credential.GetNetworkCredential().Password
        
        $plainPassword | docker login $Config.Registry --username $Config.Username --password-stdin
        
        if ($LASTEXITCODE -ne 0) {
            Write-ErrorMessage "Docker login failed"
            return $false
        }
        
        Write-Success "Successfully logged into $($Config.Registry)"
        return $true
    }
    catch {
        Write-ErrorMessage "Failed to login to Docker registry: $_"
        return $false
    }
}

# Build Docker image
function Build-DockerImage {
    param(
        [string]$Name,
        [string]$DockerfilePath,
        [string]$Context,
        [string]$Tag
    )
    
    Write-Step "Building $Name"
    Write-Info "Dockerfile: $DockerfilePath"
    Write-Info "Context: $Context"
    Write-Info "Tag: $Tag"
    
    try {
        docker build -f $DockerfilePath -t $Tag $Context
        
        if ($LASTEXITCODE -ne 0) {
            Write-ErrorMessage "Failed to build $Name"
            return $false
        }
        
        Write-Success "Successfully built $Name"
        return $true
    }
    catch {
        Write-ErrorMessage "Error building $Name : $_"
        return $false
    }
}

# Tag and push image
function Publish-DockerImage {
    param(
        [string]$LocalTag,
        [string]$RemoteTag,
        [string]$Name
    )
    
    Write-Step "Publishing $Name"
    
    try {
        # Tag image for registry
        Write-Info "Tagging image: $LocalTag -> $RemoteTag"
        docker tag $LocalTag $RemoteTag
        
        if ($LASTEXITCODE -ne 0) {
            Write-ErrorMessage "Failed to tag $Name"
            return $false
        }
        
        # Push to registry
        Write-Info "Pushing image: $RemoteTag"
        docker push $RemoteTag
        
        if ($LASTEXITCODE -ne 0) {
            Write-ErrorMessage "Failed to push $Name"
            return $false
        }
        
        Write-Success "Successfully pushed $Name to registry"
        return $true
    }
    catch {
        Write-ErrorMessage "Error publishing $Name : $_"
        return $false
    }
}

# Main script execution
function Main {
    Write-ColorOutput @"
╔═══════════════════════════════════════════════════════════╗
║                                                           ║
║        Budget Application - Docker Release Script         ║
║                                                           ║
╚═══════════════════════════════════════════════════════════╝
"@ "Cyan"
    
    Write-Info "Version: $Version"
    Write-Info "Skip Build: $SkipBuild"
    Write-Info "Skip Push: $SkipPush"
    Write-Host ""
    
    # Determine paths
    $scriptDir = $PSScriptRoot
    $hostsDir = $scriptDir
    $srcDir = Split-Path -Parent $hostsDir
    $repoRoot = Split-Path -Parent $srcDir
    
    Write-Info "Repository Root: $repoRoot"
    Write-Info "Source Directory: $srcDir"
    Write-Host ""
    
    # Check if Docker is available
    try {
        docker --version | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw "Docker is not available"
        }
    }
    catch {
        Write-ErrorMessage "Docker is not installed or not in PATH"
        exit 1
    }
    
    # Load or configure registry
    $config = Get-RegistryConfig
    
    if ($ConfigureRegistry -or (-not $config -and -not $SkipPush)) {
        $config = Initialize-RegistryConfig
    }
    
    # Define image names
    $serverImageName = "budget-server"
    $clientImageName = "budget-client"
    
    # Local tags
    $serverLocalTag = "${serverImageName}:${Version}"
    $clientLocalTag = "${clientImageName}:${Version}"
    
    # Remote tags (if registry is configured)
    $serverRemoteTag = $null
    $clientRemoteTag = $null
    
    if ($config -and $config.Registry) {
        $registryPrefix = "$($config.Registry)"
        if ($config.Namespace) {
            $registryPrefix = "$registryPrefix/$($config.Namespace)"
        }
        $serverRemoteTag = "${registryPrefix}/${serverImageName}:${Version}"
        $clientRemoteTag = "${registryPrefix}/${clientImageName}:${Version}"
    }
    
    # Build images
    if (-not $SkipBuild) {
        Write-Step "Starting Docker Image Build Process"
        
        # Build Server
        $serverDockerfile = Join-Path $hostsDir "NVs.Budget.Hosts.Web.Server\Dockerfile"
        $serverSuccess = Build-DockerImage `
            -Name "Server" `
            -DockerfilePath $serverDockerfile `
            -Context $repoRoot `
            -Tag $serverLocalTag
        
        if (-not $serverSuccess) {
            Write-ErrorMessage "Server build failed. Aborting."
            exit 1
        }
        
        # Build Client
        $clientDockerfile = Join-Path $hostsDir "NVs.Budget.Hosts.Web.Client\Dockerfile"
        $clientContext = Join-Path $hostsDir "NVs.Budget.Hosts.Web.Client"
        $clientSuccess = Build-DockerImage `
            -Name "Client" `
            -DockerfilePath $clientDockerfile `
            -Context $clientContext `
            -Tag $clientLocalTag
        
        if (-not $clientSuccess) {
            Write-ErrorMessage "Client build failed. Aborting."
            exit 1
        }
        
        Write-Success "`nAll images built successfully!"
        
        # Display local images
        Write-Step "Local Images Built"
        docker images | Select-String -Pattern "(REPOSITORY|$serverImageName|$clientImageName)"
    }
    else {
        Write-Warning "Skipping build phase"
    }
    
    # Push images to registry
    if (-not $SkipPush -and $config -and $config.Registry) {
        # Login to registry
        $loginSuccess = Connect-DockerRegistry -Config $config
        
        if (-not $loginSuccess) {
            Write-Warning "Failed to login to registry. Skipping push."
        }
        else {
            # Push Server
            $serverPushSuccess = Publish-DockerImage `
                -LocalTag $serverLocalTag `
                -RemoteTag $serverRemoteTag `
                -Name "Server"
            
            # Push Client
            $clientPushSuccess = Publish-DockerImage `
                -LocalTag $clientLocalTag `
                -RemoteTag $clientRemoteTag `
                -Name "Client"
            
            if ($serverPushSuccess -and $clientPushSuccess) {
                Write-Success "`nAll images published successfully!"
                Write-Host ""
                Write-Info "Server Image: $serverRemoteTag"
                Write-Info "Client Image: $clientRemoteTag"
            }
            else {
                Write-ErrorMessage "`nSome images failed to publish"
                exit 1
            }
        }
    }
    elseif (-not $SkipPush -and (-not $config -or -not $config.Registry)) {
        Write-Warning "`nNo registry configured. Images are available locally only."
        Write-Info "Run with -ConfigureRegistry to set up Docker registry."
    }
    else {
        Write-Warning "`nSkipping push phase"
    }
    
    Write-Host ""
    Write-Step "Release Process Complete"
    Write-Host ""
    Write-Success "Local images are tagged as:"
    Write-Host "  - $serverLocalTag"
    Write-Host "  - $clientLocalTag"
    
    if ($config -and $config.Registry -and -not $SkipPush) {
        Write-Host ""
        Write-Success "Remote images are available at:"
        Write-Host "  - $serverRemoteTag"
        Write-Host "  - $clientRemoteTag"
    }
    
    Write-Host ""
}

# Run main function
try {
    Main
}
catch {
    Write-ErrorMessage "An unexpected error occurred: $_"
    Write-ErrorMessage $_.ScriptStackTrace
    exit 1
}
