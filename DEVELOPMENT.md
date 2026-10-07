# Local Development Guide

This guide covers setting up and running the Budget application for local development.

## Prerequisites

### Required Software
- Docker Desktop
- Bash (Linux/Mac) or PowerShell 7+ (Windows)
- .NET SDK 10.0+
- Node.js 22+
- npm

## Quick Start

The easiest way to start development is using the scripts in `src/Hosts/web-debug/`:

```bash
cd src/Hosts/web-debug
./start-all.sh
```

On Windows, use the PowerShell equivalents (`.\start-all.ps1`, `.\start-server.ps1`, etc.) — every script in `web-debug/` and `web-release/` comes in both a bash (`.sh`) and a PowerShell (`.ps1`) variant with matching parameters. The examples below use the bash variants.

This will:
1. Start PostgreSQL database in Docker
2. Generate SSL certificates
3. Launch the .NET server with watch mode
4. Launch the Angular client with watch mode

## Configuration

### 1. Environment Setup

1. Navigate to `src/Hosts/web-debug/`
2. Copy `server.env.example` to `server.env`
3. Fill in your Yandex OAuth credentials:
   ```
   Auth__Yandex__ClientSecret = your_client_secret
   Auth__Yandex__ClientId = your_client_id
   ```

### 2. Database Setup

The development setup uses Docker Compose to run PostgreSQL. The database is automatically created when you run `docker compose up -d`.

**Connection Details:**
- Host: `localhost`
- Port: `20000`
- Database: `budgetdb`
- User: `postgres`
- Password: `postgres`

## Service URLs

Once all services are running:

- **Server (HTTPS)**: https://localhost:7237
- **Server (HTTP)**: http://localhost:5153
- **Client**: https://localhost:4200
- **PostgreSQL**: localhost:20000

## Development Workflow

1. **Start all services**:
   ```bash
   cd src/Hosts/web-debug
   ./start-all.sh
   ```

2. **Make changes** to your code in the editor

3. **Watch mode automatically reloads**:
   - **.NET Server**: `dotnet watch` detects changes and rebuilds/restarts
   - **Angular Client**: Hot Module Replacement (HMR) updates the browser

4. **View changes** in your browser at http://localhost:4200

## Starting Services Individually

### Start only Docker dependencies
```bash
cd src/Hosts/web-debug
docker compose up -d
```

### Start only the server
```bash
cd src/Hosts/web-debug
./start-server.sh
```

### Start only the client
```bash
cd src/Hosts/web-debug
./start-client.sh
```

### Stop all services
```bash
cd src/Hosts/web-debug
./stop-all.sh
```

Then manually stop any server/client processes still running in other terminals (Ctrl+C).
When started via `./start-all.sh`, Ctrl+C stops the client and the server together.

## Database Management

### Apply Migrations

After starting the services, apply database migrations:
```bash
GET https://localhost:7237/admin/patch-db
```

Or use curl:
```bash
curl -k https://localhost:7237/admin/patch-db
```

### Reset Database

To completely reset the database (removes all data):
```bash
cd src/Hosts/web-debug
docker compose down -v
```

This removes the Docker volume containing PostgreSQL data. The database will be recreated on next start.

## Troubleshooting

### Certificate Issues

If you encounter SSL certificate errors:

```bash
cd src/Hosts/web-debug
# Delete the certs directory
rm -rf ./certs

# Restart services to regenerate certificates
./start-all.sh
```

### Database Connection Issues

```bash
cd src/Hosts/web-debug
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

### Server Won't Start

1. Check that PostgreSQL is running: `docker compose ps`
2. Verify connection strings in `server.env`
3. Check server logs in the terminal where it's running
4. Ensure ports 7237 and 5153 are not in use

### Client Won't Start

1. Verify Node.js and npm are installed: `node --version` and `npm --version`
2. Check that port 4200 is not in use
3. Try deleting `node_modules` and reinstalling:
   ```bash
   cd src/Hosts/NVs.Budget.Hosts.Web.Client/budget-client
   rm -rf node_modules
   npm install
   ```

## Project Structure

```
src/Hosts/web-debug/
├── docker-compose.yml      # Docker services (postgres, dev-certs)
├── dev-certs.Dockerfile    # Certificate generation
├── server.env              # Server environment variables (gitignored)
├── server.env.example      # Template for server.env
├── start-all.sh / .ps1     # Master script to start everything
├── start-server.sh / .ps1  # Start .NET server on host
├── start-client.sh / .ps1  # Start Angular client on host
├── stop-all.sh / .ps1      # Stop Docker services
└── README.md               # Additional details
```

## Docker Volumes

The development setup creates persistent Docker volumes:

- `web-debug_budgetdb-data`: PostgreSQL data (persistent across restarts)
- `web-debug_certs`: SSL certificates

To remove all volumes and start fresh:
```bash
docker compose down -v
```

## Benefits of This Development Setup

✅ **Fast Iteration**: Watch mode catches changes instantly  
✅ **Better Debugging**: Direct access to processes on host  
✅ **Isolated Dependencies**: Database runs in Docker  
✅ **Flexible Development**: Start/stop services independently  
✅ **Production-like**: SSL certificates and proper configuration

## Additional Resources

For more detailed information about the development scripts, see:
- [src/Hosts/web-debug/README.md](src/Hosts/web-debug/README.md) - Detailed documentation of development scripts

