#!/usr/bin/env bash
# Stop the Budget application
#
# Usage:
#   ./stop.sh                    # stop services
#   ./stop.sh --remove-volumes   # also remove volumes (database data will be lost!)

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

remove_volumes=false

while [ $# -gt 0 ]; do
    case "$1" in
        --remove-volumes|-v) remove_volumes=true ;;
        *)
            echo "Unknown option: $1" >&2
            echo "Usage: ./stop.sh [--remove-volumes]" >&2
            exit 1
            ;;
    esac
    shift
done

echo "Budget Application - Stopping Services"
echo "======================================"
echo

if [ "$remove_volumes" = true ]; then
    echo "WARNING: This will remove all volumes including database data!"
    printf "Are you sure? Type 'yes' to confirm: "
    read -r confirm

    if [ "$confirm" != "yes" ]; then
        echo "Cancelled"
        exit 0
    fi

    echo "Stopping services and removing volumes..."
    docker-compose down -v
else
    echo "Stopping services..."
    docker-compose down
fi

echo
echo "Services stopped successfully"
