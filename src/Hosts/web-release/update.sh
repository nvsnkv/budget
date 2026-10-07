#!/usr/bin/env bash
# Update Budget application to a new version
#
# Usage:
#   ./update.sh               # use the version from .env
#   ./update.sh 1.0.1         # update to a specific version

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

version="${1:-}"

echo "Budget Application - Update"
echo "==========================="
echo

# Check if .env exists
if [ ! -f ".env" ]; then
    echo ".env file not found!" >&2
    exit 1
fi

# Update version in .env if specified
if [ -n "$version" ]; then
    echo "Updating version to: $version"
    if grep -qE '^[[:space:]]*IMAGE_VERSION[[:space:]]*=' .env; then
        sed -i.bak -E "s|^[[:space:]]*IMAGE_VERSION[[:space:]]*=.*$|IMAGE_VERSION=$version|" .env
        rm -f .env.bak
    else
        printf 'IMAGE_VERSION=%s\n' "$version" >> .env
    fi
    echo "Version updated in .env"
    echo
fi

# Pull new images
echo "Pulling images..."
if ! docker-compose pull; then
    echo "Failed to pull images" >&2
    exit 1
fi

echo "Images pulled successfully"
echo

# Restart services
echo "Restarting services..."
if ! docker-compose up -d; then
    echo "Failed to restart services" >&2
    exit 1
fi

echo
echo "Update completed successfully!"
echo
echo "Monitor the services with: docker-compose ps"
echo "View logs with: ./logs.sh -f"
