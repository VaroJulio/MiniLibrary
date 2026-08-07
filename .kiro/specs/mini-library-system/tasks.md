# Implementation Plan: MiniLibrary System

## Overview

This implementation plan builds the MiniLibrary system incrementally from foundation layers (Domain, Infrastructure) up through Application logic and finally the Frontend. The approach follows Clean Architecture principles, ensuring each layer is testable independently before wiring together. The backend uses ASP.NET Core 8 with CQRS/MediatR, the frontend uses React 18 + TypeScript + Material UI, and the database is SQL Server 2022 via EF Core 8.

## Tasks

- [x] 1. Set up solution structure and core infrastructure
  - [x] 1.1 Create .NET solution with Clean Architecture projects
    - Create `MiniLibrary.sln` with projects: `MiniLibrary.Domain`, `MiniLibrary.Application`, `MiniLibrary.Infrastructure`, `MiniLibrary.API`
    - Create test projects: `MiniLibrary.UnitTests`, `MiniLibrary.IntegrationTests`
    - Configure project references following Clean Architecture dependency rules
    - Add NuGet packages: MediatR, FluentValidation, AutoMapper, EF Core 8, FsCheck, xUnit, Moq, FluentAssertions
    - _Requirements: 10.1, 10.3, 11.1_

  - [x] 1.2 Configure EF Core DbContext and entity configurations
    - Create `AppDbContext` with entity configurations for all domain entities
    - Configure soft-delete global query filters for Book and User
    - Add `RowVersion` concurrency token on Book entity
    - Configure database indexes as specified in design (ISBN unique, status, category, etc.)
    - _Requirements: 11.1, 11.2, 11.3, 11.4, 11.5_

  - [x] 1.3 Create initial EF Core migration and Docker Compose setup
    - Generate initial migration from entity configurations
    - Update `docker/docker-compose.yml` to include SQL Server 2022, API, and Frontend services
    - Add health checks and volume mounts for SQL Server persistence
    - Create seed data script for demonstration data
    - _Requirements: 10.3, 10.5, 11.2_

  - [x] 1.4 Configure API middleware pipeline and cross-cutting concerns
    - Implement `CorrelationIdMiddleware` (generate/forward X-Correlation-Id header)
    - Implement `ErrorHandlingMiddleware` (global exception → ProblemDetails RFC 7807)
    - Configure JSON serialization: camelCase, ISO 8601 dates
    - Register MediatR pipeline behaviors: `ValidationBehavior`, `LoggingBehavior`
    - Configure Swagger/OpenAPI with Swashbuckle
    - _Requirements: 12.1, 12.2, 12.5, 14.1, 14.3, 14.5_

- [x] 2. Implement Domain Layer entities and value objects
  - [x] 2.1 Create domain entities: Book, BookLoan, User, Rating, WishlistEntry, Badge, Notification, ReviewVote, BookEmbedding
    - Implement `Book` aggregate root with status management (Available, CheckedOut), soft-delete, and RowVersion
    - Implement `BookLoan` entity with BorrowedAt, DueDate, ReturnedAt lifecycle
    - Implement `User` aggregate root with Role, notification preferences, and soft-delete
    - Implement `Rating`, `WishlistEntry`, `Badge`, `Notification`, `ReviewVote`, `BookEmbedding` entities
    - Define enumerations: `BookStatus`, `UserRole`, `NotificationType`, `BadgeType`
    - _Requirements: 1.6, 2.1, 6.3, 11.3, 16.1, 18.1, 20.1_

  - [x] 2.2 Create value objects: Isbn, DateRange, RelevanceScore
    - Implement `Isbn` value object with ISBN-13 validation (13 digits, checksum)
    - Implement `DateRange` value object for loan periods
    - Implement `RelevanceScore` value object constrained to 0.0–1.0
    - _Requirements: 1.5, 2.1, 4.5_

  - [x] 2.3 Define domain events: BookCreatedEvent, BookUpdatedEvent, BookReturnedEvent, BadgeEarnedEvent, RatingCreatedEvent
    - Create domain event base class and event interfaces
    - Implement `BookCreatedEvent` and `BookUpdatedEvent` for embedding generation triggers
    - Implement `BookReturnedEvent` for badge evaluation and wishlist availability alerts
    - Implement `BadgeEarnedEvent` for notification generation
    - Implement `RatingCreatedEvent` for ranking cache invalidation
    - _Requirements: 4.2, 18.3, 20.2, 20.4_

  - [x] 2.4 Define repository interfaces and service interfaces
    - Define `IBookRepository`, `ILoanRepository`, `IRatingRepository`, `IWishlistRepository`, `IBadgeRepository`, `INotificationRepository`, `IUserRepository`
    - Define `IEmbeddingService`, `IRecommendationService`, `INotificationService`, `ICacheService`
    - Define shared types: `PagedResult<T>`, `PaginationParams`, `SearchCriteria`
    - _Requirements: 13.1, 13.3_

  - [x] 2.5 Write property tests for domain value objects and entities
    - **Property 1: Book Validation Rejects Invalid Data** — Generate random invalid field combinations and verify rejection
    - **Property 9: ISBN Uniqueness** — Generate random ISBN values and verify Isbn value object validates format correctly
    - **Validates: Requirements 1.5, 11.4, 12.1, 12.3**

