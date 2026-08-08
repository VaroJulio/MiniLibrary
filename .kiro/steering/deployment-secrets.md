---
inclusion: manual
---

# Deployment Secrets Configuration

## Secret Locations

| Secret | Local (.env) | GitHub Secrets | Azure (infra/deploy.sh) |
|--------|-------------|---------------|-------------------------|
| OPENAI_API_KEY | `.env` and `docker/.env` | Yes (set) | Passed via deploy.sh env var |
| SQL_ADMIN_PASSWORD | `.env` | Yes (set) | Prompted or env var |
| GOOGLE_CLIENT_ID | `docker/.env` | Yes (set) | Passed via deploy.sh |
| GOOGLE_CLIENT_SECRET | `docker/.env` | Yes (set) | Passed via deploy.sh |
| AZURE_CREDENTIALS | N/A | Pending (create after first deploy) | N/A |
| AZURE_STATIC_WEB_APPS_API_TOKEN | N/A | Pending (create after first deploy) | N/A |

## Azure Subscription

- Name: Visual Studio Enterprise Subscription - MPN
- ID: e7559d11-d17a-4fd9-b319-08473493c822
- Login: `az login` (already configured)

## Docker Image

- Registry: ghcr.io
- Image: `ghcr.io/varojulio/minilibrary-api:latest`
- Login: `gh auth token | docker login ghcr.io -u VaroJulio --password-stdin`
- Build: `docker build -f docker/Dockerfile.api -t ghcr.io/varojulio/minilibrary-api:latest .`
- Push: `docker push ghcr.io/varojulio/minilibrary-api:latest`

## Deploy Command (Manual)

```bash
# From repo root:
export SQL_ADMIN_PASSWORD="$(grep SQL_ADMIN_PASSWORD .env | cut -d= -f2)"
export GOOGLE_CLIENT_ID="$(grep GOOGLE_CLIENT_ID docker/.env | cut -d= -f2)"
export GOOGLE_CLIENT_SECRET="$(grep GOOGLE_CLIENT_SECRET docker/.env | cut -d= -f2)"
export OPENAI_API_KEY="$(grep OPENAI_API_KEY .env | cut -d= -f2)"
./infra/deploy.sh
```

## Post-Deploy: Create AZURE_CREDENTIALS

After first deploy, create a service principal for GitHub Actions:

```bash
az ad sp create-for-rbac \
  --name "minilibrary-github-deploy" \
  --role contributor \
  --scopes /subscriptions/e7559d11-d17a-4fd9-b319-08473493c822/resourceGroups/minilibrary-demo-rg \
  --json-auth
```

Then: `echo '<json-output>' | gh secret set AZURE_CREDENTIALS`

## Teardown

```bash
az group delete --name minilibrary-demo-rg --yes --no-wait
```

## Make Docker Image Public (Required for Azure)

The ghcr.io package must be **public** so Azure Container Apps can pull without auth.

1. Go to: https://github.com/users/VaroJulio/packages/container/package/minilibrary-api
2. Click "Package settings" (gear icon)
3. Scroll to "Danger Zone" → Change visibility → Public

Alternatively, configure registry credentials in the Bicep template (more complex).

## Important Notes

- `.env` and `docker/.env` are in `.gitignore` — NEVER commit them
- OpenAI key is a service account key (sk-svcacct-...) 
- SQL_ADMIN_PASSWORD must meet Azure complexity requirements (uppercase, lowercase, number, special char)
- The ghcr.io image must be public OR the Container App needs registry credentials configured
