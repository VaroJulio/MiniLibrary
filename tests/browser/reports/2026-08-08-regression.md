# Browser Functional Test Report

- **Date**: 2026-08-08 12:46
- **Type**: regression
- **Environment**: http://localhost:3000 / http://localhost:5000 (Docker)
- **Tool**: Playwright (headless browser + screenshots)
- **Screenshots**: Yes — see `{TODAY}-screenshots/`

## Summary

| Total | Passed | Failed | Skipped |
|-------|--------|--------|---------|
| 10 | 8 | 0 | 2 |

## Results

| # | Test Case | Module | Status | Notes | Screenshot |
|---|-----------|--------|--------|------- | --- |
| 1 | Login Dev Member | Auth | **PASS** |  | [tc01-login-member.png](2026-08-08-screenshots/tc01-login-member.png) |
| 2 | Login Dev Admin | Auth | **PASS** | Admin nav visible: yes | [tc02-login-admin.png](2026-08-08-screenshots/tc02-login-admin.png) |
| 3 | Login Dev Librarian | Auth | **PASS** |  | [tc03-login-librarian.png](2026-08-08-screenshots/tc03-login-librarian.png) |
| 4 | Book catalog loads | Catalog | **PASS** | Books visible on page | [tc04-catalog.png](2026-08-08-screenshots/tc04-catalog.png) |
| 5 | Book detail shows info | Catalog | **PASS** | Detail page content found | [tc05-book-detail.png](2026-08-08-screenshots/tc05-book-detail.png) |
| 6 | Search returns results | Search | **PASS** | Search results visible | [tc06-search-results.png](2026-08-08-screenshots/tc06-search-results.png) |
| 7 | Checkout a book | Loans | **SKIPPED** | No available books | |
| 8 | Return a book | Loans | **SKIPPED** | No active loans | [tc08-return-no-active.png](2026-08-08-screenshots/tc08-return-no-active.png) |
| 9 | All pages load | Navigation | **PASS** | 8/8 pages OK | [tc09-navigation-last-page.png](2026-08-08-screenshots/tc09-navigation-last-page.png) |
| 10 | Dark mode toggle persists | UI | **PASS** | Toggle + reload captured | [tc10-dark-mode-toggled.png](2026-08-08-screenshots/tc10-dark-mode-toggled.png) |
