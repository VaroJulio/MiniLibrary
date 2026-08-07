---
inclusion: always
---

# Task Execution Workflow (End-to-End)

This steering defines the complete lifecycle for executing a spec task: from branch creation through Jira management, commits, PR creation, and cleanup. It integrates `git-commit-workflow.md`, `github-workflow.md`, and `jira-workflow.md` into a single sequential procedure.

## Scope

A "task" here refers to a **top-level numbered task** in tasks.md (e.g., "4. Implement Authentication", "6. Book Catalog Management"). Sub-tasks (4.1, 4.2, etc.) are commits within that task's branch.

## GitHub Repository

- **Owner**: VaroJulio
- **Repo**: MiniLibrary
- **Protected branches**: `main`, `develop` (NO push directo — todo vía PR)
- **Integration branch**: `develop`

## Golden Rule

**NUNCA hacer push directo a `develop` ni a `main`.** Todo cambio, sin excepción (código, docs, steering, configs), entra vía Pull Request desde un feature branch.

## Execution Sequence

### Phase 1: Setup (Before coding)

1. **Identify or create the Jira issue**
   - Search Jira (project MINI) for an existing issue that maps to this task.
   - If none exists, create one: type = Story or Task, link to the parent Epic.
   - Note the issue key (e.g., `MINI-48`).

2. **Transition Jira issue to "In Progress"**
   - Use `jira_transition_issue` to move the issue from To Do → In Progress.

3. **Create a feature branch from `develop`**
   - Ensure local `develop` is up-to-date: `git checkout develop && git pull origin develop`
   - Create branch: `git checkout -b feature/MINI-XX-short-description`
   - Or via GitHub MCP: `mcp_github_create_branch` with `from_branch: "develop"`.
   - Branch naming follows `github-workflow.md` conventions.

### Phase 2: Implementation (Per sub-task)

For each sub-task (4.1, 4.2, 4.3, etc.) within the parent task:

1. **Implement the sub-task** — write code, follow `coding-standards.md`.
2. **Verify build passes** — `dotnet build` (0 errors).
3. **Run tests** — `dotnet test` (all pass).
4. **Commit** — follow `git-commit-workflow.md`:
   - Stage only relevant files (`git add <specific-files>`)
   - Commit: `type(scope): description [MINI-XX]`
5. **Push** — `git push -u origin feature/MINI-XX-short-description`

Repeat for each sub-task. Each sub-task = one commit. All commits go to the SAME feature branch.

### Phase 3: Completion (After all sub-tasks done)

1. **Final verification**
   - Run full build: `dotnet build` → 0 errors
   - Run all tests: `dotnet test` → all pass

2. **Update documentation** (per `documentation-standards.md`)
   - Update `docs/CHANGELOG.md` with the task's changes.
   - If new API endpoints: verify Swagger XML comments are complete.
   - If architecture changed: update diagrams.
   - Commit: `docs(scope): update changelog and API docs [MINI-XX]`

3. **Push final state**
   - `git push origin feature/MINI-XX-short-description`

4. **Create Pull Request** (target: `develop`)
   - Use `mcp_github_create_pull_request`:
     - owner: VaroJulio
     - repo: MiniLibrary
     - base: `develop`
     - head: `feature/MINI-XX-short-description`
     - title: `type(scope): description [MINI-XX]`
     - body: Follow template from `github-workflow.md`. Include `Closes MINI-XX`.

5. **Transition Jira to "In Review"**
   - Use `jira_transition_issue`: In Progress → In Review.
   - Use `jira_add_comment`: summarize implementation + link PR.

6. **STOP — Report to user and wait for confirmation before next task.**

### Phase 4: Post-merge (After PR is approved/merged)

1. **Transition Jira to "Done"**
   - Use `jira_transition_issue`: In Review → Done.

2. **Clean up**
   - `git checkout develop && git pull origin develop`
   - `git branch -d feature/MINI-XX-short-description`

## Task-to-Branch Mapping

| tasks.md Level | Git Branch | Commits |
|----------------|-----------|--------|
| Top-level task (4, 6, 7...) | One feature branch | Multiple (one per sub-task) |
| Sub-task (4.1, 4.2, 4.3...) | Same branch as parent | One commit each |
| Checkpoint (5, 10, 17...) | No branch | No commits (verification only) |

## Execution Rules

- **ONE top-level task at a time.** Finish, PR, then start next.
- **Never parallel branches** for different top-level tasks (avoids conflicts).
- **Never push to develop/main** — use feature branch + PR.
- **Always verify before PR** — build + tests must pass.

## When to Skip

- **Checkpoint tasks** (5, 10, 17, 22, 25): Verify build/tests pass. No branch, no commits.
- **Already-implemented tasks**: If code exists and tests pass, still create branch + PR to formalize.
- **No-code tasks** (analysis only): Skip silently.

## Jira Integration

| Event | Jira Action |
|-------|-------------|
| Start task | Transition → In Progress |
| Create PR | Transition → In Review + comment with PR URL |
| PR merged | Transition → Done |
| Task blocked | Add comment explaining the blocker |

## Example: Task 8 (Search Feature)

```
1. Jira: Find/create MINI-48 → transition to In Progress
2. git checkout develop && git pull
3. git checkout -b feature/MINI-48-search-feature
4. Implement 8.1 → commit: feat(search): implement SearchBooks query [MINI-48]
5. Implement 8.2 → commit: feat(search): implement SearchController [MINI-48]
6. Implement 8.3 → commit: test(search): property tests for search [MINI-48]
7. docs commit: docs(search): update CHANGELOG [MINI-48]
8. git push -u origin feature/MINI-48-search-feature
9. Create PR: "feat(search): implement text search with filters [MINI-48]" → develop
10. Jira: MINI-48 → In Review + comment
11. STOP — report to user
12. After merge: Jira MINI-48 → Done, delete branch
```

## PROHIBITED Actions

- ❌ Push directly to `develop` or `main` (via git or GitHub API)
- ❌ Execute multiple top-level tasks in parallel
- ❌ Create PRs with partial implementations
- ❌ Skip the verification step before creating PR
- ❌ Proceed to next task without user confirmation