- [x] 3. Implement Infrastructure Layer - Repository implementations
  - [x] 3.1 Implement BookRepository with EF Core
    - Implement `IBookRepository` with GetById, GetByIsbn, Search (text-based with filters), Add, Update, Delete
    - Implement pagination with offset-based paging (page, pageSize)
    - Implement text search across title, author, ISBN, category with LIKE queries
    - Implement filter support: category, status, year range
    - Implement configurable sorting (ascending/descending by relevant fields)
    - _Requirements: 1.1, 1.2, 1.3, 3.1, 3.2, 3.3, 13.1, 13.2, 13.5_

  - [x] 3.2 Implement LoanRepository with EF Core
    - Implement `ILoanRepository` with active loan count, active loan lookup, user history (paginated), overdue loans
    - Implement DueDate-based queries for expiration alerts
    - _Requirements: 2.1, 2.7, 2.8, 19.4_

  - [x] 3.3 Implement RatingRepository, WishlistRepository, BadgeRepository, NotificationRepository, UserRepository
    - Implement `IRatingRepository` with book ratings (paginated), user rating per book, average calculation
    - Implement `IWishlistRepository` with user wishlist (paginated, max 20), book watchers lookup
    - Implement `IBadgeRepository` with user badges, badge existence check
    - Implement `INotificationRepository` with user notifications (paginated, max 50), mark as read
    - Implement `IUserRepository` with paginated user list, role update, profile queries
    - _Requirements: 16.1, 16.4, 16.6, 18.1, 18.2, 18.8, 19.5, 20.1, 20.3, 7.1, 7.2_

  - [x] 3.4 Implement MemoryCacheService
    - Implement `ICacheService` using `IMemoryCache` with Get, Set, Invalidate operations
    - Support configurable expiration per cache entry
    - _Requirements: 5.6, 17.6, 19.9_

- [x] 4. Implement Authentication and Authorization
  - [x] 4.1 Configure OAuth 2.0 authentication (Google + Microsoft)
- [x] 4. Implement Authentication and Authorization
  - [x] 4.1 Configure OAuth 2.0 authentication (Google + Microsoft)
    - Configure Google OAuth 2.0 authentication scheme
    - Configure Microsoft OAuth 2.0 authentication scheme
    - Implement JWT token generation with 60-minute expiration
    - Implement refresh token generation with 7-day expiration
    - Implement `AuthController` with login, callback, and refresh endpoints
    - _Requirements: 6.1, 6.2, 6.4, 6.5_

  - [x] 4.2 Implement role-based authorization and user provisioning
  - [x] 4.2 Implement role-based authorization and user provisioning
    - Implement automatic user creation with Member role on first SSO login
    - Configure role-based authorization policies: Admin, Librarian, Member
    - Implement `[Authorize]` attribute usage on controllers with role requirements
    - Implement permission matrix per role as defined in Requirements 7.4
    - _Requirements: 6.3, 6.6, 6.7, 7.4, 7.5_

  - [x] 4.3 Write property tests for role-based access control
  - [x] 4.3 Write property tests for role-based access control
    - **Property 8: Role-Based Access Control** — Generate random (role, endpoint) pairs and verify correct 403/200 responses
    - **Validates: Requirements 1.9, 6.6, 7.5, 8.5, 16.2, 17.7, 20.9**
