# Azure Infrastructure - MiniLibrary

## Live Environment

| Component | Service | URL |
|-----------|---------|-----|
| **Frontend** | Azure Static Web Apps (Free) | https://kind-sea-0f0e98210.7.azurestaticapps.net |
| **API** | Azure Container Apps (Consumption) | https://minilibrary-api.icymoss-654fea4b.centralus.azurecontainerapps.io |
| **Database** | Azure SQL (Free tier) | minilibrary-sql.database.windows.net |
| **Container Image** | GitHub Container Registry | ghcr.io/varojulio/minilibrary-api:latest |

- **Region**: Central US
- **Resource Group**: `minilibrary-demo`
- **Subscription**: Visual Studio Enterprise Subscription – MPN

## Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│  Azure — minilibrary-demo (Central US) — ~$0/month                   │
│                                                                       │
│  ┌──────────────────────┐        ┌─────────────────────────────┐    │
│  │ Azure Static Web Apps│  HTTPS │ Azure Container Apps         │    │
│  │ (CDN, Free tier)     │───────▶│ (Consumption, scale-to-zero)│    │
│  │                      │        │                              │    │
│  │ React 18 SPA         │        │ ASP.NET Core 8 API          │    │
│  │ Vite build → dist/   │        │ Docker: ghcr.io/varojulio/  │    │
│  │ No container needed  │        │   minilibrary-api:latest    │    │
│  │                      │        │ 0.25 vCPU / 0.5 GB RAM      │    │
│  └──────────────────────┘        └──────────────┬──────────────┘    │
│                                                   │                   │
│                                   ┌───────────────▼──────────────┐   │
│                                   │ Azure SQL Database            │   │
│                                   │ Free tier (32 GB, Gen5)       │   │
│                                   │ Auto-pause after 60 min idle  │   │
│                                   │ 100K vCore-seconds/month      │   │
│                                   └──────────────────────────────┘   │
│                                                                       │
│  ┌──────────────────────┐                                            │
│  │ Log Analytics        │                                            │
│  │ Free tier (5 GB/mo)  │ ← Container Apps logs                     │
│  └──────────────────────┘                                            │
└─────────────────────────────────────────────────────────────────────┘

External:
  ghcr.io/varojulio/minilibrary-api:latest  ← Public container image
  OpenAI API (text-embedding-3-small, GPT-4o-mini) ← AI features
```

## Cost Breakdown

| Resource | Tier | Monthly Cost |
|----------|------|:------------:|
| Azure Container Apps | Consumption (180K vCPU-sec/mo free, scale-to-zero) | $0 |
| Azure Static Web Apps | Free (100 GB bandwidth, custom domains) | $0 |
| Azure SQL Database | Free (32 GB, auto-pause, 100K vCore-sec/mo) | $0 |
| Log Analytics | Free (5 GB ingestion/mo) | $0 |
| GitHub Container Registry | Free (public image) | $0 |
| **Total** | | **$0/month** |

> Note: The database auto-pauses after 60 minutes of inactivity. First request after pause takes ~30 seconds while it resumes.

## Frontend: Not Containerized

The React frontend is **not** running in a container. It's deployed as static files (HTML/JS/CSS) to Azure Static Web Apps:

- `npm run build` produces a `dist/` folder
- Those files are uploaded directly to Azure's global CDN
- No server/runtime needed — it's served as pure static content
- SPA routing handled via `staticwebapp.config.json` (all paths → index.html)

The `docker/Dockerfile.web` exists only for **local development** with Docker Compose (uses Nginx). Production uses Static Web Apps because it's faster, free, and globally distributed.

## How it was Deployed

### 1. API Infrastructure (Bicep + deploy.sh)

```bash
export SQL_ADMIN_PASSWORD="<password>"
export GOOGLE_CLIENT_ID="<id>"
export GOOGLE_CLIENT_SECRET="<secret>"
export OPENAI_API_KEY="<key>"
export LOCATION="centralus"
export RESOURCE_GROUP="minilibrary-demo"
./infra/deploy.sh
```

This creates: Resource Group → SQL Server + DB → Container Apps Environment → Container App (pulling from ghcr.io).

### 2. Frontend (SWA CLI)

```bash
# Build with Azure API URL baked in
cd src/MiniLibrary.Web
VITE_API_BASE_URL=https://minilibrary-api.icymoss-654fea4b.centralus.azurecontainerapps.io/api npm run build

# Create Static Web App resource
az staticwebapp create --name minilibrary-web --resource-group minilibrary-demo --location centralus --sku Free

# Get deployment token
SWA_TOKEN=$(az staticwebapp secrets list --name minilibrary-web --resource-group minilibrary-demo --query "properties.apiKey" -o tsv)

# Deploy built files
SWA_CLI_DEPLOYMENT_TOKEN=$SWA_TOKEN swa deploy dist --env production
```

### 3. Seed Data

```bash
API_BASE_URL=https://minilibrary-api.icymoss-654fea4b.centralus.azurecontainerapps.io ./scripts/seed-data.sh
```

Creates 6 users, 24 books, 19 loans, 16 ratings via API calls.

### 4. Docker Image (API)

```bash
# Login to ghcr.io
gh auth token | docker login ghcr.io -u VaroJulio --password-stdin

