# Browser Functional Tests

Automated end-to-end browser tests for MiniLibrary using [Playwright MCP](https://playwright.dev/mcp/introduction) (Microsoft's official MCP server for browser automation).

## Prerequisites

- Node.js 18+ (for `npx`)
- Local Docker environment running (`docker compose -f docker/docker-compose.yml up -d`)
- Seed data loaded (`./scripts/seed-data.sh`)
- Kiro IDE with MCP support

## MCP Server Configuration

Add the following to your `.kiro/settings/mcp.json`:

```json
{
  "mcpServers": {
    "playwright": {
      "command": "npx",
      "args": ["@playwright/mcp@latest", "--headless"],
      "disabled": false
    }
  }
}
```

### Configuration Options

| Option | Description |
|--------|-------------|
| `--headless` | Run browser without visible window (default for CI) |
| `--browser=firefox` | Use Firefox instead of Chrome |
| `--viewport-size=1280x720` | Set viewport dimensions |
| Remove `--headless` | See the browser in action (debugging) |

For headed mode (watch tests execute):
```json
{
  "mcpServers": {
    "playwright": {
      "command": "npx",
      "args": ["@playwright/mcp@latest"],
      "disabled": false
    }
  }
}
```

## How It Works

Playwright MCP uses **accessibility snapshots** to interact with the page. Instead of pixel coordinates or fragile CSS selectors, it reads the accessibility tree and identifies elements by their role, label, and ref ID. This makes tests resilient to visual changes.

## Test Scenarios

The test scenarios below are designed to be executed by Kiro using the Playwright MCP tools. Each scenario describes the user flow and expected outcomes.

### Scenario 1: Dev Login Flow

**Goal**: Verify all three dev login roles work correctly.

```
Steps:
1. Navigate to http://localhost:3000/login
2. Verify the login page loads (heading "MiniLibrary", "Sign in to access the library system")
3. Select "Admin" from the Role dropdown
4. Click "Dev Login as Admin"
5. Verify redirect to dashboard/home page
6. Verify user avatar or menu is visible (authenticated state)
7. Logout
8. Repeat for "Librarian" and "Member" roles
```

### Scenario 2: Book Catalog Browsing

**Goal**: Verify the catalog page displays books and supports navigation.

```
Steps:
1. Login as Member (Dev Login)
2. Navigate to /catalog (click "Catalog" in sidebar)
3. Verify book cards are displayed (at least 1 book visible)
4. Verify each book card shows: title, author, category
5. Click on a book to view detail
6. Verify book detail page shows: title, author, description, ratings section
7. Navigate back to catalog
```

### Scenario 3: Book Search

**Goal**: Verify text search works with filters.

```
Steps:
1. Login as Member
2. Navigate to /search
3. Type "Foundation" in the search bar
4. Verify search results appear
5. Verify results contain a book with "Foundation" in the title
6. Clear search, type a non-existent term "xyznonexistent"
7. Verify empty state is shown
```

### Scenario 4: Loan Creation and Return

**Goal**: Verify a member can check out and return a book.

```
Steps:
1. Login as Member
2. Navigate to catalog, find an available book
3. Click "Check Out" button on the book
4. Verify success feedback (toast/redirect)
5. Navigate to /my-loans
6. Verify the borrowed book appears in the loans table with status "Active"
7. Click "Return" button for the loan
8. Verify the loan status changes or book disappears from active loans
```

### Scenario 5: Rate a Book

**Goal**: Verify a member can rate a returned book.

```
Steps:
1. Login as Member
2. Navigate to /my-loans
3. If a returned book exists, navigate to its detail page
4. Verify the rating form is visible (stars + text area)
5. Select 4 stars
6. Enter review text "Great book, highly recommend!"
7. Submit the rating
8. Verify the rating appears in the book's reviews section
```

### Scenario 6: Admin User Management

**Goal**: Verify admin can view and manage users.

```
Steps:
1. Login as Admin
2. Navigate to /users (User Management in sidebar)
3. Verify user list is displayed with columns (Name, Email, Role)
4. Verify at least one user is listed
5. Find a Member user
6. Change their role to Librarian via the role dropdown
7. Verify the role change is reflected in the table
8. Change it back to Member (cleanup)
```

### Scenario 7: Dark Mode Toggle

**Goal**: Verify the theme toggle works correctly.

```
Steps:
1. Login as any role
2. Note the current theme (check background color)
3. Click the theme toggle icon (sun/moon) in the top-right
4. Verify the background color changes (dark ↔ light)
5. Reload the page
6. Verify the theme persists after reload (localStorage)
```

## Running Tests

### Via Kiro Chat

In a Kiro session with Playwright MCP configured, ask:

> "Run browser functional test scenario 1 (Dev Login Flow) against http://localhost:3000"

Kiro will use the Playwright MCP tools (`browser_navigate`, `browser_snapshot`, `browser_click`, `browser_type`) to execute the test steps.

### Via Kiro Hook (On Demand)

A hook can trigger a smoke test. See `.kiro/hooks/browser-smoke-test.json` for the PostTaskExec hook that runs a basic login verification after task completion.

## Troubleshooting

| Problem | Solution |
|---------|----------|
| "Cannot connect to localhost:3000" | Run `docker compose -f docker/docker-compose.yml up -d` |
| "No books in catalog" | Run `./scripts/seed-data.sh` |
| Playwright not found | Ensure Node.js 18+ is installed and `npx` is available |
| Tests fail after code changes | Rebuild: `docker compose -f docker/docker-compose.yml build web && docker compose up -d web` |

## Architecture Decision

We chose **Microsoft Playwright MCP** (`@playwright/mcp`) over alternatives because:

1. **Open source** — No API keys, no external service dependencies
2. **Local execution** — Runs entirely on the developer's machine
3. **Accessibility-based** — Uses a11y tree instead of visual/pixel matching
4. **Official support** — Maintained by Microsoft as part of the Playwright ecosystem
5. **Zero config** — Works with just `npx`, no installation step needed
6. **IDE integration** — Native MCP support in Kiro

See [ADR-0003](../docs/architecture/decisions/0003-browser-test-tool-selection.md) for the full decision record.
