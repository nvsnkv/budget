#!/usr/bin/env bash
# Start the Budget application in production mode
#
# Usage:
#   ./start.sh            # start services
#   ./start.sh --pull     # pull latest images before starting

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

trim() {
    printf '%s' "$1" | sed -e 's/^[[:space:]]*//' -e 's/[[:space:]]*$//'
}

# Reads a KEY=VALUE entry from .env (first match, non-commented), or echoes the default
get_env_value() {
    local key="$1" default="$2" line
    line="$(grep -E "^[[:space:]]*${key}[[:space:]]*=" .env 2>/dev/null | head -n 1 || true)"
    if [ -n "$line" ]; then
        trim "${line#*=}"
    else
        printf '%s' "$default"
    fi
}

# Prints the name of every running service that is not healthy (cert-generator excluded)
compose_unhealthy_services() {
    local id line service health
    for id in $(docker-compose ps -q 2>/dev/null); do
        line="$(docker inspect --format '{{ index .Config.Labels "com.docker.compose.service" }}|{{ if .State.Health }}{{ .State.Health.Status }}{{ else }}none{{ end }}' "$id" 2>/dev/null || true)"
        [ -n "$line" ] || continue
        service="${line%%|*}"
        health="${line#*|}"
        if [ "$service" != "cert-generator" ] && [ "$health" != "healthy" ]; then
            printf '%s\n' "$service"
        fi
    done
}

pull=false
while [ $# -gt 0 ]; do
    case "$1" in
        --pull|-p) pull=true ;;
        *)
            echo "Unknown option: $1" >&2
            echo "Usage: ./start.sh [--pull]" >&2
            exit 1
            ;;
    esac
    shift
done

echo "Budget Application - Production Start"
echo "======================================"
echo

# Check if .env exists
if [ ! -f ".env" ]; then
    echo ".env file not found!" >&2
    echo >&2
    echo "Please create .env file from .env.example:" >&2
    echo "  cp .env.example .env" >&2
    echo >&2
    echo "Then edit .env and configure required variables:" >&2
    echo "  - POSTGRES_PASSWORD" >&2
    echo "  - BACKUP_PATH" >&2
    echo "  - DOCKER_REGISTRY" >&2
    echo "  - DOCKER_NAMESPACE" >&2
    echo "  - YANDEX_OAUTH_CLIENT_ID" >&2
    echo "  - YANDEX_OAUTH_CLIENT_SECRET" >&2
    exit 1
fi

# Validate required environment variables
echo "Validating environment configuration..."
required_vars=(
    POSTGRES_PASSWORD
    BACKUP_PATH
    DOCKER_REGISTRY
    DOCKER_NAMESPACE
    YANDEX_OAUTH_CLIENT_ID
    YANDEX_OAUTH_CLIENT_SECRET
)

missing_vars=()
for var in "${required_vars[@]}"; do
    if ! grep -qE "^[[:space:]]*${var}[[:space:]]*=" .env; then
        missing_vars+=("$var")
    fi
done

if [ "${#missing_vars[@]}" -gt 0 ]; then
    echo "Missing required environment variables:" >&2
    for var in "${missing_vars[@]}"; do
        echo "  - $var" >&2
    done
    echo >&2
    echo "Please update your .env file with these values." >&2
    exit 1
fi

# Validate backup path exists
backup_path="$(get_env_value BACKUP_PATH "")"
if [ -n "$backup_path" ] && [ "$backup_path" != "/path/to/your/backups" ]; then
    if [ ! -d "$backup_path" ]; then
        echo "Backup path does not exist: $backup_path"
        printf "Create it now? (y/n) "
        read -r create
        if [ "$create" = "y" ]; then
            mkdir -p "$backup_path"
            echo "Backup directory created"
        else
            echo "Cannot continue without backup directory" >&2
            exit 1
        fi
    fi
fi

echo "Environment configuration valid"
echo

# Pull images if requested
if [ "$pull" = true ]; then
    echo "Pulling latest images..."
    if ! docker-compose pull; then
        echo "Failed to pull images" >&2
        exit 1
    fi
    echo "Images pulled successfully"
    echo
fi

# Start services
echo "Starting services..."
if ! docker-compose up -d; then
    echo "Failed to start services" >&2
    exit 1
fi

echo
echo "Services started successfully!"
echo
echo "Waiting for services to be healthy..."

# Wait for services to be healthy
max_wait=60
waited=0
all_healthy=false

while [ "$waited" -lt "$max_wait" ] && [ "$all_healthy" = false ]; do
    sleep 2
    waited=$((waited + 2))

    if [ -z "$(compose_unhealthy_services)" ]; then
        all_healthy=true
    else
        printf '.'
    fi
done

echo
echo

if [ "$all_healthy" = true ]; then
    echo "All services are healthy!"
else
    echo "Some services may still be starting up"
    echo "   Run 'docker-compose ps' to check status"
fi

echo
echo "Application is available at:"

# Get ports from .env or use defaults
client_port="$(get_env_value CLIENT_PORT 25000)"
server_port="$(get_env_value SERVER_PORT 25001)"

echo "  Client (Frontend): https://localhost:$client_port"
echo "  Server (API):      https://localhost:$server_port"
echo
echo "Note: Accept the self-signed certificate warning in your browser"
echo "Both services use Kestrel with HTTPS and shared SSL certificates"
echo
echo "To view logs: ./logs.sh -f"
echo "To stop:      ./stop.sh"
