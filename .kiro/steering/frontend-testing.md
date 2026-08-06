---
inclusion: fileMatch
fileMatchPattern: "**/*.test.tsx,**/*.test.ts,**/*.spec.tsx,**/*.spec.ts,**/playwright*,**/e2e/**"
---

# Frontend Testing Strategy

## Overview
El frontend testing se divide en 3 niveles, cada uno con herramientas y objetivos distintos.

## Testing Pyramid (Frontend)

```
    ╱ E2E (Playwright) ╲        → Flujos completos en browser real
   ╱  Visual Regression  ╲      → Screenshots comparativos por viewport
  ╱   Integration Tests    ╲    → Componentes + hooks + API mocks
 ╱    Unit Tests (Vitest)    ╲  → Funciones, utils, lógica aislada
```

## 1. Unit Tests

### Herramientas
- **Runner**: Vitest (compatible con Vite, rápido)
- **DOM**: @testing-library/react
- **Assertions**: Vitest built-in + @testing-library/jest-dom
- **Mocks**: vitest mocking (vi.mock, vi.fn)

### Qué testear
- Hooks custom (useBooks, useAuth, etc.)
- Funciones utilitarias (formatters, validators, parsers)
- Lógica de componentes (states, callbacks)
- Transformaciones de datos (API response → UI model)

### Convenciones
- Archivo: `ComponentName.test.tsx` junto al componente
- Naming: `describe('ComponentName') > it('should do X when Y')`
- Evitar testing de estilos o implementación interna

### Comandos
```bash
cd src/MiniLibrary.Web
npm run test              # Watch mode
npm run test -- --run     # Single run (CI)
npm run test:coverage     # Con coverage report
```

## 2. Integration Tests

### Herramientas
- **Runner**: Vitest
- **Rendering**: @testing-library/react
- **API Mocking**: MSW (Mock Service Worker)
- **Router**: MemoryRouter (react-router)

### Qué testear
- Flujos de formulario completos (llenar + submit + feedback)
- Páginas con fetch de datos (loading → data → display)
- Interacciones entre componentes (filtros → lista actualizada)
- Error handling (API error → mensaje de error en UI)

### Convenciones
- Archivo: `FeatureName.integration.test.tsx`
- Setup MSW handlers por feature en `__mocks__/handlers/`
- Usar `screen.findBy*` para esperar contenido async

## 3. E2E Tests (Playwright)

### Herramientas
- **Framework**: Playwright
- **MCP Server**: `@executeautomation/playwright-mcp-server` (para interacción desde Kiro)
- **Screenshots**: Automáticos por viewport en cada test
- **Video**: Habilitado en CI para tests fallidos

### Qué testear
- Flujos críticos end-to-end (login → buscar libro → checkout)
- Navegación entre páginas
- Responsive behavior en múltiples viewports
- Accesibilidad (axe-core integration)

### Viewports de prueba

| Device | Width x Height | Nombre en config |
|--------|---------------|------------------|
| iPhone SE | 375 x 667 | `mobile-small` |
| iPhone 14 | 390 x 844 | `mobile` |
| iPad | 768 x 1024 | `tablet` |
| Laptop | 1440 x 900 | `desktop` |
| Monitor | 1920 x 1080 | `desktop-large` |

### Estructura de archivos E2E
```
tests/
├── e2e/
│   ├── fixtures/          # Test data y page objects
│   ├── login.spec.ts
│   ├── catalog.spec.ts
│   ├── checkout.spec.ts
│   ├── dashboard.spec.ts
│   └── responsive.spec.ts
├── evidence/              # Screenshots y videos generados
│   ├── screenshots/
│   │   ├── desktop/
│   │   ├── tablet/
│   │   └── mobile/
│   └── videos/
└── playwright.config.ts
```

### Playwright Config (referencia)
```typescript
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests/e2e',
  outputDir: './tests/evidence',
  use: {
    baseURL: 'http://localhost:3000',
    screenshot: 'on',
    video: 'on-first-retry',
    trace: 'on-first-retry',
  },
  projects: [
    { name: 'mobile-small', use: { viewport: { width: 375, height: 667 } } },
    { name: 'mobile', use: { viewport: { width: 390, height: 844 } } },
    { name: 'tablet', use: { viewport: { width: 768, height: 1024 } } },
    { name: 'desktop', use: { viewport: { width: 1440, height: 900 } } },
    { name: 'desktop-large', use: { viewport: { width: 1920, height: 1080 } } },
  ],
});
```

### Comandos E2E
```bash
# Ejecutar todos los E2E tests
npx playwright test

# Ejecutar solo en mobile
npx playwright test --project=mobile

# Con UI mode (debug visual)
npx playwright test --ui

# Generar report HTML
npx playwright show-report
```

## 4. Visual Regression Testing

### Enfoque
- Screenshots por viewport en puntos clave de cada flujo
- Comparación pixel-a-pixel contra baselines en `tests/evidence/baselines/`
- Threshold de diferencia: 0.1% (para ignorar anti-aliasing)

### Cuándo actualizar baselines
- Cuando hay un cambio intencional de diseño
- Comando: `npx playwright test --update-snapshots`
- Siempre revisar visualmente antes de commitear nuevos baselines

## 5. Evidencias de testing

### Screenshots automáticos
- Se generan en cada E2E test run
- Organizados por: `tests/evidence/screenshots/{viewport}/{test-name}.png`
- En CI: se suben como artifacts del workflow

### Videos
- Se graban automáticamente en CI cuando un test falla
- Ubicación: `tests/evidence/videos/`
- Formato: WebM
- Se suben como artifacts de GitHub Actions

### Reporte
- Playwright HTML Report: `tests/evidence/report/`
- Incluye: screenshots, videos, traces, tiempos de ejecución
- Se publica como GitHub Pages o artifact descargable en cada PR

## Coverage Requirements (Frontend)

| Nivel | Target |
|-------|--------|
| Unit tests | > 80% de funciones y hooks |
| Integration tests | Cada página principal cubierta |
| E2E tests | Todos los flujos críticos (login, búsqueda, checkout, check-in) |
| Visual regression | Todas las pantallas en todos los viewports |

## CI Integration

```yaml
# En el workflow de CI
frontend-tests:
  steps:
    - Unit + Integration tests (Vitest)
    - E2E tests (Playwright, all viewports)
    - Upload screenshots as artifacts
    - Upload video evidence for failures
    - Coverage report upload
```