- [x] 6. Implement Book Catalog Management (CRUD) Catalog Management (CRUD)
  - [x] 6.1 Implement CreateBook command with FluentValidation
    - Create `CreateBookCommand`, `CreateBookCommandHandler`, and `CreateBookCommandValidator`
    - Validate: title (1–255 chars), author (1–200 chars), ISBN (ISBN-13 format, unique), year (1450–current), description (max 2000 chars), category (max 100 chars)
    - Set initial status to Available, return created resource with HTTP 201
    - _Requirements: 1.1, 1.5, 1.6_

  - [x] 6.2 Implement UpdateBook and DeleteBook commands
    - Create `UpdateBookCommand`/Handler/Validator with same validation rules as create
    - Create `DeleteBookCommand`/Handler that checks for active loans before deletion
    - Return 404 if book not found, 409 if active loans exist on delete, 403 if Member role
    - _Requirements: 1.2, 1.3, 1.4, 1.7, 1.8, 1.9_

  - [x] 6.3 Implement BooksController with REST endpoints
    - POST `/api/books` — Create book (Librarian, Admin)
    - PUT `/api/books/{id}` — Update book (Librarian, Admin)
    - DELETE `/api/books/{id}` — Delete book (Librarian, Admin)
    - GET `/api/books/{id}` — Get book details (all authenticated)
    - Wire AutoMapper for entity ↔ DTO mapping
    - _Requirements: 1.1, 1.2, 1.3, 1.7, 1.9_

  - [x] 6.4 Write property tests for book validation and deletion invariant
    - **Property 1: Book Validation Rejects Invalid Data** — Generate random invalid CreateBookCommand instances and verify all invalid fields are rejected
    - **Property 2: Book Deletion Invariant** — Generate random books with/without active loans and verify deletion is only allowed when no active loans exist
    - **Validates: Requirements 1.4, 1.5, 12.1, 12.3**

- [x] 7. Implement Loan System (Check-in/Check-out)
  - [x] 7.1 Implement CheckOutBook command
    - Create `CheckOutBookCommand`, Handler, and Validator
    - Verify preconditions: book status is Available AND user has < 5 active loans
    - Create BookLoan with BorrowedAt = now, DueDate = now + 14 days
    - Update book status to CheckedOut
    - Handle optimistic concurrency via RowVersion for concurrent check-out attempts
    - If book is in user's wishlist, auto-remove it
    - _Requirements: 2.1, 2.3, 2.6, 11.5, 18.9_

  - [x] 7.2 Implement CheckInBook command
    - Create `CheckInBookCommand`, Handler, and Validator
    - Verify: Member can only check-in their own loans; Librarian can check-in any loan
    - Set ReturnedAt = now, change book status to Available
    - Dispatch `BookReturnedEvent` for badge evaluation and wishlist alerts
    - _Requirements: 2.2, 2.4, 2.5_

  - [x] 7.3 Implement loan history query and LoansController
    - Create `GetLoanHistoryQuery`/Handler with pagination (20 per page, ordered by date desc)
    - Create `GetOverdueLoansQuery`/Handler for Librarian/Admin (paginated)
    - Implement `LoansController` with endpoints:
      - POST `/api/loans/checkout` — Check-out (Member)
      - POST `/api/loans/checkin` — Check-in (Member, Librarian)
      - GET `/api/loans/history` — User's loan history (Member)
      - GET `/api/loans/overdue` — Overdue loans list (Librarian, Admin)
    - _Requirements: 2.7, 2.8, 19.4_

  - [x] 7.4 Write property tests for loan preconditions and correctness
    - **Property 3: Check-Out Preconditions** — Generate random (bookStatus, activeLoanCount) pairs and verify check-out succeeds iff Available AND count < 5
    - **Property 4: Loan Creation Correctness** — For any successful check-out, verify BorrowedAt = now, DueDate = BorrowedAt + 14 days, status = CheckedOut
    - **Validates: Requirements 2.1, 2.3, 2.6**

