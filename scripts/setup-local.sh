#!/bin/bash
set -e

echo "🚀 Setting up MiniLibrary local development environment..."

# Check prerequisites
command -v docker >/dev/null 2>&1 || { echo "❌ Docker is required but not installed."; exit 1; }
command -v dotnet >/dev/null 2>&1 || { echo "❌ .NET SDK is required but not installed."; exit 1; }
command -v node >/dev/null 2>&1 || { echo "❌ Node.js is required but not installed."; exit 1; }

# Create .env if not exists
if [ ! -f .env ]; then
    cp .env.example .env
    echo "📝 Created .env file from template. Please update with your secrets."
fi

# Load env
export $(grep -v '^#' .env | xargs)

# Start database
echo "🗄️ Starting SQL Server..."
docker compose -f docker/docker-compose.yml up -d sqlserver

# Wait for DB
echo "⏳ Waiting for SQL Server to be ready..."
sleep 15

# Restore and build backend
echo "🔨 Building backend..."
dotnet restore MiniLibrary.sln
dotnet build MiniLibrary.sln

# Run migrations
echo "📊 Running database migrations..."
cd src/MiniLibrary.Infrastructure
dotnet ef database update --startup-project ../MiniLibrary.API
cd ../..

# Setup frontend
echo "⚛️ Setting up frontend..."
cd src/MiniLibrary.Web
npm install
cd ../..

echo "✅ Setup complete!"
echo ""
echo "To start developing:"
echo "  Backend:  cd src/MiniLibrary.API && dotnet run"
echo "  Frontend: cd src/MiniLibrary.Web && npm run dev"
echo "  Full stack (Docker): docker compose -f docker/docker-compose.yml up -d"
