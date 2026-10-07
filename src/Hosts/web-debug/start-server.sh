#!/usr/bin/env bash
# Start the .NET Budget Server with watch mode on the host machine
# Prerequisites: Docker Compose must be running (postgres and dev-certs services)
#
# Usage: ./start-server.sh [certs-path]   (default certs path: ./certs)

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CERTS_PATH="${1:-$SCRIPT_DIR/certs}"

trim() {
    printf '%s' "$1" | sed -e 's/^[[:space:]]*//' -e 's/[[:space:]]*$//'
}

echo "Starting Budget Server..."

# Create certs directory if it doesn't exist
mkdir -p "$CERTS_PATH"

# Get absolute path for certs
CERTS_ABS_PATH="$(cd "$CERTS_PATH" && pwd)"

# Check if certs exist, if not extract from Docker volume
CERT_FILE="$CERTS_ABS_PATH/aspnetapp.pfx"
if [ ! -f "$CERT_FILE" ]; then
    echo "Extracting certificates from Docker volume..."

    # Ensure dev-certs container has run
    docker compose up dev-certs --build

    # Use a temporary container to copy files from the volume
    container_id="$(docker create -v web-debug_certs:/https alpine)"
    docker cp "$container_id:/https/." "$CERTS_ABS_PATH"
    docker rm "$container_id" >/dev/null

    echo "Certificates extracted to $CERTS_ABS_PATH"
fi

# Load Yandex Auth credentials from server.env
SERVER_ENV_PATH="$SCRIPT_DIR/server.env"
if [ -f "$SERVER_ENV_PATH" ]; then
    while IFS= read -r line || [ -n "$line" ]; do
        # Skip empty lines and comments; format is KEY = VALUE
        case "$line" in
            ''|'#'*) continue ;;
        esac
        case "$line" in
            *=*)
                key="$(trim "${line%%=*}")"
                value="$(trim "${line#*=}")"
                if [[ "$key" =~ ^[A-Za-z_][A-Za-z0-9_]*$ ]]; then
                    export "$key=$value"
                    echo "Loaded: $key"
                fi
                ;;
        esac
    done < "$SERVER_ENV_PATH"
else
    echo "Warning: server.env not found at $SERVER_ENV_PATH"
fi

# Set environment variables
export ASPNETCORE_ENVIRONMENT=Development
export ConnectionStrings__BudgetContext="Host=localhost;Port=20000;Database=budgetdb;Username=postgres;Password=postgres"
export ConnectionStrings__IdentityContext="Host=localhost;Port=20000;Database=budgetdb;Username=postgres;Password=postgres"
export ASPNETCORE_Kestrel__Certificates__Default__Path="$CERT_FILE"
export ASPNETCORE_Kestrel__Certificates__Default__Password="dev-password-do-not-use-in-production"

# Navigate to server directory
cd "$SCRIPT_DIR/../NVs.Budget.Hosts.Web.Server"

echo
echo "Environment Configuration:"
echo "  Database: localhost:20000/budgetdb"
echo "  HTTPS Certificate: $CERT_FILE"
echo "  Launch Profile: https (7237, 5153)"
echo
echo "Starting dotnet watch..."
echo "----------------------------------------"
echo

# Run dotnet watch
exec dotnet watch run --project NVs.Budget.Hosts.Web.Server.csproj --launch-profile https
