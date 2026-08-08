---
inclusion: always
---

# Git Commit Workflow (Per Sub-Task)

This steering defines the procedure for committing code after each completed **sub-task**.
It complements `github-workflow.md` (branching/PR strategy) and `task-execution-workflow.md` (end-to-end lifecycle).

## Context

- A **top-level task** (e.g., "7. Implement Loan System") gets ONE feature branch.
- Each **sub-task** (e.g., 7.1, 7.2, 7.3) within that branch gets ONE atomic commit.
- The branch is NEVER `develop` or `main` — always a feature/bugfix/chore branch.

## When to Commit

- After each completed sub-task.
- Each sub-task = one atomic commit (keeps history clean and bisectable).
- Do NOT commit if the build is broken — fix first, then commit everything together.
- Do NOT commit directly to `develop` or `main` — EVER.

## Commit Procedure

1. **Verify build passes** — `dotnet build` must succeed with 0 errors.
2. **Run tests** — `dotnet test` must pass.
3. **Review changes** — `git status` and `git diff --stat` to understand what changed.
4. **Stage selectively** — add only files related to this sub-task. Never use `git add .` blindly.
   - If a file was modified as part of this sub-task, stage it.
   - If a file has unrelated formatting changes or auto-generated diffs (obj/, bin/), leave it out.
5. **Write commit message** — follow Conventional Commits with Jira ID:
   ```
   type(scope): concise description [MINI-XX]
   ```
6. **Push to the feature branch** — `git push -u origin feature/MINI-XX-description`

## Commit Message Rules

### Format
```
type(scope): description [MINI-XX]

Optional body explaining WHY, not WHAT (the diff shows what).
```

### Types
| Type | When to use |
|------|-------------|
| `feat` | New feature or user-facing behavior |
| `fix` | Bug fix |
| `refactor` | Code restructuring without behavior change |
| `test` | Adding or updating tests |
| `docs` | Documentation only changes |
| `chore` | Tooling, config, dependencies |

### Scope
Use the primary feature/module affected: `books`, `auth`, `loans`, `users`, `search`, `infra`, `api`, `web`.

### Jira ID
- Extract from current branch name (e.g., `feature/MINI-42-search-books` → `[MINI-42]`)
- Always include in the commit message
- If no Jira ID is in the branch name, omit the bracket

## Files to Exclude from Commits

Never stage these (they should be in .gitignore but verify):
- `bin/`, `obj/` directories
- `.env` (local secrets)
- `node_modules/`
- IDE-specific files (`.vs/`, `.idea/`, `.kiro/settings/`)

## Multi-File Sub-Tasks

If a sub-task touches many files across layers (e.g., domain + application + infrastructure + API):
- Still make ONE commit per sub-task
- The commit message scope should reflect the primary area
- Use the commit body to note secondary areas if helpful

## Edge Cases

- **No changes after sub-task**: Skip commit silently (sub-task may have been analysis/verification only)
- **Only test files changed**: Use `test(scope):` type
- **Only docs changed**: Use `docs(scope):` type
- **Sub-task partially done and blocked**: Do NOT commit partial work; leave unstaged until resolved

## PROHIBITED Actions

- ❌ `git push origin develop` — NEVER push directly to develop
- ❌ `git push origin main` — NEVER push directly to main
- ❌ `git commit` while on `develop` or `main` branch
- ❌ Using GitHub API to create/update files directly on `develop` or `main`
