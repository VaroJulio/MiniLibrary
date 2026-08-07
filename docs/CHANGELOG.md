# Changelog

All notable changes to this project will be documented in this file.
Format based on [Keep a Changelog](https://keepachangelog.com/).

## [Unreleased]

### Added
- Semantic search endpoint: GET /api/search/semantic with natural language queries [MINI-48]
- OpenAI Embedding Service (text-embedding-3-small, 3-second timeout, graceful fallback) [MINI-48]
- Cosine similarity search against stored BookEmbeddings [MINI-48]
- SemanticSearchQuery/Handler/Validator with relevance >= 0.3, max 20 results, descending order [MINI-48]
- usedFallback boolean in semantic search response when AI is unavailable [MINI-48]
- Silent query truncation at 500 characters [MINI-48]
- Domain event handlers: embedding generation on BookCreatedEvent and BookUpdatedEvent [MINI-48]
- DomainEventDispatcher: EF Core SaveChanges interceptor for MediatR event publishing [MINI-48]
- IBookEmbeddingRepository for embedding persistence [MINI-48]
- FsCheck property tests for semantic search invariants (scores, ordering, max results) [MINI-48]
- SearchController: GET /api/search/books with text query, filters, and pagination [MINI-47]
- Query validation (max 200 chars), pagination validation (page >= 1, pageSize 1-100) [MINI-47]
- Category, status, yearFrom, yearTo filter support on search endpoint [MINI-47]
- FsCheck property tests for search result correctness (filter forwarding, pagination metadata) [MINI-47]
- CheckOutBook command: preconditions (Available + <5 loans), 14-day due date, optimistic concurrency, wishlist auto-remove [MINI-46]
- CheckInBook command: role-based returns (Member own, Librarian/Admin any), dispatches BookReturnedEvent [MINI-46]
- LoansController with checkout, checkin, history, and overdue endpoints [MINI-46]
- GetLoanHistory query with pagination (MemberOnly) [MINI-46]
- GetOverdueLoans query with pagination (LibrarianOrAdmin) [MINI-46]
- FsCheck property tests: loan precondition correctness and DueDate invariant [MINI-46]
- OAuth 2.0 authentication with Google and Microsoft providers [MINI-28]
- JWT token generation (60-min access, 7-day refresh) with token rotation [MINI-28]
- AuthController: login, callback, refresh, logout, me endpoints [MINI-28]
- Role-based authorization policies: AdminOnly, LibrarianOrAdmin, MemberOnly, Authenticated [MINI-29]
- Permission matrix enforced on all controllers via [Authorize] attributes [MINI-29]
- FsCheck property-based tests validating RBAC permission matrix [MINI-28]
- CreateBook command with FluentValidation (title, author, ISBN-13, year, description, category) [MINI-30]
- BookResponse DTO and AutoMapper mapping profile [MINI-30]

### Fixed
- BookLoan.Create test parameter (added missing borrowedAt argument) [MINI-28]