# Build and push
docker build -f docker/Dockerfile.api -t ghcr.io/varojulio/minilibrary-api:latest .
docker push ghcr.io/varojulio/minilibrary-api:latest
```

Image is **public** so Azure Container Apps can pull without credentials.

## GitHub Secrets & Variables

| Name | Type | Purpose |
|------|------|---------|
| `OPENAI_API_KEY` | Secret | AI recommendations & semantic search |
| `SQL_ADMIN_PASSWORD` | Secret | Azure SQL admin password |
| `GOOGLE_CLIENT_ID` | Secret | Google OAuth |
| `GOOGLE_CLIENT_SECRET` | Secret | Google OAuth |
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | Secret | Frontend deployment via GitHub Actions |
| `AZURE_API_URL` | Variable | API base URL for frontend builds |

Still pending (for fully automated pipeline):
- `AZURE_CREDENTIALS` — Service principal JSON for Container Apps deployment

## Environment Variables in Container App

| Variable | Value |
|----------|-------|
| `ASPNETCORE_ENVIRONMENT` | Production |
| `ASPNETCORE_URLS` | http://+:5000 |
| `ConnectionStrings__DefaultConnection` | Server=tcp:minilibrary-sql.database.windows.net... |
| `Jwt__Secret` | (auto-generated at deploy) |
| `Authentication__EnableDevTokens` | true (demo) |
| `Authentication__Google__ClientId` | (from secret) |
| `Authentication__Google__ClientSecret` | (from secret) |
| `OpenAI__ApiKey` | (from secret) |
| `App__PublicUrl` | https://minilibrary-api.icymoss-654fea4b.centralus.azurecontainerapps.io |
| `App__FrontendUrl` | https://kind-sea-0f0e98210.7.azurestaticapps.net |

## Useful Commands

### Check API health
```bash
curl https://minilibrary-api.icymoss-654fea4b.centralus.azurecontainerapps.io/health
```

### Get a dev token
```bash
curl -X POST https://minilibrary-api.icymoss-654fea4b.centralus.azurecontainerapps.io/api/auth/dev-token \
  -H "Content-Type: application/json" \
  -d '{"role":"Admin","name":"Test Admin","email":"test@demo.com"}'
```

### Search books (with token)
```bash
TOKEN=$(curl -s -X POST .../api/auth/dev-token -H "Content-Type: application/json" -d '{"role":"Member"}' | jq -r .accessToken)
curl -H "Authorization: Bearer $TOKEN" .../api/search/books?query=dune
```

### View Container App logs
```bash
az containerapp logs show --name minilibrary-api --resource-group minilibrary-demo --follow
```

### Restart the API container
```bash
az containerapp revision restart --name minilibrary-api --resource-group minilibrary-demo --revision $(az containerapp revision list --name minilibrary-api --resource-group minilibrary-demo --query "[0].name" -o tsv)
```

### Redeploy API with new image
```bash
docker build -f docker/Dockerfile.api -t ghcr.io/varojulio/minilibrary-api:latest .
docker push ghcr.io/varojulio/minilibrary-api:latest
az containerapp update --name minilibrary-api --resource-group minilibrary-demo --image ghcr.io/varojulio/minilibrary-api:latest
```

### Redeploy frontend
```bash
cd src/MiniLibrary.Web
VITE_API_BASE_URL=https://minilibrary-api.icymoss-654fea4b.centralus.azurecontainerapps.io/api npm run build
SWA_CLI_DEPLOYMENT_TOKEN=$(az staticwebapp secrets list --name minilibrary-web --resource-group minilibrary-demo --query "properties.apiKey" -o tsv) swa deploy dist --env production
```

## OAuth Configuration (Post-Deploy)

Add these redirect URIs in your OAuth providers:

**Google Cloud Console:**
```
https://minilibrary-api.icymoss-654fea4b.centralus.azurecontainerapps.io/signin-google
```

**Microsoft Entra ID (Azure AD):**
```
https://minilibrary-api.icymoss-654fea4b.centralus.azurecontainerapps.io/signin-microsoft
```

## Teardown

```bash
# Delete ALL Azure resources (irreversible!)
az group delete --name minilibrary-demo --yes --no-wait
```

This removes: Container App, SQL Server, Static Web App, Log Analytics workspace, and all associated data.

## Local vs Azure Environment

| Aspect | Local (Docker Compose) | Azure (Production Demo) |
|--------|----------------------|--------------------------|
| Frontend | Nginx container (port 3000) | Static Web Apps CDN |
| API | Docker container (port 5000) | Container Apps (port 5000) |
| Database | SQL Server container (port 1433) | Azure SQL (auto-pause) |
| Auth | Dev tokens enabled | Dev tokens enabled (demo) |
| AI | OpenAI API (optional) | OpenAI API (configured) |
| Cost | Free (your machine) | Free (Azure free tiers) |
| Startup | `docker compose up -d --build` | Already running (scale-to-zero) |
| Seed data | `./scripts/seed-data.sh` | `API_BASE_URL=https://... ./scripts/seed-data.sh` |

No code changes needed between environments — only environment variables differ.
