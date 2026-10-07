#!/usr/bin/env bash
# Stop all Budget application services

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo "Stopping all Budget application services..."

# Stop Docker services
echo "Stopping Docker containers..."
docker compose down

echo
echo "Docker services stopped!"
echo "Note: Server and client processes running in other terminals must be stopped manually (Ctrl+C)."
