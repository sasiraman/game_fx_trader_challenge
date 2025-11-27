# Docker Quick Start

Get FX Trader Challenge running with Docker in 3 steps!

## Prerequisites

- Docker and Docker Compose installed
- Unity WebGL build in `WebGLBuild/` directory

## Quick Start

```bash
# 1. Build and start all services
docker-compose up -d

# 2. Check status
docker-compose ps

# 3. Access the game
# Open http://localhost:8080 in your browser
```

That's it! The game is now running:
- **WebGL Game**: http://localhost:8080
- **API Server**: http://localhost:3000

## Common Commands

```bash
# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Restart services
docker-compose restart

# Rebuild after code changes
docker-compose up -d --build
```

## Using Makefile (Optional)

If you have `make` installed:

```bash
make build    # Build images
make up       # Start services
make down     # Stop services
make logs     # View logs
make health   # Check health
make clean    # Remove everything
```

## Troubleshooting

**WebGL build not found?**
- Ensure `WebGLBuild/` directory exists with Unity build files
- Run Unity build first: `File → Build Settings → WebGL → Build`

**Port already in use?**
- Change ports in `docker-compose.yml`:
  ```yaml
  api:
    ports:
      - "3001:3000"  # Use different port
  ```

**Can't connect to API?**
- Check API is running: `curl http://localhost:3000/health`
- Update API URL in game config panel to match your setup

For more details, see [DOCKER_DEPLOYMENT.md](DOCKER_DEPLOYMENT.md)

