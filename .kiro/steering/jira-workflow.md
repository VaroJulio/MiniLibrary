---
inclusion: always
---

# Jira Workflow - Kanban

## Proyecto
- **Herramienta**: Jira Cloud (https://ajjbdeveloper.atlassian.net)
- **Metodología**: Kanban (no sprints)
- **Board**: MiniLibrary Kanban Board

## Estados del Kanban

| Estado | Descripción |
|--------|-------------|
| Backlog | Historia/tarea identificada, pendiente de priorizar |
| To Do | Priorizada y lista para ser trabajada |
| In Progress | Actualmente en desarrollo |
| In Review | PR creado, en revisión de código |
| Done | Completada, PR mergeado y desplegado |

## Tipos de Issues

| Tipo | Uso | Prefijo de branch |
|------|-----|-------------------|
| Epic | Funcionalidad grande (requisito completo) | N/A |
| Story | Historia de usuario implementable | `feature/` |
| Task | Trabajo técnico no funcional | `chore/` |
| Bug | Defecto encontrado | `bugfix/` |
| Sub-task | División de una Story/Task | Usa el del padre |

## Convenciones de Issues en Jira

### Naming
- **Epic**: Nombre del requisito (ej: "Gestión del Catálogo de Libros")
- **Story**: Historia de usuario completa (ej: "Como Member, quiero buscar libros por título")
- **Task**: Acción técnica clara (ej: "Configurar EF Core migrations para Books")

### Campos requeridos
- **Summary**: Título conciso
- **Description**: Contexto, criterios de aceptación, notas técnicas
- **Labels**: `backend`, `frontend`, `infrastructure`, `documentation`
- **Priority**: Highest, High, Medium, Low, Lowest

### Vinculación código ↔ Jira
- Incluir el ID del issue en el branch name: `feature/MINI-42-search-books`
- Incluir el ID en commits: `feat(books): add search endpoint [MINI-42]`
- Referenciar en PR description: `Closes MINI-42`

## Flujo de trabajo

1. Crear/tomar issue del Backlog → mover a **To Do**
2. Al iniciar desarrollo → mover a **In Progress**, crear branch
3. Al crear PR → mover a **In Review**
4. Al mergear PR → mover a **Done**

## Reglas Kanban
- WIP limit In Progress: 3 issues máximo por persona
- Priorizar terminar lo que está en progreso antes de tomar nuevo trabajo
- Revisar el board al inicio de cada sesión de trabajo