- [x] 8. Implement Text Search
  - [x] 8.1 Implement SearchBooks query with filters and pagination
    - Create `SearchBooksQuery`/Handler with text search across title, author, ISBN, category
    - Implement filter parameters: category, status (Available/CheckedOut), year range (min/max between 1000–current year)
    - Implement pagination validation: page >= 1, pageSize 1–100, default 20
    - Return empty list with correct pagination metadata when no matches
    - Return 400 for invalid filter values or pagination parameters
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7, 3.8_

  - [x] 8.2 Implement SearchController
    - GET `/api/search/books` — Text search with filters (all authenticated)
    - Validate query length (1–200 chars when provided), return all books paginated when query is empty
    - Require authentication (401 for unauthenticated)
    - _Requirements: 3.1, 3.2, 3.5_

  - [x] 8.3 Write property tests for search result correctness
    - **Property 5: Search Results Subset Correctness** — Generate random catalogs and queries, verify every result matches query in at least one field AND satisfies all applied filters
    - **Validates: Requirements 3.1, 3.3**

- [x] 9. Implement Semantic Search with AI
  - [x] 9.1 Implement OpenAI Embedding Service
    - Implement `IEmbeddingService` using OpenAI text-embedding-3-small model
    - Generate embeddings from concatenated title + author + description
    - Store embeddings as binary in `BookEmbedding` table
    - Implement 3-second timeout with graceful fallback to text search
    - _Requirements: 4.2, 4.3, 4.8_

  - [x] 9.2 Implement Semantic Search query handler
    - Create `SemanticSearchQuery`/Handler
    - Calculate query embedding, compare via cosine similarity against stored embeddings
    - Filter results with relevance score >= 0.3, order by descending score, max 20 results
    - Truncate queries exceeding 500 characters without notification
    - Reject empty/whitespace-only queries with validation error
    - Include `usedFallback` boolean in response
    - Return relevance score (0.0–1.0) for each result
    - _Requirements: 4.1, 4.3, 4.4, 4.5, 4.6, 4.7_

  - [x] 9.3 Implement embedding generation on book create/update (domain event handler)
    - Handle `BookCreatedEvent` and `BookUpdatedEvent` to generate and store embeddings
    - If embedding generation fails, complete the book operation without embedding and log for retry
    - _Requirements: 4.2, 4.8_

  - [x] 9.4 Write property tests for semantic search invariants
    - **Property 6: Semantic Search Result Invariants** — Generate random result sets and verify all scores >= 0.3 and results are ordered by descending score
    - **Validates: Requirements 4.1, 4.7**

- [x] 10. Checkpoint - Core backend features complete
  - Ensure all tests pass, ask the user if questions arise.

- [x] 11. Implement AI Recommendations
  - [x] 11.1 Implement OpenAI Recommendation Service
    - Implement `IRecommendationService` using GPT-4o-mini
    - Analyze member's loan history to generate personalized recommendations (1–10 books)
    - Each recommendation includes: title, author, category, justification (max 200 chars)
    - Implement 10-second timeout with fallback to popular books by category
    - For members with < 3 loans: return top 10 most-borrowed books in last 90 days
    - _Requirements: 5.1, 5.2, 5.3, 5.4_

  - [x] 11.2 Implement GetRecommendations query with caching
    - Create `GetRecommendationsQuery`/Handler
    - Exclude books already read or currently on loan by the member
    - Cache recommendations per member for 1 hour
    - Invalidate member's cache on new loan or return
    - Implement `RecommendationsController`: GET `/api/recommendations` (Member)
    - _Requirements: 5.5, 5.6, 5.7_

  - [x] 11.3 Write property tests for recommendation exclusion
    - **Property 7: Recommendation Exclusion Invariant** — Generate random member histories and recommendation sets, verify no recommended book appears in member's history or active loans
    - **Validates: Requirements 5.5**

- [x] 12. Implement User Management and Dashboard
  - [x] 12.1 Implement User Management commands and queries
    - Create `GetUsersQuery`/Handler with paginated user list (Admin only)
    - Create `AssignRoleCommand`/Handler with validation (prevent sole Admin from changing own role)
    - Implement `UsersController`:
      - GET `/api/users` — List users (Admin)
      - PUT `/api/users/{id}/role` — Assign role (Admin)
      - GET `/api/users/profile` — Current user profile
    - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5_

  - [x] 12.2 Implement Dashboard statistics queries
    - Create `GetDashboardStatsQuery`/Handler: total books, available, checked-out, active loans, users by role
    - Create `GetLoanMetricsQuery`/Handler: loans by period (7d, 30d, 12m), popular categories, top 10 most-borrowed books
    - Implement `DashboardController`:
      - GET `/api/dashboard/stats` — Overview stats (Librarian, Admin)
      - GET `/api/dashboard/loan-metrics` — Loan metrics (Librarian, Admin)
    - Reject Member access with 403
    - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5_

