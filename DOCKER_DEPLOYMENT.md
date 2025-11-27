# Docker Deployment Guide

This guide explains how to deploy FX Trader Challenge using Docker.

## Prerequisites

- Docker Engine 20.10+ or Docker Desktop
- Docker Compose 2.0+
- Unity WebGL build in `WebGLBuild/` directory

## Quick Start

### 1. Build WebGL (if not already built)

First, build the Unity WebGL version:

1. Open Unity project
2. File → Build Settings → WebGL
3. Build to `WebGLBuild/` directory

### 2. Deploy with Docker Compose

```bash
# Build and start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

The game will be available at:
- **WebGL Game**: http://localhost:8080
- **API Server**: http://localhost:3000

## Individual Services

### Mock API Server

Build and run the API server:

```bash
cd mock_server
docker build -t fx-trader-api .
docker run -d -p 3000:3000 --name fx-trader-api fx-trader-api
```

### WebGL Server

Build and run the WebGL server:

```bash
# Ensure WebGLBuild/ directory exists with Unity build
docker build -f webgl-server/Dockerfile -t fx-trader-webgl .
docker run -d -p 8080:80 --name fx-trader-webgl fx-trader-webgl
```

## Configuration

### Environment Variables

#### API Server

Edit `docker-compose.yml` to set environment variables:

```yaml
api:
  environment:
    - PORT=3000
    - NODE_ENV=production
```

#### WebGL Server

The WebGL server uses nginx. To customize:

1. Edit `webgl-server/nginx.conf`
2. Rebuild the container:
   ```bash
   docker-compose build webgl
   docker-compose up -d webgl
   ```

### API Base URL

When running in Docker, update the API base URL in the game config:

1. Open http://localhost:8080
2. Use config panel to set API URL to: `http://localhost:3000`
3. Or edit `WebGLBuild/StreamingAssets/config.json`:
   ```json
   {
     "useBackend": true,
     "apiBaseUrl": "http://localhost:3000"
   }
   ```

**Note:** If accessing from a different host, use the host's IP address instead of `localhost`.

## Production Deployment

### Using Docker Compose

1. **Set up reverse proxy** (nginx/traefik) for SSL termination
2. **Update ports** in `docker-compose.yml`:
   ```yaml
   webgl:
     ports:
       - "80:80"  # Or use reverse proxy
   ```
3. **Use environment variables** for configuration
4. **Set up volumes** for persistent data (if needed)

### Example Production Setup

```yaml
version: '3.8'

services:
  api:
    build: ./mock_server
    ports:
      - "3000:3000"
    environment:
      - NODE_ENV=production
    restart: always
    # Add volume for persistent data
    volumes:
      - api-data:/app/data

  webgl:
    build:
      context: .
      dockerfile: webgl-server/Dockerfile
    ports:
      - "80:80"
    restart: always

volumes:
  api-data:
```

### Docker Swarm Deployment

```bash
# Initialize swarm
docker swarm init

# Deploy stack
docker stack deploy -c docker-compose.yml fx-trader

# View services
docker service ls

# Scale API service
docker service scale fx-trader_api=3
```

### Kubernetes Deployment

See `k8s/` directory for Kubernetes manifests (if provided).

## Health Checks

Both services include health checks:

- **API**: `GET /health` endpoint
- **WebGL**: HTTP check on root path

View health status:

```bash
docker-compose ps
```

## Logs

View logs:

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api
docker-compose logs -f webgl

# Last 100 lines
docker-compose logs --tail=100 api
```

## Troubleshooting

### WebGL Build Not Found

Ensure `WebGLBuild/` directory exists with Unity build files:
- `Build/` directory with `.wasm`, `.js`, `.data` files
- `index.html`
- `StreamingAssets/` (optional)

### CORS Errors

If accessing API from WebGL, ensure CORS is enabled:
- API server includes CORS headers
- nginx config includes CORS headers
- Check browser console for specific errors

### Port Conflicts

Change ports in `docker-compose.yml`:

```yaml
api:
  ports:
    - "3001:3000"  # Use port 3001 instead

webgl:
  ports:
    - "8081:80"  # Use port 8081 instead
```

### Container Won't Start

Check logs:
```bash
docker-compose logs api
docker-compose logs webgl
```

Check container status:
```bash
docker-compose ps
```

### API Connection Issues

1. Verify API is running: `curl http://localhost:3000/health`
2. Check API logs: `docker-compose logs api`
3. Verify network connectivity: `docker network inspect fx-trader_fx-trader-network`

## Building for Production

### Optimize Images

```bash
# Use multi-stage builds (already implemented)
docker build --target production -t fx-trader-api ./mock_server

# Use BuildKit for faster builds
DOCKER_BUILDKIT=1 docker-compose build
```

### Security Best Practices

1. **Don't run as root** (already using node:alpine and nginx:alpine)
2. **Use secrets** for sensitive data:
   ```yaml
   secrets:
     jwt_secret:
       file: ./secrets/jwt_secret.txt
   ```
3. **Scan images** for vulnerabilities:
   ```bash
   docker scan fx-trader-api
   ```
4. **Use specific tags** instead of `latest`

## Monitoring

### Health Monitoring

```bash
# Check health
docker-compose ps

# Manual health check
curl http://localhost:3000/health
curl http://localhost:8080/
```

### Resource Usage

```bash
# Container stats
docker stats

# Service stats (Swarm)
docker service ps fx-trader_api
```

## Backup and Restore

### API Data

If using volumes for persistent data:

```bash
# Backup
docker run --rm -v fx-trader_api-data:/data -v $(pwd):/backup \
  alpine tar czf /backup/api-backup.tar.gz /data

# Restore
docker run --rm -v fx-trader_api-data:/data -v $(pwd):/backup \
  alpine tar xzf /backup/api-backup.tar.gz -C /
```

## Cleanup

```bash
# Stop and remove containers
docker-compose down

# Remove volumes (WARNING: deletes data)
docker-compose down -v

# Remove images
docker-compose down --rmi all

# Clean up everything
docker system prune -a
```

## Next Steps

- Set up SSL/TLS with Let's Encrypt
- Configure reverse proxy (nginx/traefik)
- Set up monitoring (Prometheus/Grafana)
- Configure logging aggregation (ELK stack)
- Set up CI/CD pipeline

