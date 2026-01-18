# Budget Application - Production Release

This directory contains the production-ready Docker Compose configuration for the Budget application with HTTPS support, automatic database backups, and proper service orchestration.

## Services Overview

- **budget-client** - Angular client application (served via ASP.NET Core)
- **budget-server** - ASP.NET Core API server
- **postgres** - PostgreSQL 17 database
- **pgbackups** - Automated database backup service
- **nginx** - Reverse proxy with HTTPS support
- **cert-generator** - Self-signed SSL certificate generator (uses [nvs-certgen](https://github.com/nvsnkv/certgen))

## Prerequisites

- Docker and Docker Compose installed
- Access to Docker registry with the budget images
- Yandex OAuth credentials (for authentication)

## Quick Start

### 1. Setup Environment Variables

Copy the example environment file and configure it:

```bash
cp .env.example .env
```

Edit `.env` and configure the following **required** variables:

```env
# Database password (change this!)
POSTGRES_PASSWORD=your_secure_password_here

# Backup location (absolute path)
BACKUP_PATH=/path/to/your/backups

# SSL certificate hostname
CERT_HOSTNAME=budget.yourdomain.com

# Docker registry details
DOCKER_REGISTRY=docker.io
DOCKER_NAMESPACE=yournamespace
IMAGE_VERSION=latest

# Yandex OAuth credentials
YANDEX_OAUTH_CLIENT_ID=your_yandex_client_id
YANDEX_OAUTH_CLIENT_SECRET=your_yandex_client_secret
```

### 2. Pull or Build Images

If you need to build the images first:

```bash
# Navigate to the Hosts directory
cd ../

# Build and push images using the release script
.\Release-DockerImages.ps1 -Version "1.0.0"

# Or just build locally
.\Release-DockerImages.ps1 -Version "1.0.0" -SkipPush
```

If images are already in your registry, they will be pulled automatically.

### 3. Start the Application

```bash
docker-compose up -d
```

This will:
1. Create the network and volumes
2. Start PostgreSQL database
3. Generate self-signed SSL certificates
4. Start the budget-server and budget-client
5. Configure nginx as reverse proxy
6. Start the backup service

### 4. Access the Application

- **HTTPS**: `https://localhost:25000` (or your configured port)
- **HTTP**: `http://localhost:25001` (redirects to HTTPS)

**Note**: Since we're using self-signed certificates, you'll need to accept the security warning in your browser.

## Configuration Details

### Port Configuration

The application exposes two ports (configurable via `.env`):

- `NGINX_PORT` (default: 25000) - HTTPS port
- `NGINX_HTTP_PORT` (default: 25001) - HTTP port (redirects to HTTPS)

**Note**: The frontend URL is automatically constructed as `https://${CERT_HOSTNAME}:${NGINX_PORT}` and passed to the budget-server for CORS and OAuth redirects.

### Routing Rules

Nginx routes requests based on path:

- `/api/*` → budget-server
- `/admin/*` → budget-server
- `/*` (everything else) → budget-client

### Database Backups

The `pgbackups` service automatically backs up the PostgreSQL database:

- **Schedule**: Daily by default (configurable via `BACKUP_SCHEDULE`)
- **Retention**: 
  - Daily backups: 7 days
  - Weekly backups: 4 weeks
  - Monthly backups: 6 months
- **Location**: Specified by `BACKUP_PATH` in `.env`

**Important**: Ensure the backup path exists and has proper permissions:

```bash
# Linux/Mac
mkdir -p /path/to/your/backups
chmod 755 /path/to/your/backups

# Windows
New-Item -Path "C:\backups\budget" -ItemType Directory -Force
```

### SSL Certificates

The application uses [nvs-certgen](https://github.com/nvsnkv/certgen) to automatically generate self-signed SSL certificates on first run. The certificate is valid for 365 days by default (configurable via `CERT_DAYS`).

Certificate files:
- `tls.crt` - Certificate file
- `tls.key` - Private key

For production use with real domain names, you should:

1. **Option A**: Use Let's Encrypt with Certbot
2. **Option B**: Replace the `cert-generator` service with your own certificates:

```yaml
# Mount your own certificates (must be named tls.crt and tls.key)
nginx:
  volumes:
    - ./my-certs:/etc/nginx/certs:ro
```

**Note**: If you need to regenerate certificates (e.g., after hostname change or expiration), remove the certs volume:

```bash
docker-compose down
docker volume rm web-release_certs
docker-compose up -d
```

## Management Commands

### View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f budget-server
docker-compose logs -f budget-client
docker-compose logs -f nginx
```

### Stop Application

```bash
docker-compose down
```

### Stop and Remove Data

```bash
# Warning: This will delete all data including database!
docker-compose down -v
```

### Restart a Service

```bash
docker-compose restart budget-server
```

### Update to New Version

```bash
# Update .env with new version
# IMAGE_VERSION=1.0.1

# Pull new images and restart
docker-compose pull
docker-compose up -d
```

### Check Service Health

```bash
docker-compose ps
```

## Backup and Restore

### Manual Backup

```bash
docker-compose exec postgres pg_dump -U postgres budgetdb > backup.sql
```

### Restore from Backup

```bash
# Stop the application
docker-compose down

# Remove old database volume
docker volume rm web-release_postgres-data

# Start only postgres
docker-compose up -d postgres

# Wait for postgres to be ready (check logs)
docker-compose logs -f postgres

# Restore backup
cat backup.sql | docker-compose exec -T postgres psql -U postgres budgetdb

# Start all services
docker-compose up -d
```

### Access Automated Backups

Backups are stored in the directory specified by `BACKUP_PATH`:

```bash
# List backups
ls -lah /path/to/your/backups

# Backups are named: daily/budgetdb-YYYYMMDD-HHMMSS.sql.gz
```

## Troubleshooting

### Cannot Connect to Application

1. Check all services are running:
   ```bash
   docker-compose ps
   ```

2. Check nginx logs:
   ```bash
   docker-compose logs nginx
   ```

3. Verify certificates were generated:
   ```bash
   docker-compose logs cert-generator
   ```

### Database Connection Issues

1. Check postgres is healthy:
   ```bash
   docker-compose ps postgres
   ```

2. Check server logs:
   ```bash
   docker-compose logs budget-server
   ```

3. Verify database credentials in `.env`

### OAuth Authentication Issues

1. Verify Yandex OAuth credentials in `.env`
2. Check that the redirect URI is configured in Yandex OAuth settings
3. Check server logs for authentication errors

### Backup Issues

1. Verify `BACKUP_PATH` exists and is accessible:
   ```bash
   # Linux/Mac
   ls -ld /path/to/your/backups
   
   # Windows
   Test-Path "C:\backups\budget"
   ```

2. Check backup service logs:
   ```bash
   docker-compose logs pgbackups
   ```

### Certificate Issues

If you encounter certificate issues or need to regenerate certificates:

1. Check cert-generator logs:
   ```bash
   docker-compose logs cert-generator
   ```

2. Regenerate certificates:
   ```bash
   # Remove the certs volume
   docker-compose down
   docker volume rm web-release_certs

   # Restart (will regenerate certificates)
   docker-compose up -d
   ```

3. Verify hostname configuration in `.env` matches your domain

## Production Recommendations

1. **Use Real SSL Certificates**: Replace self-signed certificates with proper SSL certificates from Let's Encrypt or a certificate authority

2. **Secure Database Password**: Use a strong, randomly generated password for PostgreSQL

3. **Regular Backups**: Monitor the backup service and test restores regularly

4. **Resource Limits**: Add resource limits to services in docker-compose.yml:
   ```yaml
   deploy:
     resources:
       limits:
         cpus: '1'
         memory: 1G
   ```

5. **Monitoring**: Consider adding monitoring services (Prometheus, Grafana)

6. **Reverse Proxy**: If running on a server, consider using a proper reverse proxy like Traefik or Caddy for automatic HTTPS with Let's Encrypt

7. **Firewall**: Ensure only necessary ports are exposed to the internet

8. **Updates**: Regularly update Docker images and apply security patches

## Environment Variables Reference

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `POSTGRES_DB` | Database name | budgetdb | No |
| `POSTGRES_USER` | Database user | postgres | No |
| `POSTGRES_PASSWORD` | Database password | - | **Yes** |
| `BACKUP_PATH` | Path for database backups | - | **Yes** |
| `BACKUP_SCHEDULE` | Cron schedule for backups | @daily | No |
| `BACKUP_KEEP_DAYS` | Days to keep daily backups | 7 | No |
| `BACKUP_KEEP_WEEKS` | Weeks to keep weekly backups | 4 | No |
| `BACKUP_KEEP_MONTHS` | Months to keep monthly backups | 6 | No |
| `CERT_HOSTNAME` | SSL certificate hostname | localhost | No |
| `CERT_DAYS` | Certificate validity in days | 365 | No |
| `CERTGEN_VERSION` | nvs-certgen image version | latest | No |
| `DOCKER_REGISTRY` | Docker registry URL (used for all images) | docker.io | **Yes** |
| `DOCKER_NAMESPACE` | Docker namespace/username | - | **Yes** |
| `IMAGE_VERSION` | Image version tag | latest | No |
| `NGINX_PORT` | HTTPS port | 25000 | No |
| `NGINX_HTTP_PORT` | HTTP port | 25001 | No |
| `YANDEX_OAUTH_CLIENT_ID` | Yandex OAuth client ID | - | **Yes** |
| `YANDEX_OAUTH_CLIENT_SECRET` | Yandex OAuth client secret | - | **Yes** |

## Support

For issues or questions, please refer to the main project documentation or contact the development team.