- [x] 13. Implement Ratings and Reviews
  - [x] 13.1 Implement Rating commands
    - Create `CreateOrUpdateRatingCommand`/Handler/Validator
    - Validate: score 1–5, review text max 1000 chars, member must have completed loan for the book
    - If rating exists for (user, book): update existing; otherwise create new
    - Recalculate book's AverageRating and TotalRatings on create/update/delete
    - Create `DeleteRatingCommand`/Handler (member can delete own rating, recalculates average)
    - _Requirements: 16.1, 16.2, 16.3, 16.4, 16.7, 16.8_

  - [x] 13.2 Implement Rating queries and controller
    - Create `GetBookRatingsQuery`/Handler: paginated reviews (20/page) with author name, score, text, date
    - Include in book detail response: average rating, total ratings, last 5 reviews
    - Create `VoteReviewUsefulCommand`/Handler: 1 vote per member per review, reject self-votes with 403
    - Implement `RatingsController`:
      - POST `/api/books/{id}/ratings` — Create/update rating (Member)
      - DELETE `/api/books/{id}/ratings` — Delete own rating (Member)
      - GET `/api/books/{id}/ratings` — List reviews (all authenticated)
      - POST `/api/ratings/{id}/useful` — Vote useful (Member)
    - _Requirements: 16.1, 16.5, 16.6, 20.6, 20.9_

  - [x] 13.3 Write property tests for rating average correctness
    - **Property 12: Rating Average Correctness** — Generate random sets of N ratings and verify AverageRating = round(sum/N, 1) and TotalRatings = N
    - **Validates: Requirements 16.4, 16.1, 16.8**

- [x] 14. Implement Rankings
  - [x] 14.1 Implement Book Rankings query
    - Create `GetBookRankingsQuery`/Handler
    - Filter: only books with >= 3 ratings; support category, year range, availability filters
    - Support sort by: average rating (default desc), number of ratings, total loans, publication date
    - Return: position, title, author, category, avg rating, total ratings, total loans, status
    - Cache rankings for 15 minutes, invalidate on new rating
    - Implement category ranking: categories with best-rated book and category average
    - _Requirements: 17.1, 17.2, 17.3, 17.4, 17.5, 17.6, 17.7_

  - [x] 14.2 Implement Reader Rankings query
    - Create `GetReaderRankingsQuery`/Handler
    - Order by: returned loans count in period (descending)
    - Support period filters: 30 days, 90 days, 12 months, all-time
    - Return: position, name, books read in period, most-read category, avg rating given
    - Include requesting member's own position in response
    - Cache for 1 hour, invalidate on new return
    - Implement `RankingsController`:
      - GET `/api/rankings/books` — Book rankings (all authenticated)
      - GET `/api/rankings/readers` — Reader rankings (all authenticated)
      - GET `/api/rankings/categories` — Category rankings (all authenticated)
    - _Requirements: 19.5, 19.6, 19.7, 19.8, 19.9_

  - [x] 14.3 Write property tests for ranking invariants
    - **Property 13: Book Ranking Invariants** — Generate random rated books, verify all results have >= 3 ratings and correct ordering
    - **Property 15: Reader Ranking Ordering** — Generate random loan histories, verify readers are ordered by descending return count in period
    - **Validates: Requirements 17.1, 17.3, 19.5**

