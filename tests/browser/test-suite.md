# MiniLibrary — Browser Functional Test Suite

This is the master catalog of browser functional tests. Each test case is designed to be:
- **Idempotent** — Can run independently, in any order
- **Repeatable** — Same result every time with seed data loaded
- **Taggable** — Execute subsets by tag or module

## How to Use

Ask Kiro in chat:
- "Ejecuta la suite de regresión completa" → runs all tests
- "Ejecuta los tests #smoke" → runs only smoke-tagged tests
- "Ejecuta los tests del módulo Loans" → runs TC-07 and TC-08
- "Agrega un test para [nueva funcionalidad]" → I'll add it to this file

## Preconditions (All Tests)

- Docker environment running: `docker compose -f docker/docker-compose.yml up -d`
- Seed data loaded: `./scripts/seed-data.sh`
- Frontend accessible at: http://localhost:3000
- API healthy at: http://localhost:5000/health

---

## TC-01: Login Dev como Member
- **Módulo**: Auth
- **Prioridad**: Alta
- **Tags**: #auth #smoke #critical
- **Precondiciones**: Ninguna adicional
- **Pasos**:
  1. Navegar a http://localhost:3000/login
  2. Verificar que la página muestra heading "MiniLibrary" y texto "Sign in to access the library system"
  3. Seleccionar "Member" en el dropdown de Role
  4. Clic en "Dev Login as Member"
  5. Verificar redirect a página autenticada
- **Resultado esperado**: Sidebar visible con opciones: Catalog, Search, My Loans, Recommendations, Ratings, Rankings, Wishlist, Badges
- **Resultado NO esperado**: Página de login sigue visible, error en consola, o redirect a /login

---

## TC-02: Login Dev como Admin
- **Módulo**: Auth
- **Prioridad**: Alta
- **Tags**: #auth #smoke #critical
- **Precondiciones**: Ninguna adicional
- **Pasos**:
  1. Navegar a http://localhost:3000/login
  2. Seleccionar "Admin" en el dropdown de Role
  3. Clic en "Dev Login as Admin"
  4. Verificar redirect a página autenticada
- **Resultado esperado**: Sidebar visible con opciones incluyendo "User Management" y "Dashboard" (opciones Admin-only)
- **Resultado NO esperado**: Opciones de Admin no visibles, o redirect a /login

---

## TC-03: Login Dev como Librarian
- **Módulo**: Auth
- **Prioridad**: Alta
- **Tags**: #auth #critical
- **Precondiciones**: Ninguna adicional
- **Pasos**:
  1. Navegar a http://localhost:3000/login
  2. Seleccionar "Librarian" en el dropdown de Role
  3. Clic en "Dev Login as Librarian"
  4. Verificar redirect a página autenticada
- **Resultado esperado**: Sidebar visible con opciones incluyendo "Dashboard" (Librarian tiene acceso a dashboard)
- **Resultado NO esperado**: Error de autenticación o sidebar vacío

---

## TC-04: Listado de libros carga correctamente
- **Módulo**: Catalog
- **Prioridad**: Alta
- **Tags**: #catalog #smoke
- **Precondiciones**: Login como cualquier rol, seed data loaded
- **Pasos**:
  1. Login como Member
  2. Navegar a /catalog (clic en "Catalog" en sidebar)
  3. Verificar que la página carga
  4. Verificar que hay al menos 1 libro visible (card o lista)
- **Resultado esperado**: Se muestran book cards con título y autor visibles
- **Resultado NO esperado**: Página vacía, spinner infinito, o mensaje de error

---

## TC-05: Detalle de libro muestra información completa
- **Módulo**: Catalog
- **Prioridad**: Media
- **Tags**: #catalog
- **Precondiciones**: Login como Member, al menos 1 libro en catálogo
- **Pasos**:
  1. Login como Member
  2. Navegar a /catalog
  3. Clic en el primer libro disponible
  4. Verificar que la página de detalle carga
- **Resultado esperado**: Se muestra título, autor, descripción, y sección de ratings/reviews
- **Resultado NO esperado**: Página en blanco, 404, o campos faltantes

---

