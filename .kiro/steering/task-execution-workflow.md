---
inclusion: always
---

# Task Execution Workflow (End-to-End)

This steering defines the complete lifecycle for executing a spec task: from branch creation through Jira management, commits, PR creation, and cleanup. It integrates `git-commit-workflow.md`, `github-workflow.md`, and `jira-workflow.md` into a single sequential procedure.

## Scope

A "task" here refers to a **top-level numbered task** in tasks.md (e.g., "4. Implement Authentication and Authorization", "6. Implement Book Catalog Management"). Sub-tasks (4.1, 4.2, etc.) are commits within that task's branch.

## GitHub Repository

- **Owner**: VaroJulio
- **Repo**: MiniLibrary
- **Default branch**: main
- **Integration branch**: develop

## Execution Sequence

### Phase 1: Setup (Before coding)

1. **Identify or create the Jira issue**
   - Search Jira for an existing issue that maps to this task (same Epic or Story).
   - If none exists, create one: type = Story or Task, link to the parent Epic.
   - Note the issue key (e.g., `MINI-42`).

2. **Transition Jira issue to "In Progress"**
   - Use `jira_transition_issue` to move the issue from To Do → In Progress.

3. **Create a feature branch from `develop`**
   - Branch naming: `feature/MINI-XX-short-description` (or `bugfix/`, `chore/` per issue type).
   - Use GitHub MCP: `mcp_github_create_branch` with `from_branch: "develop"`.
   - Locally: `git checkout develop && git pull origin develop && git checkout -b feature/MINI-XX-short-description`
   - Example: `feature/MINI-42-auth-and-authorization`

### Phase 2: Implementation (Per sub-task)

For each sub-task (4.1, 4.2, 4.3, etc.) within the parent task:

1. **Implement the sub-task** — write code, run tests.
2. **Verify build passes** — `dotnet build` (backend) or `npm run build` (frontend).
3. **Run tests** — `dotnet test` or equivalent.
4. **Commit** — follow `git-commit-workflow.md`:
   - Stage only relevant files (`git add <specific-files>`)
   - Commit message: `type(scope): description [MINI-XX]`
   - Example: `feat(auth): configure OAuth 2.0 with Google and Microsoft [MINI-42]`
5. **Push** — `git push -u origin feature/MINI-XX-short-description`

Repeat for each sub-task. Each sub-task = one commit. All commits go to the same branch.

### Phase 3: Completion (After all sub-tasks done)

1. **Final verification**
   - Run full build: `dotnet build`
   - Run all tests: `dotnet test`
   - Ensure zero errors.

2. **Update documentation** (per `documentation-standards.md`)
   - Update `docs/CHANGELOG.md` with the task's changes.
   - If new API endpoints: verify Swagger XML comments are complete.
   - If architecture changed: update diagrams in `docs/architecture/`.
   - Commit docs: `docs(scope): update changelog and API docs [MINI-XX]`

3. **Push final state**
   - `git push origin feature/MINI-XX-short-description`

4. **Create Pull Request**
   - Use `mcp_github_create_pull_request`:
     - owner: VaroJulio
     - repo: MiniLibrary
     - base: `develop`
     - head: `feature/MINI-XX-short-description`
     - title: `type(scope): description [MINI-XX]`
     - body: Follow the PR template from `github-workflow.md`, include `Closes MINI-XX`.

5. **Transition Jira issue to "In Review"**
   - Use `jira_transition_issue` to move In Progress → In Review.
   - Use `jira_add_comment` summarizing what was implemented and linking the PR.

### Phase 4: Post-merge (After PR is approved and merged)

1. **Transition Jira issue to "Done"**
   - Use `jira_transition_issue` to move In Review → Done.

2. **Clean up local branch**
   - `git checkout develop && git pull origin develop`
   - `git branch -d feature/MINI-XX-short-description`

## Task-to-Branch Mapping Rules

| tasks.md Level | Git Branch | Commits |
|----------------|-----------|---------|
| Top-level task (4, 6, 7...) | One branch per task | Multiple (one per sub-task) |
| Sub-task (4.1, 4.2, 4.3...) | Same branch as parent | One commit each |
| Checkpoint tasks (5, 10, 17...) | No branch needed | No code changes |

## Execution Mode

When executing tasks from the orchestrator:
- **Execute ONE top-level task at a time** (all its sub-tasks sequentially).
- After completing all sub-tasks → perform Phase 3 (verification, docs, PR, Jira).
- **STOP** after creating the PR. Report to the user before starting the next task.
- Wait for user confirmation before proceeding to the next top-level task.

## Parallel Execution Rules

- **Do NOT execute multiple top-level tasks in parallel** on the same branch.
- Each top-level task gets its own branch.
- Only work on one top-level task at a time to avoid merge conflicts.
- Checkpoints (5, 10, 17, 22, 25) are verification gates — no new branches needed.

## When to Skip

- **Checkpoint tasks** (5, 10, 17, 22, 25): No branch, no commits. Just verify build + tests pass.
- **Tasks with no code changes** (analysis, documentation-only): Only create branch if docs are substantial.
- **Already-implemented tasks**: If code already exists and tests pass, still commit if not yet pushed, create PR if not created.

## Jira Integration Summary

| Event | Jira Action |
|-------|-------------|
| Start task | Transition → In Progress |
| Create PR | Transition → In Review + add comment with PR link |
| PR merged | Transition → Done |
| Task blocked | Add comment explaining the blocker |

## Example: Task 8 (Search Feature)

```
1. Jira: MINI-48 → In Progress
2. git checkout -b feature/MINI-48-search-feature (from develop)
3. Implement 8.1 → commit: feat(search): implement SearchBooks query [MINI-48]
4. Implement 8.2 → commit: feat(search): implement SearchController [MINI-48]
5. Implement 8.3 → commit: test(search): add property tests for search correctness [MINI-48]
6. docs commit: docs(search): update CHANGELOG [MINI-48]
7. Push, create PR: "feat(search): implement text search with filters [MINI-48]"
8. Jira: MINI-48 → In Review + comment with PR URL
9. After merge: Jira MINI-48 → Done
```
