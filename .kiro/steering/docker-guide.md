---
inclusion: fileMatch
fileMatchPattern: "**/docker*,**/Docker*,**/.env*"
---

# Docker Local Development Guide

## Quick Start
```bash
# Start all services
docker compose up -d

# Start only infrastructure (DB)
docker compose up -d sqlserver

# Rebuild after code changes
docker compose up -d --build api web

# View logs
docker compose logs -f api

# Stop all services
docker compose down

# Stop and remove volumes (clean slate)
docker compose down -v
```

## Services
| Service    | Port  | Description           |
|-----------|-------|-----------------------|
| api       | 5000  | ASP.NET Core Web API  |
| web       | 3000  | React Frontend (Vite) |
| sqlserver | 1433  | SQL Server 2022       |

## Environment Variables
All secrets are stored in `.env` file (never committed):
- `SA_PASSWORD` - SQL Server SA password
- `GITHUB_TOKEN` - GitHub PAT for CI/CD
- `JIRA_API_TOKEN` - Jira API token
- `JIRA_EMAIL` - Jira email
- `OPENAI_API_KEY` - OpenAI API key (for AI features)

## Database Connection
- Server: `localhost,1433`
- Database: `MiniLibraryDb`
- User: `sa`
- Password: from `SA_PASSWORD` env var

## Running Migrations
```bash
# From src/MiniLibrary.Infrastructure
dotnet ef migrations add <MigrationName> --startup-project ../MiniLibrary.API
dotnet ef database update --startup-project ../MiniLibrary.API
```
