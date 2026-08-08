#!/bin/bash
set -e

# =============================================================================
# MiniLibrary Azure Deployment Script
# Deploys: Container Apps (API) + Azure SQL Free
# Frontend: Deploy separately via Azure Static Web Apps (GitHub integration)
# =============================================================================

# Configuration
RESOURCE_GROUP="${RESOURCE_GROUP:-minilibrary-demo-rg}"
LOCATION="${LOCATION:-eastus}"
BASE_NAME="${BASE_NAME:-minilibrary}"
API_IMAGE="${API_IMAGE:-ghcr.io/varojulio/minilibrary-api:latest}"

echo "╔══════════════════════════════════════════════╗"
echo "║  MiniLibrary Azure Deployment               ║"
echo "╠══════════════════════════════════════════════╣"
echo "║  Resource Group: $RESOURCE_GROUP"
echo "║  Location:       $LOCATION"
echo "║  API Image:      $API_IMAGE"
echo "╚══════════════════════════════════════════════╝"
echo ""

# Check Azure CLI is logged in
if ! az account show &> /dev/null; then
    echo "❌ Not logged in to Azure. Run: az login"
    exit 1
fi

echo "📋 Subscription: $(az account show --query name -o tsv)"
echo ""

# Prompt for secrets if not set
if [ -z "$SQL_ADMIN_PASSWORD" ]; then
    read -s -p "🔑 Enter SQL Admin Password: " SQL_ADMIN_PASSWORD
    echo ""
fi

if [ -z "$JWT_SECRET" ]; then
    JWT_SECRET="MiniLibrary-Azure-Demo-Secret-Key-$(openssl rand -hex 16)"
    echo "🔑 Generated JWT Secret (save this): $JWT_SECRET"
fi

# Create resource group
echo ""
echo "📦 Creating resource group..."
az group create --name "$RESOURCE_GROUP" --location "$LOCATION" --output none

# Deploy Bicep template
echo "🚀 Deploying infrastructure (this takes 2-5 minutes)..."
DEPLOYMENT_OUTPUT=$(az deployment group create \
    --resource-group "$RESOURCE_GROUP" \
    --template-file "$(dirname "$0")/main.bicep" \
    --parameters \
        baseName="$BASE_NAME" \
        location="$LOCATION" \
        sqlAdminPassword="$SQL_ADMIN_PASSWORD" \
        jwtSecret="$JWT_SECRET" \
        googleClientId="${GOOGLE_CLIENT_ID:-}" \
        googleClientSecret="${GOOGLE_CLIENT_SECRET:-}" \
        openAiApiKey="${OPENAI_API_KEY:-}" \
        emailSenderEmail="${EMAIL_SENDER_EMAIL:-}" \
        emailAppPassword="${EMAIL_APP_PASSWORD:-}" \
        apiImage="$API_IMAGE" \
    --query properties.outputs \
    --output json)

# Extract outputs
API_URL=$(echo "$DEPLOYMENT_OUTPUT" | jq -r '.apiUrl.value')
SQL_FQDN=$(echo "$DEPLOYMENT_OUTPUT" | jq -r '.sqlServerFqdn.value')

echo ""
echo "╔══════════════════════════════════════════════╗"
echo "║  ✅ Deployment Complete!                    ║"
echo "╠══════════════════════════════════════════════╣"
echo "║  API URL:    $API_URL"
echo "║  SQL Server: $SQL_FQDN"
echo "║  Health:     $API_URL/health"
echo "║  Swagger:    $API_URL/swagger"
echo "╚══════════════════════════════════════════════╝"
echo ""
echo "📝 Next steps:"
echo "   1. Add redirect URI in Google Console: ${API_URL}/signin-google"
echo "   2. Deploy frontend: Connect repo to Azure Static Web Apps"
echo "   3. Test: curl ${API_URL}/health"
echo ""
echo "🧹 To delete everything: az group delete --name $RESOURCE_GROUP --yes"
