---
inclusion: always
---

# Git Commit Workflow (Per-Task)

This steering defines the procedure for committing code after each completed task.
It complements `coding-standards.md` (general conventions) and `github-workflow.md` (branching/PR strategy).

## When to Commit

- After each completed spec task (triggered by the `commit-after-task` hook)
- Each task = one atomic commit (keeps history clean and bisectable)
- Do NOT commit if the build is broken — fix first, then commit everything together

## Commit Procedure

1. **Verify build passes** — check the most recent build output. If it fails, fix before committing.
2. **Review changes** — run `git status` and `git diff --stat` to understand what changed.
3. **Stage selectively** — add only files related to the current task. Never use `git add .` blindly.
   - If a file was modified as part of this task, stage it.
   - If a file has unrelated formatting changes or auto-generated diffs (obj/, bin/), leave it out.
4. **Write commit message** — follow Conventional Commits with Jira ID:
   ```
   type(scope): concise description [MINI-XX]
   ```
5. **Push to remote** — push the current branch with tracking: `git push -u origin <branch-name>`

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
Use the primary feature/module affected: `books`, `auth`, `loans`, `users`, `infra`, `api`, `web`.

### Jira ID
- Extract from current branch name (e.g., `feature/MINI-42-search-books` → `[MINI-42]`)
- Always include in the commit message
- If no Jira ID is in the branch name, omit the bracket

## Files to Exclude from Commits

Never stage these (they should be in .gitignore but verify):
- `bin/`, `obj/` directories
- `.env` (local secrets)
- `node_modules/`
- IDE-specific files (`.vs/`, `.idea/`)

## Multi-File Tasks

If a task touches many files across layers (e.g., domain + application + infrastructure + API):
- Still make ONE commit per task
- The commit message scope should reflect the primary area
- Use the commit body to note secondary areas if helpful

## Edge Cases

- **No changes after task**: Skip commit silently (task may have been analysis/documentation only)
- **Only test files changed**: Use `test(scope):` type
- **Only docs changed**: Use `docs(scope):` type
- **Task partially done and blocked**: Do NOT commit partial work; leave unstaged until resolved
