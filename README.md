# MiniLibrary - Library Management System

A modern, full-stack library management system built with ASP.NET Core 8 and React 18. Features AI-powered semantic search, personalized recommendations, gamification, and real-time notifications.

## Features

### Core
- Book Management: full CRUD with validation (ISBN-13, category, year range)
- Check-in/Check-out: track book loans with 14-day due dates, 5-loan limit per member
- Text Search: search by title, author, ISBN, category with filters and pagination
- Loan History: paginated history with status indicators (active, overdue, returned)

### AI-Powered
- Semantic Search: natural language queries using OpenAI embeddings (cosine similarity)
- Recommendations: personalized suggestions based on reading history (GPT-4o-mini)
- Fallback: graceful degradation to text search when AI is unavailable

### Social & Gamification
- Ratings & Reviews: 1-5 star ratings with review text, "useful" votes
- Book Rankings: sorted by rating, loans, or category (min 3 ratings)
- Reader Rankings: leaderboard by books read per period
- Badges: 12 achievement types awarded automatically on milestones
- Wishlist: up to 20 books, availability notifications on return

### Admin & Operations
- Role-Based Access: Admin, Librarian, Member with permission matrix
- Dashboard: statistics, loan metrics, popular categories, top books
- User Management: role assignment (Admin only)
- Notifications: in-app alerts for expiring loans, overdue, wishlist availability

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core 8, C#, CQRS/MediatR, FluentValidation, AutoMapper |
| Frontend | React 18, TypeScript, Vite 6, Material UI 6, TanStack Query 5 |
| Database | SQL Server 2022, EF Core 8 |
| Auth | JWT + OAuth 2.0 (Google, Microsoft) |
| AI | OpenAI (text-embedding-3-small, GPT-4o-mini) |
| Testing | xUnit, FsCheck (property-based), FluentAssertions, WebApplicationFactory |
| DevOps | Docker, Docker Compose, GitHub Actions CI/CD |

## Quick Start

### Prerequisites

- Docker Desktop (for SQL Server)
- .NET 8 SDK
- Node.js 20+
- (Optional) OpenAI API key for AI features
- (Optional) Google/Microsoft OAuth credentials for SSO

### Option 1: Run Locally (Development)

**1. Start SQL Server:**

```bash
docker run -d --name minilibrary-sql \
  -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest
```

**2. Wait ~20 seconds for SQL Server to start, then apply migrations:**

```bash
dotnet tool install --global dotnet-ef  # if not already installed
dotnet ef database update \
  --project src/MiniLibrary.Infrastructure \
  --startup-project src/MiniLibrary.API
```

**3. (Optional) Seed demonstration data:**

```bash
./scripts/seed-data.sh
```

This adds 10 sample books, 5 users (Admin, Librarian, 3 Members), loans, ratings, and badges.

**4. Run the API:**

```bash
dotnet run --project src/MiniLibrary.API
```

The API will be available at:
- API: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger
- Health check: http://localhost:5000/health

**5. Run the Frontend:**

```bash
cd src/MiniLibrary.Web
npm install  # first time only
npm run dev
```

Frontend at http://localhost:3000 (proxies API requests to localhost:5000)

### Option 2: Docker Compose (Full Stack)

```bash
cd docker
docker compose up -d --build
```

Services:
- Frontend: http://localhost:3000 (Nginx + React)
- API: http://localhost:5000
- SQL Server: localhost:1433

To stop: `docker compose down` (add `-v` to also remove database volume)

## Configuration

Copy `.env.example` to `.env` and configure:

```bash
cp .env.example .env
```

Key settings:

| Variable | Description | Required |
|----------|-------------|----------|
| `SA_PASSWORD` | SQL Server password (must meet complexity requirements) | Yes |
| `JWT_SECRET` | Secret for JWT token signing (min 32 chars) | Yes |
| `GOOGLE_CLIENT_ID` / `GOOGLE_CLIENT_SECRET` | Google OAuth credentials | For SSO |
| `MICROSOFT_CLIENT_ID` / `MICROSOFT_CLIENT_SECRET` | Microsoft OAuth credentials | For SSO |
| `OPENAI_API_KEY` | OpenAI API key | For AI features |

Without OAuth configured, you can test the API using manually crafted JWT tokens (see Testing section below).

## Testing

### Automated Tests

```bash
# Run all tests (335 total: 319 unit + 16 integration)
dotnet test

# Unit tests only (no external dependencies needed)
dotnet test tests/MiniLibrary.UnitTests

# Integration tests (uses in-memory database, no Docker required)
dotnet test tests/MiniLibrary.IntegrationTests

# Frontend TypeScript check
cd src/MiniLibrary.Web && npx tsc --noEmit

# Frontend lint
cd src/MiniLibrary.Web && npm run lint

# Frontend production build
cd src/MiniLibrary.Web && npm run build
```

