# 📚 MiniLibrary - Library Management System

A modern, full-stack library management system built with ASP.NET Core 8 and React 18.

## Features

### Core
- 📖 **Book Management**: Add, edit, and delete books with rich metadata
- 🔄 **Check-in/Check-out**: Track book loans with due dates
- 🔍 **Smart Search**: Find books by title, author, ISBN, or category

### Bonus
- 🔐 **Authentication & SSO**: Google/Microsoft login with role-based access
- 🤖 **AI Features**: Book recommendations and semantic search powered by OpenAI
- 📊 **Dashboard**: Statistics and analytics for librarians
- 📱 **Responsive UI**: Works on desktop and mobile

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core 8, C# |
| Frontend | React 18, TypeScript, Vite |
| Database | SQL Server 2022 |
| UI | Material UI (MUI) |
| Auth | ASP.NET Identity + OAuth 2.0 |
| AI | OpenAI API |
| DevOps | Docker, GitHub Actions |

## Quick Start

### Prerequisites
- Docker Desktop
- .NET 8 SDK
- Node.js 20+

### Run with Docker (recommended)
```bash
# Clone the repository
git clone https://github.com/VaroJulio/MiniLibrary.git
cd MiniLibrary

# Copy environment template
cp .env.example .env
# Edit .env with your secrets

# Start all services
docker compose -f docker/docker-compose.yml up -d
```

Access the app:
- **Frontend**: http://localhost:3000
- **API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger

### Run locally (development)
```bash
# Setup environment
chmod +x scripts/setup-local.sh
./scripts/setup-local.sh

# Start backend
cd src/MiniLibrary.API
dotnet run

# Start frontend (new terminal)
cd src/MiniLibrary.Web
npm run dev
```

## Testing
```bash
# All tests
dotnet test MiniLibrary.sln

# Unit tests only
dotnet test tests/MiniLibrary.UnitTests

# Integration tests (requires Docker)
dotnet test tests/MiniLibrary.IntegrationTests

# Frontend tests
cd src/MiniLibrary.Web && npm test
```

## Project Structure
```
MiniLibrary/
├── src/
│   ├── MiniLibrary.API/            # Web API (controllers, middleware)
│   ├── MiniLibrary.Domain/         # Entities, value objects, interfaces
│   ├── MiniLibrary.Application/    # Use cases, CQRS handlers
│   ├── MiniLibrary.Infrastructure/ # EF Core, external services
│   └── MiniLibrary.Web/            # React SPA
├── tests/
│   ├── MiniLibrary.UnitTests/
│   └── MiniLibrary.IntegrationTests/
├── docker/                         # Docker configuration
├── docs/                           # Architecture & API docs
└── scripts/                        # Utility scripts
```

## Architecture
Clean Architecture with CQRS pattern. See [docs/architecture/](docs/architecture/) for detailed diagrams.

## License
MIT
