---
inclusion: always
---

# MiniLibrary - Project Overview

## Architecture
- **Backend**: ASP.NET Core 8 Web API (C#)
- **Frontend**: React 18 + TypeScript + Vite
- **Database**: SQL Server 2022 (containerized)
- **Authentication**: ASP.NET Identity + SSO (Google/Microsoft OAuth)
- **Containerization**: Docker + Docker Compose

## Repository Structure
```
MiniLibrary/
├── src/
│   ├── MiniLibrary.API/           # ASP.NET Core Web API
│   ├── MiniLibrary.Domain/        # Domain entities and interfaces
│   ├── MiniLibrary.Application/   # Business logic, CQRS, MediatR
│   ├── MiniLibrary.Infrastructure/ # Data access, EF Core, external services
│   └── MiniLibrary.Web/           # React frontend
├── tests/
│   ├── MiniLibrary.UnitTests/     # xUnit unit tests
│   └── MiniLibrary.IntegrationTests/ # Integration tests with TestContainers
├── docker/
│   ├── docker-compose.yml         # Full local environment
│   ├── docker-compose.override.yml
│   ├── Dockerfile.api             # Backend container
│   └── Dockerfile.web             # Frontend container
├── .github/
│   └── workflows/
│       ├── ci.yml                 # CI pipeline
│       └── cd.yml                 # CD pipeline
├── docs/
│   ├── architecture/             # Architecture diagrams (C4, Mermaid)
│   └── api/                      # API documentation (Swagger/OpenAPI)
└── scripts/
    ├── setup-local.sh            # Local environment setup
    └── seed-data.sh              # Database seeding
```

## Tech Stack Details
- **ORM**: Entity Framework Core 8
- **CQRS**: MediatR
- **Validation**: FluentValidation
- **Mapping**: AutoMapper
- **Testing**: xUnit + Moq + FluentAssertions + TestContainers
- **API Docs**: Swagger/OpenAPI (Swashbuckle)
- **Frontend State**: TanStack Query (React Query)
- **UI Components**: Material UI (MUI)
- **AI Features**: OpenAI API for book recommendations and smart search

## Conventions
- Follow Clean Architecture principles
- Use CQRS pattern with MediatR
- All API endpoints follow RESTful conventions
- Use DTOs for API request/response, never expose domain entities
- All database changes through EF Core migrations
- Branch naming: `feature/`, `bugfix/`, `hotfix/`, `release/`
- Commit messages follow Conventional Commits
- All PRs require passing CI before merge