- [x] 15. Implement Wishlist and Notifications
  - [x] 15.1 Implement Wishlist commands and queries
    - Create `AddToWishlistCommand`/Handler: validate max 20 entries, reject duplicates (409)
    - Create `RemoveFromWishlistCommand`/Handler
    - Create `GetWishlistQuery`/Handler: paginated (20/page), include book status and date added
    - Implement `WishlistController`:
      - POST `/api/wishlist` — Add book (Member)
      - DELETE `/api/wishlist/{bookId}` — Remove book (Member)
      - GET `/api/wishlist` — List wishlist (Member)
    - _Requirements: 18.1, 18.2, 18.6, 18.7, 18.8_

  - [x] 15.2 Implement Notification system
    - Create `GetNotificationsQuery`/Handler: list notifications (max 50, ordered by date desc, read + unread)
    - Create `MarkNotificationReadCommand`/Handler
    - Implement `INotificationService`: in-app + email delivery
    - Implement availability alerts: on BookReturnedEvent, notify all members with book in wishlist
    - Implement `NotificationsController`:
      - GET `/api/notifications` — List notifications (Member)
      - PUT `/api/notifications/{id}/read` — Mark as read (Member)
    - _Requirements: 18.3, 18.4, 18.5, 18.10_

  - [x] 15.3 Implement loan expiration alerts (background job)
    - Implement `LoanExpirationJob` as daily background service
    - Generate notifications for loans expiring in <= 3 days (title, due date, days remaining)
    - Generate daily notifications for overdue loans (title, days overdue)
    - Send both in-app and email notifications
    - Implement notification preferences: allow members to opt in/out of email alerts by type
    - _Requirements: 19.1, 19.2, 19.3, 19.10_

  - [x] 15.4 Write property tests for wishlist size limit
    - **Property 14: Wishlist Size Limit** — Generate random wishlist operations and verify total entries never exceed 20, additions at limit rejected with 409
    - **Validates: Requirements 18.8**

- [x] 16. Implement Gamification System
  - [x] 16.1 Implement Badge evaluation logic
    - Create `EvaluateBadgesCommand`/Handler triggered asynchronously after book return or review creation
    - Implement badge criteria evaluation for all 12 badge types
    - Award badge exactly once per member per badge type
    - Generate notification (in-app + email) when badge is earned
    - Implement `MonthlyBadgeJob` for "Lector del Mes" (first of each month)
    - _Requirements: 20.1, 20.2, 20.4, 20.8_

  - [x] 16.2 Implement Gamification queries and controller
    - Create `GetUserBadgesQuery`/Handler: earned badges with dates + pending badges with progress (% or count)
    - Create `GetGamificationLeaderboardQuery`/Handler: top 10 members by badge count, cached 1 hour
    - Implement viewing another member's public badges
    - Implement `GamificationController`:
      - GET `/api/gamification/badges` — Own badges + progress (Member)
      - GET `/api/gamification/badges/{userId}` — Public badges of another member
      - GET `/api/gamification/leaderboard` — Top 10 badge holders
    - _Requirements: 20.3, 20.5, 20.7_

  - [x] 16.3 Write property tests for badge criteria evaluation
    - **Property 16: Badge Criteria Evaluation** — Generate random member activity histories meeting badge criteria and verify badge is awarded exactly once
    - **Validates: Requirements 20.1, 20.2**

- [x] 17. Checkpoint - All backend features complete
  - Ensure all tests pass, ask the user if questions arise.

- [x] 18. Implement Pagination and Serialization cross-cutting concerns
  - [x] 18.1 Implement standard paginated response wrapper and metadata
    - Create `PagedResponse<T>` wrapper with `data` and `pagination` properties
    - Implement `PaginationMetadata`: totalCount, pageSize, currentPage, totalPages, hasNext, hasPrevious
    - Apply wrapper to all list endpoints consistently
    - Handle out-of-range page requests: return empty list with correct metadata
    - _Requirements: 13.1, 13.2, 13.3, 13.4, 14.2_

  - [x] 18.2 Write property tests for pagination metadata consistency and serialization round-trip
    - **Property 10: Pagination Metadata Consistency** — Generate random (totalCount, pageSize, currentPage) and verify totalPages = ceil(T/S), hasNext/hasPrevious correctness
    - **Property 11: JSON Serialization Round-Trip** — Generate random response objects, serialize to JSON (camelCase, ISO 8601) and deserialize back, verify equivalence
    - **Validates: Requirements 13.3, 13.1, 13.2, 14.4, 14.1, 14.3**

