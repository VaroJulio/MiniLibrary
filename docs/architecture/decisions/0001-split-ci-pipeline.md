# 0001 - Split CI Pipeline into Independent Backend and Frontend Jobs

## Status
Accepted

## Date
2026-08-06

## Context

The original CI pipeline (`ci.yml`) was a single monolithic job that executed both backend (.NET) and frontend (React/Node) steps sequentially. This caused the entire pipeline to fail when the frontend project (`src/MiniLibrary.Web/`) did not yet exist, blocking backend PRs from passing CI checks.

The MiniLibrary implementation follows an incremental approach where the backend is built first (Tasks 1-17) and the frontend is added later (Tasks 19-22). During the backend-only phase, the CI pipeline was failing at the `setup-node` cache step because `src/MiniLibrary.Web/package-lock.json` did not exist.

## Decision

Split the CI pipeline into two independent jobs:

1. **`backend`** — Runs .NET restore, build, unit tests, and integration tests. Always executes.
2. **`frontend`** — Runs npm ci, build, and tests. Only executes when `src/MiniLibrary.Web/package-lock.json` exists (using `hashFiles()` condition).

Additionally:
- Integration tests use `continue-on-error: true` since the database schema may not be applied yet during early development.
- Each job uploads its test results as separate artifacts for easier debugging.

## Alternatives Considered

1. **Placeholder `package.json`**: Create an empty frontend project just to satisfy CI. Rejected because it adds noise to the repo and may cause confusion about implementation status.
2. **Comment out frontend steps**: Manually toggle CI steps. Rejected because it requires remembering to re-enable later and pollutes git history.
3. **Single job with `if` conditions per step**: Keeps one job but conditionally skips frontend steps. Rejected because it's harder to read and doesn't show separate pass/fail status per concern.

## Consequences

### Positive
- Backend PRs pass CI immediately without waiting for frontend to exist.
- Clear visibility: each job shows its own green/red status in the PR checks.
- Frontend CI activates automatically once `package-lock.json` is committed (no manual intervention needed).
- Faster feedback: backend and frontend jobs run in parallel once both exist.

### Negative
- Integration tests have `continue-on-error` during early development, which means a failing integration test won't block merge. This should be tightened once the database migration pipeline is stable.

### Follow-up
- Remove `continue-on-error` from integration tests once Task 1.3 (migrations + Docker) is verified in CI.
- When frontend is created (Task 19), verify the `hashFiles` condition activates the frontend job correctly.
