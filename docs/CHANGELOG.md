# Changelog

All notable changes to this project will be documented in this file.
Format based on [Keep a Changelog](https://keepachangelog.com/).

## [Unreleased]

### Added
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
