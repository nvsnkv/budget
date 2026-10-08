#!/usr/bin/env bash
# Master script to start all Budget application services
# This script starts Docker dependencies and runs server/client on the host

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo "========================================"
echo "  Budget Application - Development Mode"
echo "========================================"
echo

# Step 1: Start Docker services (postgres and dev-certs)
echo "[1/4] Starting Docker services (postgres, dev-certs)..."
docker compose up -d postgres dev-certs
echo "Docker services started successfully!"
echo

# Step 2: Wait for postgres to be ready
echo "[2/4] Waiting for PostgreSQL to be ready..."
max_attempts=30
attempt=0
ready=false

while [ "$ready" = false ] && [ "$attempt" -lt "$max_attempts" ]; do
    attempt=$((attempt + 1))
    if docker exec "$(docker compose ps -q postgres)" pg_isready -U postgres 2>/dev/null | grep -q "accepting connections"; then
        ready=true
    else
        echo "  Attempt $attempt/$max_attempts - Waiting..."
        sleep 1
    fi
done

if [ "$ready" = false ]; then
    echo "PostgreSQL failed to start within timeout!" >&2
    exit 1
fi
echo "PostgreSQL is ready!"
echo

# Step 3: Extract certificates if needed
echo "[3/4] Setting up SSL certificates..."
certs_path="$SCRIPT_DIR/certs"
mkdir -p "$certs_path"

cert_file="$certs_path/aspnetapp.pfx"
if [ ! -f "$cert_file" ]; then
    echo "  Extracting certificates from Docker volume..."

    # Wait for dev-certs to complete
    sleep 2

    # Copy certs from Docker volume
    container_id="$(docker create -v web-debug_certs:/https alpine)"
    docker cp "$container_id:/https/." "$certs_path"
    docker rm "$container_id" >/dev/null

    echo "  Certificates extracted successfully!"
else
    echo "  Certificates already exist."
fi
echo

# Step 4: Start server and client
echo "[4/4] Starting server and client..."
cat <<'BANNER'

================================================================================
SERVICES STARTING
================================================================================

The following services will start in this terminal:
  - .NET Server (https://localhost:7237, http://localhost:5153)  [background]
  - Angular Client (http://localhost:4200)                       [foreground]

Docker Services Running:
  - PostgreSQL (localhost:20000)

Press Ctrl+C to stop the client (the server stops with it).
To stop all services including Docker, run: ./stop-all.sh

================================================================================

BANNER

# Start the server in the background
"$SCRIPT_DIR/start-server.sh" &
server_pid=$!

# Stop the server when this script exits (e.g. after Ctrl+C on the client)
trap 'kill "$server_pid" 2>/dev/null' EXIT

# Start the client in the foreground
"$SCRIPT_DIR/start-client.sh"
