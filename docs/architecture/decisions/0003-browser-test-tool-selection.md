# 0003 - Browser Functional Test Tool Selection

## Status
Accepted

## Context
MiniLibrary needs automated browser functional tests to validate end-to-end user flows (login, catalog browsing, loan checkout/return, ratings, admin management). The project already has unit tests (xUnit) and integration tests (TestContainers) but lacks E2E browser coverage.

We evaluated three MCP-compatible browser automation tools:

| Tool | Type | Cost | Setup |
|------|------|------|-------|
| Skyvern | AI-powered cloud | API key required | External service |
| Browserbase | Cloud browser infra | Account required | External dependency |
| Playwright MCP | Local browser automation | Free / open source | npx only |

## Decision
We selected **Microsoft Playwright MCP** (`@playwright/mcp`) as the browser functional test tool.

## Consequences

### Positive
- No external service dependencies or API keys
- Runs entirely on the developer's machine (offline-capable)
- Uses accessibility snapshots instead of visual matching (resilient to CSS changes)
- Maintained by Microsoft as part of the official Playwright ecosystem
- Zero installation beyond Node.js (`npx @playwright/mcp@latest`)
- Native MCP integration with Kiro IDE
- Supports headed mode for debugging and headless for automation

### Negative
- Not AI-driven (relies on Kiro's LLM to interpret accessibility snapshots)
- Requires local Docker environment running for tests
- No built-in cloud/CI browser infrastructure (would need separate setup for CI)
- Tests are conversational (executed via Kiro chat) rather than scriptable CI jobs

### Tradeoffs
- Chose local simplicity over cloud scalability
- Chose open-source over AI-powered (Skyvern) to avoid vendor lock-in
- Test scenarios are documented as instructions rather than executable scripts
