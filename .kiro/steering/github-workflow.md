---
inclusion: fileMatch
fileMatchPattern: "**/.github/**,**/README*,**/*.yml"
---

# GitHub Workflow Guide

## Repositorio
- **Plataforma**: GitHub
- **Registry**: ghcr.io (GitHub Container Registry)
- **CI/CD**: GitHub Actions

## Branching Strategy

```
main (producción, protegida)
 └── develop (integración, protegida)
      ├── feature/MINI-XX-description
      ├── bugfix/MINI-XX-description
      └── chore/description
```

### Reglas
- `main`: solo recibe merges desde `develop` vía release PR
- `develop`: recibe PRs de feature/bugfix branches
- Feature branches: siempre desde `develop`, siempre con issue Jira
- Hotfixes: desde `main`, merge a `main` y `develop`

## Pull Requests

### Título
Formato: `type(scope): description [MINI-XX]`
Ejemplo: `feat(books): add semantic search endpoint [MINI-45]`

### Descripción (template)
```markdown
## Summary
Brief description of changes.

## Jira Issue
[MINI-XX](https://ajjbdeveloper.atlassian.net/browse/MINI-XX)

## Changes
- Change 1
- Change 2

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Manual testing performed

## Documentation
- [ ] API docs updated
- [ ] Architecture diagrams updated (if applicable)
- [ ] Changelog updated
```

### Merge Strategy
- Feature → develop: **Squash merge** (clean history)
- Develop → main: **Merge commit** (preserve release boundary)

## GitHub Actions

### CI Pipeline (`ci.yml`)
- Trigger: PR to `develop` or `main`
- Jobs: build, test, lint, coverage
- Required to pass before merge

### CD Pipeline (`cd.yml`)
- Trigger: push to `main`
- Jobs: build Docker images, push to ghcr.io, deploy

### Checks requeridos antes de merge
- CI pipeline passing
- Al menos 1 approval (si hay más de 1 contributor)
- No conflictos con base branch
- Branch actualizado con base

## Releases
- Usar GitHub Releases para versiones
- Tag format: `v1.0.0` (SemVer)
- Release notes: auto-generated desde commits + manual highlights

## Secrets del repositorio
Los siguientes secrets deben estar configurados en GitHub Settings > Secrets:
- `DOCKER_REGISTRY_TOKEN` - Para push a ghcr.io
- `OPENAI_API_KEY` - Para features de IA
- `SA_PASSWORD` - Para SQL Server en tests de integración
- `DEPLOY_SSH_KEY` - Para deploy (si aplica)

## GitHub Container Registry
- Imágenes publicadas en: `ghcr.io/ajjbdeveloper/minilibrary-api` y `ghcr.io/ajjbdeveloper/minilibrary-web`
- Tags: `latest`, `v1.0.0`, `sha-abc1234`
