---
inclusion: manual
---
# Browser Testing Workflow

This steering defines how to execute browser functional tests for MiniLibrary using Playwright MCP.

## When to Use

Activate this steering (`#browser-testing`) when:
- The user asks to "run tests", "run regression", "execute browser tests"
- The user asks to verify a specific flow after a code change
- Running post-deployment smoke tests

## Execution Procedure

### 1. Verify Environment

Before running any test, verify:
```
curl -sf http://localhost:3000 → HTTP 200 (frontend)
curl -sf http://localhost:5000/health → HTTP 200 (API)
```

If either fails, inform the user:
> "The local Docker environment is not running. Start it with: `docker compose -f docker/docker-compose.yml up -d`"

### 2. Load Test Suite

Read the test cases from: `tests/browser/test-suite.md`

### 3. Execute Tests

For each test case (or filtered subset):

1. **Read the TC** — understand preconditions, steps, expected result
2. **Execute steps** using Playwright MCP tools:
   - `browser_navigate` — go to URLs
   - `browser_snapshot` — read the accessibility tree
   - `browser_click` — click elements by ref
   - `browser_type` — type in inputs
   - `browser_select_option` — select dropdown values
3. **Evaluate result** — compare snapshot against expected result
4. **Record** — PASS or FAIL with details

### 4. Generate Report

Create report at: `tests/browser/reports/YYYY-MM-DD-{type}.md`

Where `{type}` is:
- `regression` — full suite
- `smoke` — only #smoke tagged tests
- `targeted-{module}` — specific module (e.g., `targeted-loans`)

Report format:
```markdown
# Browser Functional Test Report

- **Date**: YYYY-MM-DD HH:MM
- **Type**: regression | smoke | targeted
- **Branch**: current git branch
- **Environment**: localhost:3000 (Docker)
- **Tool**: Playwright MCP (headless)

## Summary

| Total | Passed | Failed | Skipped |
|-------|--------|--------|---------|
| X     | X      | X      | X       |

## Results

| # | Test Case | Module | Status | Notes |
|---|-----------|--------|--------|-------|
| 1 | Login Dev Member | Auth | PASS | — |
| 2 | ... | ... | ... | ... |

## Failed Tests Detail

### TC-XX: [Name]
- **Expected**: ...
- **Actual**: ...
- **Snapshot**: (relevant portion of a11y tree)
```

### 5. Report to User

After execution, provide:
- Summary line: "Regression complete: 9/10 PASS, 1 FAIL"
- Table of results
- Detail on any failures
- Ask if they want to commit the report

## Selective Execution

When the user requests a subset:

| User says | Execute |
|-----------|---------|
| "tests smoke" | Only tests tagged #smoke |
| "tests critical" | Only tests tagged #critical |
| "tests de loans" | Only tests in módulo Loans |
| "tests de auth" | Only tests in módulo Auth |
| "test TC-07" | Only that specific test case |
| "regresión completa" | All tests in order |

## Adding New Tests

When the user says "agrega un test para X":
1. Read `tests/browser/test-suite.md` to get the last TC number
2. Append a new TC following the template at the bottom of the file
3. Confirm the addition to the user

## Important Rules

- Never modify seed data during tests (tests must be idempotent)
- If a test requires a specific state (e.g., active loan), verify it exists first; if not, mark as SKIPPED
- Always logout between test cases that require different roles
- Report actual accessibility snapshot content as evidence, not assumptions
- If Playwright MCP is not available, inform the user and offer to document expected manual steps instead
