---
inclusion: fileMatch
fileMatchPattern: "**/.github/**,**/workflow*"
---

# CI/CD Guide

## GitHub Actions Workflows

### CI Pipeline (ci.yml)
Triggered on: PR to `develop` or `main`
Steps:
1. Checkout code
2. Setup .NET 8 SDK
3. Setup Node.js 20
4. Restore NuGet packages
5. Build solution
6. Run unit tests
7. Run integration tests (with service containers)
8. Build frontend
9. Run frontend tests
10. Upload coverage reports

### CD Pipeline (cd.yml)
Triggered on: Push to `main`
Steps:
1. Build Docker images
2. Push to GitHub Container Registry (ghcr.io)
3. Deploy to target environment

## Secrets Required in GitHub
- `DOCKER_REGISTRY_TOKEN`
- `DEPLOY_SSH_KEY` (if deploying to VM)
- `OPENAI_API_KEY`
- `SA_PASSWORD`

## Branch Protection Rules
- `main`: Require PR, require CI passing, require 1 approval
- `develop`: Require PR, require CI passing
