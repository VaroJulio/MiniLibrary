// MiniLibrary Azure Infrastructure - Demo Environment
// Architecture: Container Apps (API) + Static Web Apps (Frontend) + Azure SQL Free
// Cost: ~$0/month within free tier allowances

targetScope = 'resourceGroup'

@description('Base name for all resources')
param baseName string = 'minilibrary'

@description('Azure region for resources')
param location string = resourceGroup().location

@description('SQL Server admin password')
@secure()
param sqlAdminPassword string

@description('JWT secret for token signing')
@secure()
param jwtSecret string

@description('Google OAuth Client ID (optional)')
param googleClientId string = ''

@description('Google OAuth Client Secret (optional)')
@secure()
param googleClientSecret string = ''

@description('OpenAI API Key (optional)')
@secure()
param openAiApiKey string = ''

@description('Container image for API')
param apiImage string = 'ghcr.io/varojulio/minilibrary-api:latest'

// ============================================================================
// Azure SQL Database (Free tier: 32GB, 100K vCore-seconds/month)
// ============================================================================

resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: '${baseName}-sql'
  location: location
  properties: {
    administratorLogin: 'sqladmin'
    administratorLoginPassword: sqlAdminPassword
    version: '12.0'
    publicNetworkAccess: 'Enabled'
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: sqlServer
  name: '${baseName}db'
  location: location
  sku: {
    name: 'GP_S_Gen5'
    tier: 'GeneralPurpose'
    family: 'Gen5'
    capacity: 1
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 34359738368 // 32GB
    autoPauseDelay: 60 // Auto-pause after 60 min inactivity (saves cost)
    minCapacity: json('0.5')
    useFreeLimit: true // Free tier!
    freeLimitExhaustionBehavior: 'AutoPause'
  }
}

// Allow Azure services to access SQL
resource sqlFirewallAzure 'Microsoft.Sql/servers/firewallRules@2023-05-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// ============================================================================
// Container Apps Environment + API Container App
// ============================================================================

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: '${baseName}-logs'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource containerAppEnv 'Microsoft.App/managedEnvironments@2023-11-02-preview' = {
  name: '${baseName}-env'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

resource apiContainerApp 'Microsoft.App/containerApps@2023-11-02-preview' = {
  name: '${baseName}-api'
  location: location
  properties: {
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      ingress: {
        external: true
        targetPort: 5000
        transport: 'http'
        corsPolicy: {
          allowedOrigins: ['*']
          allowedMethods: ['*']
          allowedHeaders: ['*']
        }
      }
      registries: []
    }
    template: {
      containers: [
        {
          name: 'api'
          image: apiImage
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
            { name: 'ASPNETCORE_URLS', value: 'http://+:5000' }
            { name: 'ConnectionStrings__DefaultConnection', value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Database=${sqlDatabase.name};User ID=sqladmin;Password=${sqlAdminPassword};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;' }
            { name: 'Jwt__Secret', value: jwtSecret }
            { name: 'Jwt__Issuer', value: 'MiniLibrary' }
            { name: 'Jwt__Audience', value: 'MiniLibrary' }
            { name: 'Jwt__ExpirationMinutes', value: '60' }
            { name: 'Authentication__EnableDevTokens', value: 'true' }
            { name: 'Authentication__Google__ClientId', value: googleClientId }
            { name: 'Authentication__Google__ClientSecret', value: googleClientSecret }
            { name: 'OpenAI__ApiKey', value: openAiApiKey }
            { name: 'App__PublicUrl', value: 'https://${baseName}-api.${containerAppEnv.properties.defaultDomain}' }
            { name: 'App__FrontendUrl', value: 'https://${baseName}-web.azurestaticapps.net' }
          ]
        }
      ]
      scale: {
        minReplicas: 0 // Scale to zero when idle (free!)
        maxReplicas: 1
        rules: [
          {
            name: 'http-rule'
            http: {
              metadata: {
                concurrentRequests: '10'
              }
            }
          }
        ]
      }
    }
  }
}

// ============================================================================
// Outputs
// ============================================================================

output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output sqlDatabaseName string = sqlDatabase.name
output apiUrl string = 'https://${apiContainerApp.properties.configuration.ingress.fqdn}'
output containerAppEnvDomain string = containerAppEnv.properties.defaultDomain
