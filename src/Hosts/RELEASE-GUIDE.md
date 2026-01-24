# Docker Release Guide

This guide explains how to use the `Release-DockerImages.ps1` script to build and publish Docker images for the Budget application.

## Prerequisites

- PowerShell 5.1 or later (PowerShell Core 7+ recommended)
- Docker installed and running
- Access to a Docker registry (Docker Hub, GitHub Container Registry, or private registry)

## Quick Start

### First Time Setup

1. Navigate to the Hosts directory:
```powershell
cd src\Hosts
```

2. Run the script (it will prompt for registry configuration):
```powershell
.\Release-DockerImages.ps1
```

3. When prompted, provide:
   - **Docker Registry URL**: e.g., `docker.io`, `ghcr.io`, or your private registry
   - **Registry Username**: Your Docker registry username
   - **Registry Password**: Your Docker registry password (hidden input)
   - **Image Namespace**: e.g., `mycompany/budget` or just your username

The script will save these settings in `docker-registry-config.json` for future use.

## Usage Examples

### Build and Push with Default Version (latest)
```powershell
.\Release-DockerImages.ps1
```

### Build and Push with Specific Version
```powershell
.\Release-DockerImages.ps1 -Version "1.2.3"
```

### Build Locally Without Pushing
```powershell
.\Release-DockerImages.ps1 -SkipPush
```

### Push Previously Built Images
```powershell
.\Release-DockerImages.ps1 -SkipBuild -Version "1.2.3"
```

### Reconfigure Registry Settings
```powershell
.\Release-DockerImages.ps1 -ConfigureRegistry
```

## Parameters

| Parameter | Description | Default |
|-----------|-------------|---------|
| `-Version` | Version tag for images | `latest` |
| `-SkipBuild` | Skip building images, only push existing ones | `false` |
| `-SkipPush` | Build images but don't push to registry | `false` |
| `-ConfigureRegistry` | Force reconfiguration of registry settings | `false` |

## What the Script Does

1. **Validates Environment**: Checks that Docker is installed and accessible
2. **Loads/Creates Configuration**: Uses saved registry config or prompts for new configuration
3. **Builds Client Image** (Angular build only): 
   - Uses `Controllers/NVs.Budget.Controllers.Web.Client/Dockerfile`
   - Build context: `Controllers/NVs.Budget.Controllers.Web.Client` directory
   - Builds Angular production bundle
   - Tags as `budget-client:<version>` (intermediate build image)
4. **Builds Server Image** (with embedded client): 
   - Uses `NVs.Budget.Hosts.Web.Server/Dockerfile`
   - Build context: Repository root
   - Copies client assets from client image via build arg
   - Tags as `budget-server:<version>`
5. **Logs into Registry**: Authenticates with the configured Docker registry
6. **Pushes Server Image**: Tags and pushes the server image (which includes the embedded client) to the registry

## Registry Configuration

The script stores registry credentials in `docker-registry-config.json`. This file contains:
- Registry URL
- Username
- Encrypted password (Windows DPAPI encrypted)
- Namespace/repository path
- Configuration date

**⚠️ Security Note**: The password is encrypted using Windows Data Protection API (DPAPI), which means it can only be decrypted by the same user on the same machine. Do not commit this file to version control.

## Configuration File Location

- **Config File**: `src/Hosts/docker-registry-config.json`
- **Script Location**: `src/Hosts/Release-DockerImages.ps1`

## Docker Registry Examples

### Docker Hub
```
Registry: docker.io
Username: your-dockerhub-username
Namespace: your-dockerhub-username
```
Images will be: `docker.io/your-dockerhub-username/budget-server:latest`

### GitHub Container Registry
```
Registry: ghcr.io
Username: your-github-username
Namespace: your-github-username
```
Images will be: `ghcr.io/your-github-username/budget-server:latest`

### Private Registry
```
Registry: registry.mycompany.com
Username: your-username
Namespace: mycompany/budget
```
Images will be: `registry.mycompany.com/mycompany/budget/budget-server:latest`

## Troubleshooting

### Docker Not Found
```
✗ Docker is not installed or not in PATH
```
**Solution**: Install Docker Desktop or ensure Docker is in your PATH

### Login Failed
```
✗ Docker login failed
```
**Solution**: 
- Verify your credentials
- Check if you have access to the registry
- Run with `-ConfigureRegistry` to re-enter credentials

### Build Failed
```
✗ Failed to build Server/Client
```
**Solution**: 
- Check Docker logs for specific errors
- Ensure all source files are present
- Verify Dockerfile paths are correct
- Check available disk space

### Permission Denied
```
denied: permission denied for resource
```
**Solution**: 
- Verify you have push permissions to the registry
- Check namespace/repository exists and you have access
- For Docker Hub, the repository must exist before first push

## CI/CD Integration

For automated builds in CI/CD pipelines, you can:

1. Store credentials in pipeline secrets
2. Create the config file programmatically:
```powershell
$config = @{
    Registry = $env:DOCKER_REGISTRY
    Username = $env:DOCKER_USERNAME
    EncryptedPassword = ConvertFrom-SecureString (ConvertTo-SecureString $env:DOCKER_PASSWORD -AsPlainText -Force)
    Namespace = $env:DOCKER_NAMESPACE
}
$config | ConvertTo-Json | Set-Content "docker-registry-config.json"
```

3. Run the build:
```powershell
.\Release-DockerImages.ps1 -Version $env:BUILD_VERSION
```

## Local Development

For local testing without pushing to registry:
```powershell
.\Release-DockerImages.ps1 -SkipPush -Version "dev"
```

Then run locally:
```powershell
docker run -p 5153:5153 budget-server:dev
```

Note: The client is now embedded in the server image, so only the server container is needed.

## Version Tagging Strategy

Consider using semantic versioning:
- **Major.Minor.Patch**: `1.2.3` for releases
- **latest**: Always points to the most recent stable release
- **dev**: Development builds
- **Branch names**: `feature-auth`, `hotfix-123`

Example workflow:
```powershell
# Development build
.\Release-DockerImages.ps1 -Version "dev" -SkipPush

# Release candidate
.\Release-DockerImages.ps1 -Version "1.2.3-rc1"

# Production release
.\Release-DockerImages.ps1 -Version "1.2.3"
.\Release-DockerImages.ps1 -Version "latest"
```

## Support

For issues or questions about the release process, please refer to the project documentation or contact the development team.
