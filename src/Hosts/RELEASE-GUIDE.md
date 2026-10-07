# Docker Release Guide

This guide explains how to use the `release-docker-images.sh` script to build and publish Docker images for the Budget application.

## Prerequisites

- Bash (Linux/Mac, or WSL/Git Bash on Windows)
- Docker installed and running
- Access to a Docker registry (Docker Hub, GitHub Container Registry, or private registry)

## Quick Start

### First Time Setup

1. Navigate to the Hosts directory:
```bash
cd src/Hosts
```

2. Run the script (it will prompt for registry configuration):
```bash
./release-docker-images.sh
```

3. When prompted, provide:
   - **Docker Registry URL**: e.g., `docker.io`, `ghcr.io`, or your private registry
   - **Registry Username**: Your Docker registry username
   - **Registry Password**: Your Docker registry password (hidden input)
   - **Image Namespace**: e.g., `mycompany/budget` or just your username

The script will save these settings in `docker-registry-config.json` for future use.

## Usage Examples

### Build and Push with Auto-Generated Version
```bash
./release-docker-images.sh
```

### Build and Push with Specific Version
```bash
./release-docker-images.sh --version 1.2.3
```

### Build Locally Without Pushing
```bash
./release-docker-images.sh --skip-push
```

### Push Previously Built Images
```bash
./release-docker-images.sh --skip-build --version 1.2.3
```

### Reconfigure Registry Settings
```bash
./release-docker-images.sh --configure-registry
```

## Parameters

| Parameter | Description | Default |
|-----------|-------------|---------|
| `--version <v>` | Version tag for the image | Auto-generated `Year.Month.Number` (e.g., `2026.10.1`) |
| `--skip-build` | Skip building the image, only push an existing one | `false` |
| `--skip-push` | Build the image but don't push to registry | `false` |
| `--configure-registry` | Force reconfiguration of registry settings | `false` |
| `--cleanup-old-images` | Remove old local Docker images, keeping the most recent versions | `false` |
| `--keep-versions <n>` | Number of recent versions to keep when cleaning up | `5` |

## What the Script Does

1. **Validates Environment**: Checks that Docker is installed and accessible
2. **Loads/Creates Configuration**: Uses saved registry config or prompts for new configuration
3. **Builds the Server Image** (single multi-stage build):
   - Uses `NVs.Budget.Hosts.Web.Server/Dockerfile`
   - Build context: Repository root (see the root `.dockerignore`; `.git` is intentionally included so GitVersion can derive the assembly version)
   - Stage 1 (node): builds the Angular client production bundle
   - Stage 2 (sdk): publishes the .NET server
   - Final stage: aspnet runtime with the client assets embedded in `/app/wwwroot`
   - Tags as `budget-server:<version>`
4. **Logs into Registry**: Authenticates with the configured Docker registry
5. **Pushes Server Image**: Tags and pushes the server image (which includes the embedded client) to the registry

## Registry Configuration

The script stores registry credentials in `docker-registry-config.json`. This file contains:
- Registry URL
- Username
- Password
- Namespace/repository path
- Configuration date

**⚠️ Security Note**: The password is stored in plain text (the Windows DPAPI encryption used by the previous PowerShell version is not available on Linux). The script restricts the file permissions to `600` (readable by the owner only). Do not commit this file to version control. Alternatively, you can keep the password out of the file entirely and supply it via the `DOCKER_REGISTRY_PASSWORD` environment variable at run time.

## Configuration File Location

- **Config File**: `src/Hosts/docker-registry-config.json`
- **Script Location**: `src/Hosts/release-docker-images.sh`

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
**Solution**: Install Docker (Docker Engine/Docker Desktop) and ensure the `docker` command is in your PATH

### Login Failed
```
✗ Docker login failed
```
**Solution**: 
- Verify your credentials
- Check if you have access to the registry
- Run with `--configure-registry` to re-enter credentials

### Build Failed
```
✗ Failed to build Server
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
```bash
cat > docker-registry-config.json <<EOF
{
  "Registry": "$DOCKER_REGISTRY",
  "Username": "$DOCKER_USERNAME",
  "Password": "$DOCKER_PASSWORD",
  "Namespace": "$DOCKER_NAMESPACE"
}
EOF
chmod 600 docker-registry-config.json
```

   Or skip storing the password in the file and export it instead:
```bash
export DOCKER_REGISTRY_PASSWORD="$DOCKER_PASSWORD"
```

3. Run the build:
```bash
./release-docker-images.sh --version "$BUILD_VERSION"
```

## Local Development

For local testing without pushing to registry:
```bash
./release-docker-images.sh --skip-push --version dev
```

Then run locally:
```bash
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
```bash
# Development build
./release-docker-images.sh --version dev --skip-push

# Release candidate
./release-docker-images.sh --version 1.2.3-rc1

# Production release
./release-docker-images.sh --version 1.2.3
./release-docker-images.sh --version latest
```

## Support

For issues or questions about the release process, please refer to the project documentation or contact the development team.
