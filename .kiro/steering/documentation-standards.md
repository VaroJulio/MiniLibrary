---
inclusion: always
---

# Documentation Standards

## Principio
Cada feature implementada o cambio significativo DEBE documentarse. La documentación es parte de "Done".

## Qué documentar

### Por cada feature/cambio:
1. **Changelog** (`docs/CHANGELOG.md`): Entrada con fecha, issue Jira, y descripción breve
2. **API docs** (`docs/api/`): Endpoints nuevos o modificados con request/response examples
3. **Architecture docs** (`docs/architecture/`): Si el cambio afecta la arquitectura, actualizar diagramas
4. **README.md**: Si cambian instrucciones de setup o uso

### ADRs (Architecture Decision Records)
- Ubicación: `docs/architecture/decisions/`
- Formato: `NNNN-titulo-descriptivo.md`
- Usar para: decisiones técnicas significativas (elección de library, cambio de patrón, etc.)
- Template:
  ```markdown
  # NNNN - Título

  ## Estado
  Aceptado | Propuesto | Depreciado

  ## Contexto
  Qué problema estamos resolviendo.

  ## Decisión
  Qué decidimos hacer.

  ## Consecuencias
  Qué implicaciones tiene esta decisión.
  ```

## Formato de Changelog

```markdown
## [Unreleased]

### Added
- Descripción del feature [MINI-XX]

### Changed
- Descripción del cambio [MINI-XX]

### Fixed
- Descripción del fix [MINI-XX]
```

Seguir formato [Keep a Changelog](https://keepachangelog.com/).

## Documentación de API

- Mantener OpenAPI/Swagger actualizado automáticamente vía Swashbuckle
- Agregar XML comments en controllers para documentación inline
- Para endpoints complejos, agregar ejemplos en `docs/api/examples/`

## Diagramas
- Formato: Mermaid (renderizable en GitHub y con MCP mermaid)
- Ubicación: `docs/architecture/`
- Actualizar cuando cambie la estructura del sistema
- Ver `architecture-diagrams.md` steering para convenciones

## Idioma
- Código: inglés
- Documentación técnica: inglés o español (consistente dentro del documento)
- Comentarios en código: inglés
- Commits y PRs: inglés
