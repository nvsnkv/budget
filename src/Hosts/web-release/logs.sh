#!/usr/bin/env bash
# View logs from Budget application services
#
# Usage:
#   ./logs.sh                     # all services
#   ./logs.sh budget-server       # specific service (budget-server, postgres, pgbackups, ...)
#   ./logs.sh -f                  # follow log output
#   ./logs.sh -f budget-server

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

usage() {
    echo "Usage: ./logs.sh [-f|--follow] [service]"
    echo
    echo "Options:"
    echo "  -f, --follow   Follow log output"
    echo "  service        Specific service to view logs for (budget-server, postgres, pgbackups, ...)"
}

follow_flag=""
service=""

while [ $# -gt 0 ]; do
    case "$1" in
        -f|--follow) follow_flag="-f" ;;
        -h|--help) usage; exit 0 ;;
        *) service="$1" ;;
    esac
    shift
done

if [ -n "$service" ]; then
    echo "Viewing logs for: $service"
    docker-compose logs $follow_flag "$service"
else
    echo "Viewing logs for all services"
    docker-compose logs $follow_flag
fi
