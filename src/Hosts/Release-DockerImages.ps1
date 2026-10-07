#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds and releases the Docker image for the Budget application (server with embedded client)

.DESCRIPTION
    This script builds the budget-server Docker image (ASP.NET Core app with the
    Angular client embedded in wwwroot), optionally tags it with version information,
    and pushes it to a Docker registry.

.PARAMETER Version
    Version tag for the image (e.g., "1.0.0", "2026.1.1"). If not specified, automatically generates next version in format Year.Month.Number (e.g., 2026.1.1, 2026.1.2)

.PARAMETER SkipBuild
    Skip building images and only push existing ones

.PARAMETER SkipPush
    Build images but skip pushing to registry

.PARAMETER ConfigureRegistry
    Force reconfiguration of Docker registry settings

.PARAMETER CleanupOldImages
    Remove old Docker images, keeping only the specified number of recent versions (default: 5)

.PARAMETER KeepVersions
    Number of recent versions to keep when cleaning up old images (default: 5)

.EXAMPLE
    .\Release-DockerImages.ps1
    Automatically generates next version (e.g., 2026.1.1) and builds/pushes the image

.EXAMPLE
    .\Release-DockerImages.ps1 -Version "1.2.3"
    Builds and pushes the image with version "1.2.3"

.EXAMPLE
    .\Release-DockerImages.ps1 -SkipPush
    Builds images locally without pushing to registry

.EXAMPLE
    .\Release-DockerImages.ps1 -CleanupOldImages -KeepVersions 3
    Builds with auto-version and removes old images, keeping only 3 most recent versions
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [string]$Version = "",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipBuild,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipPush,
    
    [Parameter(Mandatory=$false)]
    [switch]$ConfigureRegistry,
    
    [Parameter(Mandatory=$false)]
    [switch]$CleanupOldImages,
    
    [Parameter(Mandatory=$false)]
    [int]$KeepVersions = 5
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

# Get next version in format Year.Month.Number
function Get-NextVersion {
    param(
        [string]$RepositoryRoot,
        [string]$ImageName
    )

    $currentYear = (Get-Date).Year
    $currentMonth = (Get-Date).Month
    $versionPattern = "^${currentYear}\.${currentMonth}\.(\d+)$"

    $existingVersions = @()

    # Git tags are the authoritative record of released versions
    try {
        $gitTags = git -C $RepositoryRoot tag --list
        if ($gitTags) {
            $existingVersions += $gitTags | Where-Object { $_ -match $versionPattern }
        }
    }
    catch {
        Write-Warning "Could not query git tags: $_"
    }

    # Local image tags serve as an additional source
    try {
        $localImages = docker images --format "{{.Tag}}" "${ImageName}" 2>$null
        if ($localImages) {
            $existingVersions += $localImages | Where-Object { $_ -match $versionPattern }
        }
    }
    catch {
        Write-Warning "Could not query local images: $_"
    }

    # Find the highest number for current year.month
    $maxNumber = 0
    foreach ($version in $existingVersions) {
        if ($version -match $versionPattern) {
            $number = [int]$matches[1]
            if ($number -gt $maxNumber) {
                $maxNumber = $number
            }
        }
    }

    # Return next version
    $nextNumber = $maxNumber + 1
    return "${currentYear}.${currentMonth}.${nextNumber}"
}

