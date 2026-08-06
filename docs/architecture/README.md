# Architecture Documentation

## C4 Model Diagrams

### System Context
```mermaid
C4Context
    title MiniLibrary System Context
    
    Person(librarian, "Librarian", "Manages books and loans")
    Person(member, "Library Member", "Borrows and returns books")
    
    System(minilib, "MiniLibrary", "Library management system")
    System_Ext(oauth, "OAuth Provider", "Google/Microsoft SSO")
    System_Ext(openai, "OpenAI API", "AI recommendations")
    
    Rel(librarian, minilib, "Manages catalog")
    Rel(member, minilib, "Searches/borrows books")
    Rel(minilib, oauth, "Authenticates via")
    Rel(minilib, openai, "Gets recommendations from")
```

### Container Diagram
```mermaid
C4Container
    title MiniLibrary Container Diagram
    
    Person(user, "User", "Library staff or member")
    
    Container(web, "Web App", "React + TypeScript", "SPA frontend")
    Container(api, "API", "ASP.NET Core 8", "REST API backend")
    ContainerDb(db, "Database", "SQL Server 2022", "Stores books, users, loans")
    System_Ext(oauth, "OAuth Provider", "SSO")
    System_Ext(openai, "OpenAI", "AI features")
    
    Rel(user, web, "Uses", "HTTPS")
    Rel(web, api, "Calls", "HTTP/JSON")
    Rel(api, db, "Reads/Writes", "EF Core")
    Rel(api, oauth, "Authenticates")
    Rel(api, openai, "Requests")
```

### Component Diagram (API)
```mermaid
C4Component
    title API Component Diagram
    
    Container_Boundary(api, "API Application") {
        Component(controllers, "Controllers", "ASP.NET Core", "HTTP endpoints")
        Component(mediatr, "MediatR", "CQRS", "Command/Query handling")
        Component(handlers, "Handlers", "C#", "Business logic")
        Component(repos, "Repositories", "EF Core", "Data access")
        Component(validators, "Validators", "FluentValidation", "Input validation")
    }
    
    ContainerDb(db, "SQL Server", "Database")
    
    Rel(controllers, mediatr, "Sends commands/queries")
    Rel(mediatr, handlers, "Dispatches to")
    Rel(handlers, repos, "Uses")
    Rel(handlers, validators, "Validates with")
    Rel(repos, db, "Queries")
```

## Domain Model
```mermaid
erDiagram
    Book ||--o{ BookLoan : "has"
    Book }|--|| Category : "belongs to"
    User ||--o{ BookLoan : "borrows"
    User }|--|| Role : "has"
    
    Book {
        guid Id PK
        string Title
        string Author
        string ISBN
        int PublishedYear
        string Description
        BookStatus Status
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    Category {
        guid Id PK
        string Name
        string Description
    }
    
    User {
        guid Id PK
        string Email
        string FullName
        string PasswordHash
        guid RoleId FK
        datetime CreatedAt
    }
    
    Role {
        guid Id PK
        string Name
        string[] Permissions
    }
    
    BookLoan {
        guid Id PK
        guid BookId FK
        guid UserId FK
        datetime BorrowedAt
        datetime DueDate
        datetime ReturnedAt
        LoanStatus Status
    }
```

## Technology Stack
| Layer | Technology | Purpose |
|-------|-----------|---------|
| Frontend | React 18 + TypeScript | SPA |
| UI Library | Material UI | Component library |
| State Mgmt | TanStack Query | Server state |
| Backend | ASP.NET Core 8 | REST API |
| CQRS | MediatR | Command/Query separation |
| ORM | EF Core 8 | Data access |
| Database | SQL Server 2022 | Persistence |
| Auth | ASP.NET Identity + OAuth | Authentication |
| AI | OpenAI API | Recommendations |
| Containers | Docker + Compose | Development & Deployment |
| CI/CD | GitHub Actions | Automation |
