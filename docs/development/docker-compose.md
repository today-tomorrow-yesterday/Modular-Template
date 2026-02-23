# Docker Compose Development Guide

## Overview

Docker Compose provides a way to run the full Rtl.Core stack locally, simulating the production ECS deployment with separate containers per module.

## Quick Start

```bash
docker-compose up
```

This starts:
- All 6 module API containers
- PostgreSQL database
- Redis cache (optional)

## Services

| Service | Port | Description |
|---------|------|-------------|
| `orders-api` | 5001 | Orders module API |
| `sales-api` | 5002 | Sales module API |
| `customer-api` | 5003 | Customer module API |
| `organization-api` | 5005 | Organization module API |
| `sample-api` | 5006 | Sample module API |
| `postgres` | 5432 | PostgreSQL database |
| `redis` | 6379 | Redis cache |

## docker-compose.yml

```yaml
version: '3.8'

services:
  # Module APIs
  orders-api:
    build:
      context: .
      dockerfile: src/Api/Host.SampleOrders/Dockerfile
    ports:
      - "5001:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__Database=Host=postgres;Database=orders;Username=postgres;Password=postgres
      - ConnectionStrings__Cache=redis:6379
    depends_on:
      postgres:
        condition: service_healthy

  sales-api:
    build:
      context: .
      dockerfile: src/Api/Host.Sales/Dockerfile
    ports:
      - "5002:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__Database=Host=postgres;Database=sales;Username=postgres;Password=postgres
      - ConnectionStrings__Cache=redis:6379
    depends_on:
      postgres:
        condition: service_healthy

  # ... additional module services ...

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
CREATE DATABASE orders;
CREATE DATABASE sales;
CREATE DATABASE customer;
CREATE DATABASE organization;
CREATE DATABASE sample;
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
# Start only Orders module and dependencies
docker-compose up orders-api postgres redis
```

### Rebuild Images

```bash
# Rebuild all images
docker-compose build

# Rebuild specific image
docker-compose build orders-api

# Rebuild and start
docker-compose up --build
```

### View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f orders-api
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
docker-compose exec orders-api sh

# Access database
docker-compose exec postgres psql -U postgres
```

## Profiles

Use profiles to run subsets of services:

```yaml
services:
  orders-api:
    profiles: ["orders", "all"]
    # ...

  sales-api:
    profiles: ["sales", "all"]
    # ...
```

```bash
# Run only orders profile
docker-compose --profile orders up

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
docker-compose up orders-api -d

# View logs
docker-compose logs -f orders-api

# Make code changes, rebuild, restart
docker-compose up --build orders-api -d
```

### 3. Full Stack Testing

```bash
# Start everything
docker-compose up -d

# Run integration tests
dotnet test src/Modules/SampleOrders/test/Modules.SampleOrders.IntegrationTests

# Check all services
curl http://localhost:5001/health
curl http://localhost:5002/health
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
  orders-api:
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
┌─────────────────────────────────────────┐
│           docker-compose network        │
│                                         │
│  ┌─────────┐  ┌─────────┐  ┌────────┐  │
│  │orders-api│  │sales-api│  │postgres│  │
│  └────┬────┘  └────┬────┘  └───┬────┘  │
│       │            │           │        │
│       └────────────┴───────────┘        │
│                    │                    │
└────────────────────┼────────────────────┘
                     │
              Host Machine
         (localhost:5001, 5002, 5432)
```

## Troubleshooting

### Container Won't Start

```bash
# Check logs
docker-compose logs orders-api

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
docker-compose logs orders-api | grep -i connection
```

### Port Conflicts

```bash
# Find what's using the port
netstat -ano | findstr :5001

# Change port in docker-compose.yml
ports:
  - "5011:8080"  # Use different host port
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
