---
inclusion: always
---

# GitHub Workflow Guide

## Repositorio
- **Owner**: VaroJulio
- **Repo**: MiniLibrary
- **Plataforma**: GitHub
- **Registry**: ghcr.io (GitHub Container Registry)
- **CI/CD**: GitHub Actions

## Branching Strategy

```
main (producción, protegida — NUNCA push directo)
 └── develop (integración, protegida — NUNCA push directo)
      ├── feature/MINI-XX-description
      ├── bugfix/MINI-XX-description
      └── chore/description
```

### Reglas de protección
- **`main`**: solo recibe merges desde `develop` vía release PR. Push directo PROHIBIDO.
- **`develop`**: solo recibe merges desde feature/bugfix branches vía PR. Push directo PROHIBIDO.
- **Feature branches**: siempre desde `develop`, siempre con issue Jira en el nombre.
- **Hotfixes**: desde `main`, merge a `main` y `develop` (ambos vía PR).
- **Todo cambio, sin excepción (incluso docs, steering, configs) entra vía Pull Request.**

### Naming de branches
| Tipo | Formato | Ejemplo |
|------|---------|--------|
| Feature | `feature/MINI-XX-descripcion` | `feature/MINI-48-search-feature` |
| Bugfix | `bugfix/MINI-XX-descripcion` | `bugfix/MINI-55-fix-loan-limit` |
| Chore | `chore/descripcion` | `chore/update-steering-docs` |
| Hotfix | `hotfix/MINI-XX-descripcion` | `hotfix/MINI-60-auth-crash` |

## Pull Requests

### Cuándo crear un PR
- Al completar TODOS los sub-tasks de una tarea top-level.
- Nunca PRs parciales (un PR = una tarea completa).

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
- [ ] Build passes (`dotnet build`)
- [ ] All tests pass (`dotnet test`)

## Documentation
- [ ] API docs updated (Swagger XML comments)
- [ ] Architecture diagrams updated (if applicable)
- [ ] Changelog updated (`docs/CHANGELOG.md`)

Closes MINI-XX
```

### Merge Strategy
- Feature/Bugfix → develop: **Squash merge** (clean history, one commit per feature)
- Develop → main: **Merge commit** (preserve release boundary)

## GitHub Actions

### CI Pipeline (`ci.yml`)
- Trigger: PR to `develop` or `main`
- Jobs: restore, build, test, lint
- Required to pass before merge

### CD Pipeline (`cd.yml`)
- Trigger: push to `main`
- Jobs: build Docker images, push to ghcr.io, deploy

### Checks requeridos antes de merge
- CI pipeline passing
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

## GitHub Container Registry
- Imágenes: `ghcr.io/varojulio/minilibrary-api` y `ghcr.io/varojulio/minilibrary-web`
- Tags: `latest`, `v1.0.0`, `sha-abc1234`
