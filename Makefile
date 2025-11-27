.PHONY: help build up down logs clean restart api webgl

help: ## Show this help message
	@echo 'Usage: make [target]'
	@echo ''
	@echo 'Available targets:'
	@awk 'BEGIN {FS = ":.*?## "} /^[a-zA-Z_-]+:.*?## / {printf "  %-15s %s\n", $$1, $$2}' $(MAKEFILE_LIST)

build: ## Build all Docker images
	docker-compose build

up: ## Start all services
	docker-compose up -d

down: ## Stop all services
	docker-compose down

logs: ## View logs from all services
	docker-compose logs -f

logs-api: ## View API server logs
	docker-compose logs -f api

logs-webgl: ## View WebGL server logs
	docker-compose logs -f webgl

restart: ## Restart all services
	docker-compose restart

clean: ## Stop and remove all containers, networks, and volumes
	docker-compose down -v

api: ## Build and run API server only
	cd mock_server && docker build -t fx-trader-api . && docker run -d -p 3000:3000 --name fx-trader-api fx-trader-api

webgl: ## Build and run WebGL server only (requires WebGLBuild/)
	docker build -f webgl-server/Dockerfile -t fx-trader-webgl . && docker run -d -p 8080:80 --name fx-trader-webgl fx-trader-webgl

ps: ## Show running containers
	docker-compose ps

health: ## Check health of all services
	@echo "Checking API health..."
	@curl -s http://localhost:3000/health || echo "API not responding"
	@echo ""
	@echo "Checking WebGL health..."
	@curl -s -o /dev/null -w "HTTP Status: %{http_code}\n" http://localhost:8080/ || echo "WebGL not responding"

stop-api: ## Stop API server
	docker stop fx-trader-api || true
	docker rm fx-trader-api || true

stop-webgl: ## Stop WebGL server
	docker stop fx-trader-webgl || true
	docker rm fx-trader-webgl || true