### Testing the API with curl

**Health check (no auth needed):**

```bash
curl http://localhost:5000/health
# Returns: {"status":"healthy","timestamp":"2024-..."}
```

**All API endpoints require a JWT Bearer token.** To get one:

1. If OAuth is configured: navigate to http://localhost:5000/api/auth/login/google
2. For development without OAuth: generate a test token using the JWT secret from `appsettings.json`

Example with token:

```bash
TOKEN="your-jwt-token-here"

# Search books
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5000/api/search/books?page=1&pageSize=10"

# Create a book (Librarian/Admin only)
curl -X POST http://localhost:5000/api/books \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Clean Code",
    "author": "Robert C. Martin",
    "isbn": "9780132350884",
    "publishedYear": 2008,
    "description": "A handbook of agile software craftsmanship",
    "category": "Technology"
  }'

# Check out a book (Member)
curl -X POST http://localhost:5000/api/loans/checkout \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"bookId": "book-guid-here"}'

# Get loan history
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5000/api/loans/history?page=1&pageSize=20"

# Get recommendations
curl -H "Authorization: Bearer $TOKEN" \
  http://localhost:5000/api/recommendations

# Dashboard stats (Librarian/Admin)
curl -H "Authorization: Bearer $TOKEN" \
  http://localhost:5000/api/dashboard/stats
```

### API Endpoints Summary

| Endpoint | Method | Role | Description |
|----------|--------|------|-------------|
| `/health` | GET | Public | Health check |
| `/api/auth/login/{provider}` | GET | Public | OAuth login |
| `/api/auth/refresh` | POST | Public | Refresh JWT |
| `/api/books` | POST | Librarian+ | Create book |
| `/api/books/{id}` | GET/PUT/DELETE | Auth/Lib+ | Book CRUD |
| `/api/search/books` | GET | Auth | Text search |
| `/api/search/semantic` | GET | Auth | AI search |
| `/api/loans/checkout` | POST | Member+ | Check out |
| `/api/loans/checkin` | POST | Member+ | Check in |
| `/api/loans/history` | GET | Member+ | Loan history |
| `/api/loans/overdue` | GET | Librarian+ | Overdue list |
| `/api/recommendations` | GET | Member+ | AI recommendations |
| `/api/ratings` | POST/DELETE | Member+ | Rate books |
| `/api/rankings/books` | GET | Auth | Book rankings |
| `/api/rankings/readers` | GET | Auth | Reader rankings |
| `/api/wishlist` | GET/POST/DELETE | Member+ | Wishlist |
| `/api/gamification/badges` | GET | Member+ | Badges |
| `/api/gamification/leaderboard` | GET | Auth | Top 10 |
| `/api/notifications` | GET | Member+ | Notifications |
| `/api/users` | GET | Admin | User list |
| `/api/users/{id}/role` | PUT | Admin | Assign role |
| `/api/dashboard/stats` | GET | Librarian+ | Stats |
| `/api/dashboard/loan-metrics` | GET | Librarian+ | Metrics |

### Swagger UI

When running in Development mode, interactive API documentation is available at:
http://localhost:5000/swagger

Click "Authorize" and paste your JWT token to test endpoints directly.

## Project Structure

```
MiniLibrary/
├── src/
│   ├── MiniLibrary.API/            # Web API (controllers, middleware, auth)
│   ├── MiniLibrary.Domain/         # Entities, value objects, domain events
│   ├── MiniLibrary.Application/    # CQRS handlers, validators, DTOs
│   ├── MiniLibrary.Infrastructure/ # EF Core, repositories, OpenAI, jobs
│   └── MiniLibrary.Web/            # React SPA (Vite + MUI)
├── tests/
│   ├── MiniLibrary.UnitTests/      # 319 unit + property-based tests
│   └── MiniLibrary.IntegrationTests/ # 16 WebApplicationFactory tests
├── docker/                         # Dockerfiles + docker-compose
├── docs/                           # Architecture, API docs, changelog
└── scripts/                        # Setup and seed scripts
```

## Architecture

Clean Architecture with CQRS/MediatR pattern:

- **Domain**: entities, value objects (Isbn, DateRange, RelevanceScore), domain events
- **Application**: commands/queries via MediatR, FluentValidation, AutoMapper
- **Infrastructure**: EF Core repositories, OpenAI services, background jobs, caching
- **API**: controllers, middleware (correlation ID, error handling), JWT auth

See [docs/architecture/](docs/architecture/) for detailed diagrams and ADRs.

## CI/CD

- **CI** (on PR to develop/main): Backend build + 335 tests, Frontend TypeScript + ESLint + Vite build
- **CD** (on push to main): Docker images built and pushed to `ghcr.io/varojulio/minilibrary-api` and `ghcr.io/varojulio/minilibrary-web`

## License

MIT
