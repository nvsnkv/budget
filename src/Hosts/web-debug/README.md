# Budget Application - Development Scripts

This directory contains scripts to run the Budget application in development mode with live reload/watch capabilities.

## Architecture

- **Docker Services**: PostgreSQL database and SSL certificate generation
- **Host Services**: .NET server and Angular client running with watch mode for fast iteration

## Prerequisites

### Required Software
- Docker Desktop
- PowerShell 7+
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

### Start All Services
```powershell
.\start-all.ps1
```

This will:
1. Start PostgreSQL and generate SSL certificates in Docker
2. Extract certificates to the `certs/` directory
3. Launch the .NET server with watch mode in a new window
4. Launch the Angular client with watch mode in a new window

### Start Services Individually

**Start only Docker dependencies:**
```powershell
docker compose up -d
```

**Start only the server:**
```powershell
.\start-server.ps1
```

**Start only the client:**
```powershell
.\start-client.ps1
```

### Stop All Services
```powershell
.\stop-all.ps1
```

Then manually stop server/client processes (Ctrl+C in their windows).

## Service URLs

- **Server (HTTPS)**: https://localhost:7237
- **Server (HTTP)**: http://localhost:5153
- **Client**: http://localhost:4200
- **PostgreSQL**: localhost:20000

## Development Workflow

1. Start all services with `.\start-all.ps1`
2. Make changes to your code
3. Watch mode will automatically detect changes and reload:
   - **.NET Server**: `dotnet watch` rebuilds and restarts
   - **Angular Client**: Hot module replacement (HMR)
4. View changes in your browser

## Troubleshooting

### Certificate Issues
If you encounter SSL certificate errors:
```powershell
# Delete the certs directory
Remove-Item -Recurse -Force .\certs

# Restart services to regenerate
.\start-all.ps1
```

### Database Connection Issues
```powershell
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
```powershell
docker compose down -v
```

## Files Structure

```
web-debug/
├── docker-compose.yml      # Docker services (postgres, dev-certs)
├── dev-certs.Dockerfile    # Certificate generation
├── server.env              # Server environment variables (gitignored)
├── server.env.example      # Template for server.env
├── start-all.ps1           # Master script to start everything
├── start-server.ps1        # Start .NET server on host
├── start-client.ps1        # Start Angular client on host
├── stop-all.ps1            # Stop Docker services
└── README.md               # This file
```

## Benefits of This Approach

✅ **Fast Iteration**: Watch mode catches changes instantly  
✅ **Better Debugging**: Direct access to processes on host  
✅ **Isolated Dependencies**: Database runs in Docker  
✅ **Flexible Development**: Start/stop services independently  
✅ **Production-like**: SSL certificates and proper configuration

