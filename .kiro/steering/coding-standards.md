---
inclusion: always
---

# Coding Standards

## C# / .NET Backend

### Naming Conventions
- PascalCase for classes, methods, properties, and public fields
- camelCase for local variables and parameters
- _camelCase for private fields (with underscore prefix)
- Interfaces prefixed with `I` (e.g., `IBookRepository`)
- Async methods suffixed with `Async` (e.g., `GetBookAsync`)

### Architecture Rules
- Domain layer has NO dependencies on other layers
- Application layer depends only on Domain
- Infrastructure implements interfaces defined in Application/Domain
- API layer handles HTTP concerns only, delegates to Application via MediatR
- Use Result pattern for error handling (no exceptions for business logic flow)

### Code Organization
- One class per file (except small records/enums)
- Group by feature in Application layer (e.g., `Books/Commands/`, `Books/Queries/`)
- Repository pattern for data access
- Specification pattern for complex queries

### Testing
- Unit tests: test business logic in isolation (mock dependencies)
- Integration tests: test API endpoints with real database (TestContainers)
- Naming: `MethodName_Scenario_ExpectedBehavior`
- Arrange-Act-Assert pattern
- Property-based testing for invariants using FsCheck

## TypeScript / React Frontend

### Naming Conventions
- PascalCase for components and types
- camelCase for functions, variables, hooks
- Use `.tsx` for components, `.ts` for utilities
- Prefix custom hooks with `use` (e.g., `useBooks`)

### Component Structure
- Functional components only (no class components)
- Props interface defined above component
- Use React Query for server state
- Use React Context for app-level state (auth, theme)
- Collocate styles, tests, and types with components

### File Organization
```
src/
├── components/      # Shared UI components
├── features/        # Feature-based modules
│   ├── books/
│   │   ├── components/
│   │   ├── hooks/
│   │   ├── api/
│   │   └── types/
│   └── auth/
├── hooks/           # Shared hooks
├── services/        # API client configuration
├── types/           # Global type definitions
└── utils/           # Utility functions
```

## Git Workflow
- Main branch: `main` (protected)
- Development branch: `develop`
- Feature branches from `develop`
- PRs into `develop`, then release into `main`
- Squash merge for feature branches
- Conventional commits: `feat:`, `fix:`, `docs:`, `test:`, `chore:`, `refactor:`