# Clean up old Docker images
function Remove-OldImages {
    param(
        [string]$ImageName,
        [string]$RegistryPrefix = $null,
        [int]$KeepCount = 5
    )
    
    Write-Step "Cleaning up old images (keeping $KeepCount most recent)"
    
    $allImages = @()
    
    # Get local images (both base name and registry-prefixed if pulled)
    $imagePatterns = @($ImageName)
    if ($RegistryPrefix) {
        $imagePatterns += "${RegistryPrefix}/${ImageName}"
    }
    
    foreach ($pattern in $imagePatterns) {
        try {
            $dockerOutput = docker images --format "{{.Repository}}:{{.Tag}}|{{.ID}}" $pattern 2>&1
            if ($dockerOutput -and $LASTEXITCODE -eq 0) {
                foreach ($line in $dockerOutput) {
                    if ([string]::IsNullOrWhiteSpace($line)) { continue }
                    
                    $parts = $line -split '\|'
                    if ($parts.Length -ge 2) {
                        $fullName = $parts[0].Trim()
                        $id = $parts[1].Trim()
                        
                        # Extract tag from full name
                        if ($fullName -match '^[^:]+:(.+)$') {
                            $tag = $matches[1]
                            
                            # Only process version tags in format Year.Month.Number
                            if ($tag -match '^\d{4}\.\d+\.\d+$') {
                                # Check if we already have this tag (avoid duplicates)
                                $existing = $allImages | Where-Object { $_.Tag -eq $tag }
                                if (-not $existing) {
                                    $allImages += @{
                                        Tag = $tag
                                        Id = $id
                                        FullName = $fullName
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        catch {
            Write-Warning "Could not query images for pattern '$pattern': $_"
        }
    }
    
    if ($allImages.Count -eq 0) {
        Write-Info "No versioned images found to clean up."
        return
    }
    
    if ($allImages.Count -le $KeepCount) {
        Write-Info "Found $($allImages.Count) versioned image(s). No cleanup needed (keeping $KeepCount)."
        return
    }
    
    # Sort by version (newest first)
    # Convert version string to numeric value for proper sorting
    $sortedImages = $allImages | Sort-Object {
        $parts = $_.Tag -split '\.'
        if ($parts.Length -eq 3) {
            try {
                [int]$parts[0] * 10000 + [int]$parts[1] * 100 + [int]$parts[2]
            }
            catch {
                0
            }
        }
        else {
            0
        }
    } -Descending
    
    # Get images to remove (everything after KeepCount)
    $imagesToRemove = $sortedImages | Select-Object -Skip $KeepCount
    
    if ($imagesToRemove.Count -eq 0) {
        Write-Info "No images to remove."
        return
    }
    
    Write-Info "Found $($allImages.Count) versioned image(s). Removing $($imagesToRemove.Count) old image(s)..."
    
    $removedCount = 0
    $failedCount = 0
    
    foreach ($image in $imagesToRemove) {
        try {
            Write-Info "Removing $($image.FullName)..."
            $removeOutput = docker rmi $image.FullName 2>&1
            
            if ($LASTEXITCODE -eq 0) {
                Write-Success "Removed $($image.FullName)"
                $removedCount++
            }
            else {
                $errorMsg = $removeOutput | Out-String
                if ($errorMsg -match "image is being used|image has dependent child images") {
                    Write-Warning "Skipped $($image.FullName) (image is in use)"
                }
                else {
                    Write-Warning "Failed to remove $($image.FullName): $errorMsg"
                }
                $failedCount++
            }
        }
        catch {
            Write-Warning "Error removing $($image.FullName): $_"
            $failedCount++
        }
    }
    
    if ($removedCount -gt 0) {
        Write-Success "Cleanup complete. Removed $removedCount image(s), kept $KeepCount most recent version(s)."
    }
    if ($failedCount -gt 0) {
        Write-Warning "$failedCount image(s) could not be removed (may be in use)."
    }
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

# Create git tag for release version
function Ensure-GitTag {
    param(
        [string]$RepositoryRoot,
        [string]$Version
    )
    
    Write-Step "Creating Git tag"
    
    try {
        git -C $RepositoryRoot --version | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw "Git is not available"
        }
    }
    catch {
        Write-Warning "Git is not available. Skipping tag creation."
        return
    }
    
    try {
        $existingTag = git -C $RepositoryRoot tag --list $Version
        if (-not [string]::IsNullOrWhiteSpace($existingTag)) {
            Write-Info "Git tag '$Version' already exists. Skipping creation."
            return
        }
        
        git -C $RepositoryRoot tag -a $Version -m "Release $Version"
        
        if ($LASTEXITCODE -ne 0) {
            Write-ErrorMessage "Failed to create git tag '$Version'"
            exit 1
        }
        
        Write-Success "Created git tag '$Version'"
    }
    catch {
        Write-ErrorMessage "Failed to create git tag '$Version': $_"
        exit 1
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
    $legacyClientImageName = "budget-client"   # old intermediate image, cleaned up only

    # Determine registry prefix for remote tags
    $registryPrefix = $null
    if ($config -and $config.Registry) {
        $registryPrefix = "$($config.Registry)"
        if ($config.Namespace) {
            $registryPrefix = "$registryPrefix/$($config.Namespace)"
        }
    }

    # Auto-generate version if not provided
    if ([string]::IsNullOrWhiteSpace($Version)) {
        Write-Step "Auto-generating next version"
        $Version = Get-NextVersion -RepositoryRoot $repoRoot -ImageName $serverImageName
        Write-Success "Generated version: $Version"
    }

    # Create git tag before building images
    Ensure-GitTag -RepositoryRoot $repoRoot -Version $Version

    # Local and remote tags
    $serverLocalTag = "${serverImageName}:${Version}"
    $serverRemoteTag = $null

    if ($config -and $config.Registry) {
        $serverRemoteTag = "${registryPrefix}/${serverImageName}:${Version}"
    }
    
    # Build image
    if (-not $SkipBuild) {
        Write-Step "Starting Docker Image Build"

        $serverDockerfile = Join-Path $hostsDir "NVs.Budget.Hosts.Web.Server\Dockerfile"
        $serverSuccess = Build-DockerImage `
            -Name "Server (with embedded client)" `
            -DockerfilePath $serverDockerfile `
            -Context $repoRoot `
            -Tag $serverLocalTag

        if (-not $serverSuccess) {
            Write-ErrorMessage "Server build failed. Aborting."
            exit 1
        }

        Write-Success "`nImage built successfully!"

        # Display local images
        Write-Step "Local Images Built"
        docker images | Select-String -Pattern "(REPOSITORY|$serverImageName)"
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
            $serverPushSuccess = Publish-DockerImage `
                -LocalTag $serverLocalTag `
                -RemoteTag $serverRemoteTag `
                -Name "Server"

            if ($serverPushSuccess) {
                Write-Success "`nServer image published successfully!"
                Write-Host ""
                Write-Info "Server Image: $serverRemoteTag"
                Write-Info "Note: Client assets are embedded in the server image"
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
    
    # Cleanup old images if requested
    if ($CleanupOldImages) {
        Remove-OldImages -ImageName $serverImageName -RegistryPrefix $registryPrefix -KeepCount $KeepVersions
        Remove-OldImages -ImageName $legacyClientImageName -KeepCount $KeepVersions
    }

    Write-Host ""
    Write-Step "Release Process Complete"
    Write-Host ""
    Write-Success "Local image is tagged as:"
    Write-Host "  - $serverLocalTag (client assets embedded in wwwroot)"
    
    if ($config -and $config.Registry -and -not $SkipPush) {
        Write-Host ""
        Write-Success "Remote images are available at:"
        Write-Host "  - $serverRemoteTag (includes embedded client)"
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
