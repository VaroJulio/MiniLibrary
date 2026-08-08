# Azure Infrastructure - MiniLibrary Demo

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│  Azure (Demo Environment - ~$0/month)                    │
│                                                          │
│  ┌────────────────────┐   ┌────────────────────────┐   │
│  │ Azure Static Web   │   │ Azure Container Apps   │   │
│  │ Apps (Free)        │──▶│ (Free allowance)       │   │
│  │                    │   │                        │   │
│  │ React SPA          │   │ ASP.NET Core 8 API    │   │
│  │ (Vite build)       │   │ Image: ghcr.io/...    │   │
│  └────────────────────┘   └───────────┬────────────┘   │
│                                        │                 │
│                            ┌───────────▼────────────┐   │
│                            │ Azure SQL Database     │   │
│                            │ (Free: 32GB, auto-pause)│   │
│                            └────────────────────────┘   │
│                                                          │
│  Container image from: ghcr.io/varojulio/minilibrary-api │
└─────────────────────────────────────────────────────────┘
```

## Cost Breakdown (Demo)

| Resource | Tier | Cost |
|----------|------|------|
| Container Apps | Consumption (180K vCPU-sec/month free) | $0 |
| Static Web Apps | Free | $0 |
| Azure SQL | Free (32GB, auto-pause at 60min) | $0 |
| Log Analytics | Free tier (5GB/month) | $0 |
| **Total** | | **~$0/month** |

## Prerequisites

- Azure CLI installed (`az --version`)
- Logged in (`az login`)
- A subscription with access to create resources
- `jq` installed (for parsing deployment outputs)

## Deploy

### Option 1: Script (recommended)

```bash
# Set secrets (or script will prompt)
export SQL_ADMIN_PASSWORD="YourStr0ng!P@ssword"
export GOOGLE_CLIENT_ID="your-google-client-id.apps.googleusercontent.com"
export GOOGLE_CLIENT_SECRET="your-google-secret"

# Deploy
./infra/deploy.sh
```

### Option 2: Azure CLI directly

```bash
# Create resource group
az group create --name minilibrary-demo-rg --location eastus

# Deploy Bicep
az deployment group create \
  --resource-group minilibrary-demo-rg \
  --template-file infra/main.bicep \
  --parameters \
    sqlAdminPassword="YourStr0ng!P@ssword" \
    jwtSecret="$(openssl rand -hex 32)" \
    googleClientId="your-client-id" \
    googleClientSecret="your-secret"
```

### Option 3: GitHub Actions (CI/CD)

1. Add these secrets in GitHub repo settings:
   - `AZURE_CREDENTIALS` — Service principal JSON (see below)
   - `AZURE_STATIC_WEB_APPS_API_TOKEN` — From Static Web Apps resource

2. Trigger manually from Actions tab → "Deploy to Azure (Demo)"

#### Create Service Principal

```bash
az ad sp create-for-rbac \
  --name "minilibrary-github-deploy" \
  --role contributor \
  --scopes /subscriptions/{subscription-id}/resourceGroups/minilibrary-demo-rg \
  --json-auth
```

Copy the JSON output to GitHub secret `AZURE_CREDENTIALS`.

## Frontend Deployment (Static Web Apps)

### Via Azure Portal

1. Go to Azure Portal → Create "Static Web App"
2. Connect to GitHub repo `VaroJulio/MiniLibrary`
3. Build preset: **Custom**
4. App location: `src/MiniLibrary.Web`
5. Output location: `dist`
6. Build command: `npm run build`
7. Environment variable: `VITE_API_BASE_URL=https://minilibrary-api.<env-domain>/api`

### Via CLI

```bash
az staticwebapp create \
  --name minilibrary-web \
  --resource-group minilibrary-demo-rg \
  --source https://github.com/VaroJulio/MiniLibrary \
  --branch develop \
  --app-location src/MiniLibrary.Web \
  --output-location dist \
  --login-with-github
```

## Post-Deployment Configuration

### 1. Google OAuth

Add redirect URI in Google Cloud Console:
```
https://minilibrary-api.<container-apps-domain>/signin-google
```

### 2. Microsoft OAuth

Add redirect URI in Azure Entra ID:
```
https://minilibrary-api.<container-apps-domain>/signin-microsoft
```

### 3. Access SQL from local (optional)

```bash
# Add your IP to SQL firewall
az sql server firewall-rule create \
  --resource-group minilibrary-demo-rg \
  --server minilibrary-sql \
  --name AllowMyIP \
  --start-ip-address YOUR_IP \
  --end-ip-address YOUR_IP
```

## Environment Coexistence

| Setting | Local (docker/.env) | Azure (Container App env) |
|---------|--------------------|-----------------------------|
| DB | localhost:1433 | minilibrary-sql.database.windows.net |
| API URL | http://localhost:5000 | https://minilibrary-api.*.azurecontainerapps.io |
| Frontend | http://localhost:3000 | https://minilibrary-web.azurestaticapps.net |
| Dev Tokens | true | true (demo) / false (prod) |

No code changes needed between environments — only configuration via env vars.

## Teardown

```bash
# Delete everything (irreversible!)
az group delete --name minilibrary-demo-rg --yes --no-wait
```
