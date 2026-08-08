---
inclusion: always
---

# Local Deployment Guide

## Quick Start

```bash
cd /home/ajjb/Repositories/MiniLibrary
docker compose -f docker/docker-compose.yml up -d --build
```

Wait ~45 seconds for all services to be healthy, then open: http://localhost:3000

## Services

| Service | Container | Port | URL |
|---------|-----------|------|-----|
| SQL Server 2022 | minilibrary-db | 1433 | localhost:1433 |
| API (ASP.NET Core 8) | minilibrary-api | 5000 | http://localhost:5000 |
| Frontend (React/Nginx) | minilibrary-web | 3000 | http://localhost:3000 |

## After Code Changes

### Frontend changes only (React/TypeScript):
```bash
docker compose -f docker/docker-compose.yml build web
docker compose -f docker/docker-compose.yml up -d web
```

### Backend changes (C#):
```bash
docker compose -f docker/docker-compose.yml build api
docker compose -f docker/docker-compose.yml up -d api
```

### Both:
```bash
docker compose -f docker/docker-compose.yml up -d --build
```

## Seed Data

To populate the local database with sample data (books, users, loans, ratings):
```bash
./scripts/seed-data.sh
```
This requires the API to be running and healthy. It's idempotent (safe to run multiple times).

## Checking Status

```bash
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
```

All three containers should show "healthy":
- minilibrary-db: healthy
- minilibrary-api: healthy
- minilibrary-web: running (may show unhealthy — that's OK, nginx healthcheck is basic)

## Environment Variables

Environment variables are loaded from `docker/.env`:
- `SA_PASSWORD` — SQL Server password
- `JWT_SECRET` — JWT signing key
- `GOOGLE_CLIENT_ID` / `GOOGLE_CLIENT_SECRET` — OAuth (optional)
- `MICROSOFT_CLIENT_ID` / `MICROSOFT_CLIENT_SECRET` — OAuth (optional)
- `OPENAI_API_KEY` — AI features (optional)
- `ENABLE_DEV_TOKENS` — true for Dev Login button

## Authentication

The Dev Login button on the login page (http://localhost:3000/login) allows logging in as any role without OAuth. Select Admin, Librarian, or Member and click "Dev Login as [Role]".

## Stopping Everything

```bash
docker compose -f docker/docker-compose.yml down
```

To also remove the database volume (wipes all data):
```bash
docker compose -f docker/docker-compose.yml down -v
```

## Troubleshooting

### API not starting (depends on SQL)
SQL Server takes ~30s to initialize on first run. The API has `start_period: 45s` and waits for SQL to be healthy.

### Database migrations
Migrations are applied automatically on API startup (`db.Database.Migrate()`). No manual step needed.

### Port conflicts
If port 5000 is in use, the API won't start. Check: `lsof -i :5000`

### Rebuild from scratch
```bash
docker compose -f docker/docker-compose.yml down -v
docker compose -f docker/docker-compose.yml up -d --build
# Wait ~45s then seed:
./scripts/seed-data.sh
```