## TC-06: Búsqueda por texto retorna resultados
- **Módulo**: Search
- **Prioridad**: Alta
- **Tags**: #search #smoke
- **Precondiciones**: Login como Member, seed data con libros
- **Pasos**:
  1. Login como Member
  2. Navegar a /search
  3. Escribir "Foundation" en el campo de búsqueda
  4. Esperar resultados (debounce ~300ms)
  5. Verificar que aparecen resultados
- **Resultado esperado**: Al menos 1 resultado que contiene "Foundation" en el título
- **Resultado NO esperado**: Sin resultados para un término que existe en seed data

---

## TC-07: Checkout de un libro
- **Módulo**: Loans
- **Prioridad**: Alta
- **Tags**: #loans #critical
- **Precondiciones**: Login como Member, al menos 1 libro con status "Available"
- **Pasos**:
  1. Login como Member
  2. Navegar a /catalog
  3. Encontrar un libro con botón "Check Out" visible
  4. Clic en "Check Out"
  5. Verificar feedback de éxito
  6. Navegar a /my-loans
  7. Verificar que el libro aparece con status "Active"
- **Resultado esperado**: Libro aparece en My Loans con fecha de vencimiento (14 días desde hoy)
- **Resultado NO esperado**: Error 422/500, libro no aparece en My Loans

---

## TC-08: Devolución de un libro
- **Módulo**: Loans
- **Prioridad**: Alta
- **Tags**: #loans #critical
- **Precondiciones**: Login como Member, al menos 1 préstamo activo
- **Pasos**:
  1. Login como Member
  2. Navegar a /my-loans
  3. Verificar que hay al menos 1 préstamo con status "Active"
  4. Clic en botón "Return" del préstamo activo
  5. Verificar que el diálogo de rating aparece o el préstamo desaparece de activos
- **Resultado esperado**: Préstamo ya no muestra "Active" (cambió a "Returned" o desapareció), posible diálogo de rating
- **Resultado NO esperado**: Error, botón no responde, préstamo sigue activo

---

## TC-09: Todas las páginas del sidebar cargan sin errores
- **Módulo**: Navigation
- **Prioridad**: Media
- **Tags**: #navigation #regression
- **Precondiciones**: Login como Member
- **Pasos**:
  1. Login como Member
  2. Para cada ítem del sidebar (Catalog, Search, My Loans, Recommendations, Ratings, Rankings, Wishlist, Badges):
     a. Clic en el ítem
     b. Verificar que la página carga (no muestra error, no se queda en spinner infinito)
     c. Verificar que el heading de la página corresponde al ítem
- **Resultado esperado**: Todas las 8 páginas cargan con su heading correcto
- **Resultado NO esperado**: Alguna página muestra error, blank screen, o spinner infinito (>5s)

---

## TC-10: Toggle dark/light mode persiste tras recarga
- **Módulo**: UI
- **Prioridad**: Media
- **Tags**: #ui #regression
- **Precondiciones**: Login como cualquier rol
- **Pasos**:
  1. Login como Member
  2. Identificar el modo actual (revisar si hay toggle de tema en la barra superior)
  3. Clic en el toggle de tema (icono sol/luna)
  4. Verificar que el snapshot de accesibilidad refleja cambio (elementos siguen visibles y funcionales)
  5. Recargar la página (navegar a misma URL)
  6. Verificar que el modo persiste después de la recarga
- **Resultado esperado**: El tema cambia y persiste tras reload (stored in localStorage)
- **Resultado NO esperado**: Tema revierte al default tras reload

---

## Adding New Tests

To add a new test case, append it following this template:

```markdown
## TC-XX: [Descriptive name]
- **Módulo**: [Auth|Catalog|Search|Loans|Ratings|Navigation|UI|Admin]
- **Prioridad**: [Alta|Media|Baja]
- **Tags**: #module #smoke|#regression|#critical
- **Precondiciones**: [What needs to be true before running]
- **Pasos**:
  1. Step 1
  2. Step 2
- **Resultado esperado**: [Specific, verifiable outcome]
- **Resultado NO esperado**: [What would indicate failure]
```

Increment TC number sequentially. Add relevant tags for selective execution.