- [x] 19. Set up React Frontend project
  - [x] 19.1 Initialize React + TypeScript + Vite project
    - Create `src/MiniLibrary.Web/` with Vite + React 18 + TypeScript template
    - Install dependencies: Material UI, TanStack Query, React Router, Axios
    - Configure path aliases and tsconfig
    - Set up code splitting with React.lazy and Suspense per route
    - _Requirements: 9.2, 15.5_

  - [x] 19.2 Implement theme, design system, and layout
    - Create Material UI custom theme: primary Indigo #1E3A5F, secondary Amber #F59E0B
    - Configure typography (Inter/Roboto), border-radius (8–12px)
    - Implement light/dark mode toggle persisting to localStorage, respecting prefers-color-scheme
    - Implement responsive layout: drawer navigation (desktop) and hamburger/bottom nav (mobile)
    - Create shared components: `EmptyState`, `PagedList` (skeleton loading), loading spinners
    - _Requirements: 9.1, 9.3, 9.4, 15.3, 15.4, 15.6, 15.8_

  - [x] 19.3 Implement API client and authentication context
    - Create Axios instance with base URL, auth interceptor (JWT bearer token), correlation ID header
    - Implement `AuthContext` with login (OAuth redirect), logout, token refresh logic
    - Implement `OAuthCallback` page handling provider responses
    - Configure TanStack Query client with default stale times and error handling
    - _Requirements: 6.1, 6.2, 6.4, 6.5, 14.5_

