# Docker Compose Development Guide

## Overview

Docker Compose provides a way to run the full Rtl.Core stack locally, simulating the production ECS deployment with separate containers per module.

## Quick Start

```bash
docker-compose up
```

This starts:
- All module API containers
- PostgreSQL database
- Redis cache (optional)

## Services

| Service | Port | Description |
|---------|------|-------------|
| `api` | 5000 | All modules (monolith) |
| `sampleorders-api` | 5002 | SampleOrders module API |
| `samplesales-api` | 5003 | SampleSales module API |
| `postgres` | 5432 | PostgreSQL database |
| `redis` | 6379 | Redis cache |

## docker-compose.yml

```yaml
version: '3.8'

services:
  # Monolith API (All Modules)
  api:
    build:
      context: .
      dockerfile: src/Api/Host/Dockerfile
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__Database=Host=postgres;Database=rtlcore;Username=postgres;Password=postgres
      - ConnectionStrings__Cache=redis:6379
    depends_on:
      postgres:
        condition: service_healthy

  # Module APIs (Independent Deployment)
  sampleorders-api:
    build:
      context: .
      dockerfile: src/Api/Host.SampleOrders/Dockerfile
    ports:
      - "5002:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__Database=Host=postgres;Database=sampleorders;Username=postgres;Password=postgres
      - ConnectionStrings__Cache=redis:6379
    depends_on:
      postgres:
        condition: service_healthy

  samplesales-api:
    build:
      context: .
      dockerfile: src/Api/Host.SampleSales/Dockerfile
    ports:
      - "5003:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__Database=Host=postgres;Database=samplesales;Username=postgres;Password=postgres
      - ConnectionStrings__Cache=redis:6379
    depends_on:
      postgres:
        condition: service_healthy

  # Infrastructure
  postgres:
    image: postgres:16
    environment:
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data
      - ./init-databases.sql:/docker-entrypoint-initdb.d/init.sql
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 5s
      timeout: 5s
      retries: 5

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data

volumes:
  postgres-data:
  redis-data:
```

## Database Initialization

Create `init-databases.sql` to initialize all module databases:

```sql
-- init-databases.sql
CREATE DATABASE sampleorders;
CREATE DATABASE samplesales;
```

## Common Commands

### Start All Services

```bash
# Start in foreground (see logs)
docker-compose up

# Start in background
docker-compose up -d
```

### Start Specific Services

```bash
# Start only SampleOrders module and dependencies
docker-compose up sampleorders-api postgres redis
```

### Rebuild Images

```bash
# Rebuild all images
docker-compose build

# Rebuild specific image
docker-compose build sampleorders-api

# Rebuild and start
docker-compose up --build
```

### View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f sampleorders-api
```

### Stop Services

```bash
# Stop and remove containers
docker-compose down

# Stop, remove containers, and volumes
docker-compose down -v
```

### Shell Access

```bash
# Access running container
docker-compose exec sampleorders-api sh

# Access database
docker-compose exec postgres psql -U postgres
```

## Profiles

Use profiles to run subsets of services:

```yaml
services:
  sampleorders-api:
    profiles: ["sampleorders", "all"]
    # ...

  samplesales-api:
    profiles: ["samplesales", "all"]
    # ...
```

```bash
# Run only sampleorders profile
docker-compose --profile sampleorders up

# Run all services
docker-compose --profile all up
```

## Development Workflow

### 1. Initial Setup

```bash
# Build all images
docker-compose build

# Start infrastructure only
docker-compose up postgres redis -d

# Wait for healthy status
docker-compose ps
```

### 2. Daily Development

```bash
# Start the module you're working on
docker-compose up sampleorders-api -d

# View logs
docker-compose logs -f sampleorders-api

# Make code changes, rebuild, restart
docker-compose up --build sampleorders-api -d
```

### 3. Full Stack Testing

```bash
# Start everything
docker-compose up -d

# Check all services
curl http://localhost:5000/health
curl http://localhost:5002/health
curl http://localhost:5003/health
```

## Environment Variables

Override settings via environment variables:

```bash
# Command line
ConnectionStrings__Database="Host=custom-host;..." docker-compose up

# .env file
echo 'POSTGRES_PASSWORD=custom-password' > .env
docker-compose up
```

## Resource Limits

Add resource limits for production-like behavior:

```yaml
services:
  sampleorders-api:
    deploy:
      resources:
        limits:
          cpus: '0.5'
          memory: 512M
        reservations:
          cpus: '0.25'
          memory: 256M
```

## Networking

Services communicate using Docker's internal network:

```
┌──────────────────────────────────────────────────┐
│              docker-compose network              │
│                                                  │
│  ┌──────────────┐  ┌──────────────┐  ┌────────┐ │
│  │sampleorders  │  │samplesales   │  │postgres│ │
│  │    -api      │  │    -api      │  │        │ │
│  └──────┬───────┘  └──────┬───────┘  └───┬────┘ │
│         │                 │              │       │
│         └─────────────────┴──────────────┘       │
│                           │                      │
└───────────────────────────┼──────────────────────┘
                            │
                     Host Machine
              (localhost:5002, 5003, 5432)
```

## Troubleshooting

### Container Won't Start

```bash
# Check logs
docker-compose logs sampleorders-api

# Check container status
docker-compose ps

# Rebuild from scratch
docker-compose down -v
docker-compose build --no-cache
docker-compose up
```

### Database Connection Issues

```bash
# Verify database is healthy
docker-compose ps postgres

# Check database exists
docker-compose exec postgres psql -U postgres -c '\l'

# View connection errors
docker-compose logs sampleorders-api | grep -i connection
```

### Port Conflicts

```bash
# Find what's using the port
netstat -ano | findstr :5002

# Change port in docker-compose.yml
ports:
  - "5012:8080"  # Use different host port
```

### Slow Builds

```bash
# Use BuildKit
DOCKER_BUILDKIT=1 docker-compose build

# Cache NuGet packages
# (already configured in Dockerfile)
```

## Comparison with Other Options

| Approach | Startup Time | Resource Usage | Production Similarity |
|----------|--------------|----------------|----------------------|
| `dotnet run` (all modules) | Fast | Low | Low |
| `dotnet run` (single module) | Fastest | Lowest | Medium |
| Docker Compose | Slow | High | High |
| Kubernetes (minikube) | Slowest | Highest | Highest |

## Related Documentation

- [Local Development](./local-development.md)
- [Module Deployment Architecture](../architecture/module-deployment.md)
- [Dockerfile Reference](../deployment/github-actions.md)
