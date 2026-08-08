---
inclusion: always
---

# Jira Workflow - Kanban

## Project
- **Tool**: Jira Cloud (https://ajjbdeveloper.atlassian.net)
- **Methodology**: Kanban (no sprints)
- **Board**: MiniLibrary Kanban Board

## Kanban States

| State | Description |
|-------|-------------|
| Backlog | Story/task identified, pending prioritization |
| To Do | Prioritized and ready to be worked on |
| In Progress | Currently in development |
| In Review | PR created, in code review |
| Done | Completed, PR merged and deployed |

## Issue Types and Hierarchy

| Type | Use | Branch prefix |
|------|-----|---------------|
| Epic | Large feature (groups related Stories/Tasks) | N/A |
| Story | User-facing feature with direct user value | `feature/` |
| Task | Technical work without direct user value | `chore/` |
| Bug | Defect found in existing functionality | `bugfix/` |
| Sub-task | Breakdown of a Story/Task | Uses parent's prefix |

### Hierarchy rules
- **Epic** → groups multiple Stories and/or Tasks under a feature area
- **Story** → delivers user value, linked to its parent Epic
- **Task** → technical enabler (infra, config, refactor), linked to its parent Epic
- Do NOT create intermediate Stories to group Tasks — Tasks link directly to Epics
- Do NOT convert Tasks to Stories if they don't deliver user value

## Issue Conventions

### Naming
- **Epic**: Feature area name (e.g., "Book Catalog Management (CRUD)")
- **Story**: User story format (e.g., "As a Member, I want to search books by title")
- **Task**: Clear technical action (e.g., "Configure EF Core migrations for Books")

### Required fields
- **Summary**: Concise title
- **Description**: Context, acceptance criteria, technical notes
- **Labels**: `backend`, `frontend`, `infrastructure`, `documentation`
- **Priority**: Highest, High, Medium, Low, Lowest

### Code ↔ Jira linking
- Include issue ID in branch name: `feature/MINI-42-search-books`
- Include issue ID in commits: `feat(books): add search endpoint [MINI-42]`
- Reference in PR description: `Closes MINI-42`

## Workflow

1. Find/take issue from Backlog → move to **To Do**
2. When starting development → move to **In Progress**, create branch
3. When PR is created → move to **In Review**
4. When PR is merged → move to **Done**

## Kanban Rules
- WIP limit In Progress: 3 issues max per person
- Prioritize finishing in-progress work before taking new work
- Review the board at the start of each work session

## Issue Reuse Rule

**Before creating a new Jira issue**, always search the board for an existing issue in "To Do" that covers the same functionality. If one exists, **reuse it** instead of creating a new one — transition it through the workflow states as development progresses. This prevents duplicates and keeps the board clean.

Steps:
1. Search: `project = MINI AND summary ~ "keyword" AND status = "Por hacer"`
2. If found: use that issue key for the branch, commits, and PR
3. If NOT found: only then create a new issue

## Language Convention

All Jira issue summaries and descriptions must be written in **English**. This ensures consistency across the board and aligns with the codebase language.

## Mandatory Issue Rule (CRITICAL)

**NO code can be pushed to the repository without an associated Jira issue.** This is non-negotiable.

Before creating a branch or making any commit:
1. Search Jira for an existing issue that covers the work
2. If none exists, CREATE a new issue (Bug, Task, or Story as appropriate)
3. Transition it to "In Progress"
4. Use the issue key in the branch name and commit messages
5. Reference the issue in the PR description

This applies to ALL changes: features, bugfixes, documentation, CI/CD, config changes — everything.

**Violation**: If code was pushed without a Jira issue, retroactively create the issue and add a comment linking to the PR(s). This should never happen again.