- [x] 20. Implement Frontend Feature Modules - Catalog and Loans
  - [x] 20.1 Implement Book Catalog feature module
    - Create `BookList` with paginated table/grid, search bar, and category/status/year filters
    - Create `BookDetail` page: book info, average rating, recent reviews, check-out/wishlist actions
    - Create `BookForm` for create/update (Librarian/Admin) with field validation and error display
    - Implement optimistic updates for book CRUD operations
    - Implement skeleton loaders for book list and detail pages
    - _Requirements: 1.1, 1.2, 9.4, 12.4, 15.2, 15.6, 15.7_

  - [x] 20.2 Implement Loan feature module
    - Create `CheckOutButton` and `CheckInButton` with optimistic updates
    - Create `LoanHistory` page with paginated table (member's own loans)
    - Show loan status indicators (active, returned, overdue)
    - Implement optimistic check-out/check-in with rollback on API error
    - _Requirements: 2.1, 2.2, 2.7, 15.7_

  - [x] 20.3 Implement Search and Semantic Search features
    - Create `SearchBar` component with debounced text input
    - Create `SearchResults` page with filter sidebar (category, status, year range)
    - Create `SemanticSearchPage` with natural language input and relevance scores
    - Show fallback indicator when semantic search uses text fallback
    - _Requirements: 3.1, 3.3, 4.1, 4.3, 4.5_

- [ ] 21. Implement Frontend Feature Modules - Advanced Features
  - [ ] 21.1 Implement Recommendations and Dashboard
    - Create `RecommendationsList` with `RecommendationCard` (title, author, category, justification)
    - Create `DashboardPage` with `StatsCards` and `LoanCharts` (Librarian/Admin only)
    - Implement charts for loan metrics by period, popular categories, top books
    - _Requirements: 5.1, 8.1, 8.2, 8.3, 8.4_

  - [ ] 21.2 Implement Ratings, Rankings, and Wishlist features
    - Create `RatingForm` (star selector + review text), `ReviewList` (paginated), `StarDisplay`
    - Create `BookRankingPage` with filters and sort options
    - Create `ReaderRankingPage` with period filter and current user's position highlight
    - Create `WishlistPage` with availability status badges and remove action
    - Create `WishlistButton` for adding books from catalog/detail views
    - _Requirements: 16.1, 16.5, 17.1, 17.4, 18.1, 18.2, 19.5_

  - [ ] 21.3 Implement Gamification, Notifications, and User Management
    - Create `BadgesPage` with earned badges and progress toward pending badges
    - Create `LeaderboardPage` showing top 10 members by badges
    - Create `NotificationBell` (header icon with unread count) and `NotificationPanel` (dropdown)
    - Create `UserManagement` page (Admin only) with role selector
    - Implement notification preferences settings page
    - _Requirements: 20.3, 20.5, 20.7, 18.4, 18.5, 7.1, 7.2, 19.10_

- [ ] 22. Checkpoint - Frontend feature modules complete
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 23. Implement CI/CD Pipeline and Docker Configuration
  - [ ] 23.1 Configure GitHub Actions CI pipeline
    - Update `.github/workflows/ci.yml`: build, run tests (unit + property + integration), linting
    - Configure TestContainers for SQL Server in CI environment
    - Add frontend build and lint steps (Vite build, ESLint, TypeScript check)
    - Block PR merge on CI failures
    - _Requirements: 10.2, 10.4_

  - [ ] 23.2 Configure GitHub Actions CD pipeline and Docker images
    - Update `.github/workflows/cd.yml`: build and push Docker images to ghcr.io on main branch push
    - Update `docker/Dockerfile.api` for production API build
    - Update `docker/Dockerfile.web` for production frontend build (Nginx)
    - Verify `docker-compose.yml` brings up full environment (API + Frontend + SQL Server) with one command
    - _Requirements: 10.1, 10.2, 10.3_

  - [ ] 23.3 Create seed data script
    - Update `scripts/seed-data.sh` to populate demonstration data: sample books (varied categories), users (Admin, Librarian, Members), sample loans, ratings, and badges
    - Ensure seed script is idempotent (can run multiple times safely)
    - _Requirements: 10.5_

- [ ] 24. Final integration wiring and cross-cutting validation
  - [ ] 24.1 Wire all controllers, DI registration, and integration verification
    - Register all services, repositories, handlers in DI container (`Program.cs`)
    - Configure AutoMapper profiles for all entity ↔ DTO mappings
    - Verify all endpoints are accessible and return correct response formats
    - Verify X-Correlation-Id header is present on all responses
    - Verify ProblemDetails format for all error responses
    - _Requirements: 12.5, 14.1, 14.2, 14.3, 14.5_

  - [ ] 24.2 Write integration tests for end-to-end flows
    - Test full authentication flow (OAuth mock → JWT → authorized request)
    - Test concurrent check-out scenario (verify only one succeeds)
    - Test pagination with real datasets
    - Test soft-delete behavior (deleted records excluded from queries)
    - _Requirements: 6.1, 11.3, 11.5, 13.4_

- [ ] 25. Final checkpoint - System complete
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation at key milestones
- Property tests validate universal correctness properties using FsCheck with minimum 100 iterations
- Unit tests validate specific examples and edge cases
- The backend is built layer-by-layer (Domain → Infrastructure → Application → API) following Clean Architecture
- The frontend is built after the API is functional, consuming real endpoints
- Background jobs (badge evaluation, loan expiration, monthly badges) run asynchronously to avoid impacting request latency

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1"] },
    { "id": 1, "tasks": ["1.2", "2.1", "2.2", "2.3", "2.4"] },
    { "id": 2, "tasks": ["1.3", "1.4", "2.5"] },
    { "id": 3, "tasks": ["3.1", "3.2", "3.3", "3.4"] },
    { "id": 4, "tasks": ["4.1", "4.2"] },
    { "id": 5, "tasks": ["4.3", "6.1"] },
    { "id": 6, "tasks": ["6.2", "6.3", "7.1"] },
    { "id": 7, "tasks": ["6.4", "7.2", "7.3"] },
    { "id": 8, "tasks": ["7.4", "8.1"] },
    { "id": 9, "tasks": ["8.2", "8.3", "9.1"] },
    { "id": 10, "tasks": ["9.2", "9.3"] },
    { "id": 11, "tasks": ["9.4", "11.1", "12.1", "12.2"] },
    { "id": 12, "tasks": ["11.2", "11.3", "13.1"] },
    { "id": 13, "tasks": ["13.2", "13.3", "14.1"] },
    { "id": 14, "tasks": ["14.2", "14.3", "15.1"] },
    { "id": 15, "tasks": ["15.2", "15.3", "15.4"] },
    { "id": 16, "tasks": ["16.1", "16.2"] },
    { "id": 17, "tasks": ["16.3", "18.1"] },
    { "id": 18, "tasks": ["18.2", "19.1"] },
    { "id": 19, "tasks": ["19.2", "19.3"] },
    { "id": 20, "tasks": ["20.1", "20.2", "20.3"] },
    { "id": 21, "tasks": ["21.1", "21.2", "21.3"] },
    { "id": 22, "tasks": ["23.1", "23.2", "23.3"] },
    { "id": 23, "tasks": ["24.1", "24.2"] }
  ]
}
```
