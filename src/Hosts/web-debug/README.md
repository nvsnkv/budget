# Budget Application - Development Scripts

This directory contains scripts to run the Budget application in development mode with live reload/watch capabilities.

## Architecture

- **Docker Services**: PostgreSQL database and SSL certificate generation
- **Host Services**: .NET server and Angular client running with watch mode for fast iteration

## Prerequisites

### Required Software
- Docker Desktop
- Bash (Linux/Mac) or PowerShell 7+ (Windows)
- .NET SDK 8.0+
- Node.js 20+
- npm

### Configuration
1. Copy `server.env.example` to `server.env`
2. Fill in your Yandex OAuth credentials in `server.env`:
   ```
   Auth__Yandex__ClientSecret = your_client_secret
   Auth__Yandex__ClientId = your_client_id
   ```

## Quick Start

Every script in this directory comes in both a bash (`.sh`) and a PowerShell (`.ps1`) variant with matching parameters — use whichever matches your OS. The examples below use the bash variants (on Windows: `.\start-all.ps1`, `.\start-server.ps1`, etc.).

### Start All Services
```bash
./start-all.sh
```

This will:
1. Start PostgreSQL and generate SSL certificates in Docker
2. Extract certificates to the `certs/` directory
3. Launch the .NET server with watch mode (in the background)
4. Launch the Angular client with watch mode (in the foreground)

### Start Services Individually

**Start only Docker dependencies:**
```bash
docker compose up -d
```

**Start only the server:**
```bash
./start-server.sh
```

**Start only the client:**
```bash
./start-client.sh
```

### Stop All Services
```bash
./stop-all.sh
```

Then manually stop any server/client processes still running in other terminals (Ctrl+C).
When started via `./start-all.sh`, Ctrl+C stops the client and the server together.

## Service URLs

- **Server (HTTPS)**: https://localhost:7237
- **Server (HTTP)**: http://localhost:5153
- **Client**: http://localhost:4200
- **PostgreSQL**: localhost:20000

## Development Workflow

1. Start all services with `./start-all.sh`
2. Make changes to your code
3. Watch mode will automatically detect changes and reload:
   - **.NET Server**: `dotnet watch` rebuilds and restarts
   - **Angular Client**: Hot module replacement (HMR)
4. View changes in your browser

## Troubleshooting

### Certificate Issues
If you encounter SSL certificate errors:
```bash
# Delete the certs directory
rm -rf ./certs

# Restart services to regenerate
./start-all.sh
```

### Database Connection Issues
```bash
# Check if PostgreSQL is running
docker compose ps

# View PostgreSQL logs
docker compose logs postgres

# Restart PostgreSQL
docker compose restart postgres
```

### Port Already in Use
If ports are already in use, stop any conflicting services:
- Server: 7237 (HTTPS), 5153 (HTTP)
- Client: 4200
- PostgreSQL: 20000

## Docker Volumes

- `web-debug_budgetdb-data`: PostgreSQL data (persistent)
- `web-debug_certs`: SSL certificates

To reset the database:
```bash
docker compose down -v
```

## Files Structure

```
web-debug/
├── docker-compose.yml      # Docker services (postgres, dev-certs)
├── dev-certs.Dockerfile    # Certificate generation
├── server.env              # Server environment variables (gitignored)
├── server.env.example      # Template for server.env
├── start-all.sh / .ps1     # Master script to start everything
├── start-server.sh / .ps1  # Start .NET server on host
├── start-client.sh / .ps1  # Start Angular client on host
├── stop-all.sh / .ps1      # Stop Docker services
└── README.md               # This file
```

## Benefits of This Approach

✅ **Fast Iteration**: Watch mode catches changes instantly  
✅ **Better Debugging**: Direct access to processes on host  
✅ **Isolated Dependencies**: Database runs in Docker  
✅ **Flexible Development**: Start/stop services independently  
✅ **Production-like**: SSL certificates and proper configuration

