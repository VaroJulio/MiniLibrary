---
inclusion: fileMatch
fileMatchPattern: "**/*.tsx,**/*.css,**/*.scss,**/theme*,**/styles*,**/components/**"
---

# UI/UX Design Standards

## Design Philosophy
MiniLibrary debe transmitir **confianza, claridad y calidez** — como una biblioteca moderna bien organizada. La interfaz es limpia, con abundante espacio en blanco, jerarquía visual clara y transiciones suaves.

## Design System

### Framework
- **Component Library**: Material UI (MUI) v5
- **Theme**: Custom MiniLibrary theme extendiendo el default de MUI
- **Icons**: Material Icons + complementarios de Lucide React (para íconos de libros)

### Color Palette

#### Light Mode
| Token | Hex | Uso |
|-------|-----|-----|
| primary.main | `#1E3A5F` | Navegación, botones principales, headers |
| primary.light | `#4A6FA5` | Hover states, backgrounds secundarios |
| primary.dark | `#0F1F33` | Texto principal en contraste |
| secondary.main | `#F59E0B` | Acciones destacadas, badges, ratings |
| secondary.light | `#FCD34D` | Highlights, tooltips |
| success | `#10B981` | Disponible, éxito, confirmaciones |
| error | `#EF4444` | Errores, eliminación, alertas |
| warning | `#F97316` | Vencimiento próximo, atención |
| background.default | `#F8FAFC` | Fondo general de la app |
| background.paper | `#FFFFFF` | Cards, modales, drawers |
| text.primary | `#1E293B` | Texto principal |
| text.secondary | `#64748B` | Texto secundario, captions |

#### Dark Mode
| Token | Hex | Uso |
|-------|-----|-----|
| primary.main | `#60A5FA` | Navegación, botones principales |
| background.default | `#0F172A` | Fondo general |
| background.paper | `#1E293B` | Cards, superficies elevadas |
| text.primary | `#F1F5F9` | Texto principal |

### Typography
- **Font family UI**: `'Inter', 'Roboto', sans-serif`
- **Font family literary** (títulos de libros): `'Merriweather', serif`
- **Scale**: Seguir la escala de MUI (h1-h6, body1, body2, caption, overline)
- **Weights**: 400 (regular), 500 (medium), 600 (semibold), 700 (bold)

### Spacing & Grid
- **Base unit**: 8px
- **Container max-width**: 1200px (centrado)
- **Grid**: 12 columnas MUI Grid
- **Spacing entre secciones**: 32px (4 units)
- **Spacing entre elementos**: 16px (2 units)
- **Padding de cards**: 24px (3 units)

### Border Radius
- Cards: `12px`
- Buttons: `8px`
- Inputs: `8px`
- Chips/Badges: `16px` (pill)
- Avatar: `50%`

### Shadows
- Elevation 1 (cards): `0 1px 3px rgba(0,0,0,0.08)`
- Elevation 2 (hover): `0 4px 12px rgba(0,0,0,0.12)`
- Elevation 3 (modales): `0 8px 24px rgba(0,0,0,0.16)`

## Components por caso de uso

| Caso de uso | Componente MUI | Notas |
|-------------|----------------|-------|
| Lista de libros | Card + CardMedia + CardContent | Grid responsivo |
| Tabla de préstamos | DataGrid | Con sorting y filtros |
| Búsqueda | TextField + InputAdornment (icon) | Con debounce 300ms |
| Filtros | Chip (seleccionables) + Drawer en mobile | Colapsibles |
| Navegación desktop | Drawer permanente (240px) | Con íconos + texto |
| Navegación mobile | BottomNavigation | 4-5 items principales |
| Feedback éxito/error | Snackbar + Alert | Auto-dismiss 5s |
| Confirmaciones | Dialog | Acción destructiva requiere confirmación |
| Loading | Skeleton | Mismo layout que el contenido final |
| Empty states | Custom (ilustración + texto + CTA) | Siempre con acción sugerida |
| Formularios | Stack vertical + TextField | Labels encima, errores inline abajo |
| Paginación | TablePagination o Pagination | Siempre con total count visible |

## Responsive Breakpoints

| Breakpoint | Width | Layout |
|------------|-------|--------|
| xs (mobile) | 0-599px | Single column, bottom nav, cards full-width |
| sm (tablet portrait) | 600-899px | 1-2 columns, hamburger menu |
| md (tablet landscape) | 900-1199px | 2-3 columns, side drawer colapsible |
| lg (desktop) | 1200-1535px | 3-4 columns, side drawer permanente |
| xl (large desktop) | 1536px+ | Max-width container, centrado |

### Viewport targets para testing
- **Mobile**: 375x667 (iPhone SE), 390x844 (iPhone 14)
- **Tablet**: 768x1024 (iPad)
- **Desktop**: 1440x900 (laptop), 1920x1080 (monitor)

## UX Principles

### 1. Three-Click Rule
Cualquier acción principal debe ser alcanzable en máximo 3 clicks desde cualquier pantalla.

### 2. Immediate Feedback
- Loading skeleton visible en < 100ms tras interacción
- Optimistic updates para acciones de usuario (check-out, favoritos)
- Toast de confirmación tras cada acción exitosa

### 3. Progressive Disclosure
- Mostrar información esencial primero
- Detalles adicionales bajo "Ver más" o en panel lateral
- Filtros avanzados colapsados por defecto

### 4. Error Prevention
- Validación inline en tiempo real (debounce 500ms)
- Confirmación para acciones destructivas
- Deshacer disponible durante 5s para acciones reversibles

### 5. Consistent Patterns
- Misma estructura de página para todas las listas (header + filters + content + pagination)
- Mismo patrón de formulario para todos los CRUD
- Navegación siempre visible y predecible

## Performance Targets

| Metric | Target | Tool |
|--------|--------|------|
| First Contentful Paint (FCP) | < 1.5s | Lighthouse |
| Time to Interactive (TTI) | < 3s | Lighthouse |
| Largest Contentful Paint (LCP) | < 2.5s | Lighthouse |
| Cumulative Layout Shift (CLS) | < 0.1 | Lighthouse |
| Initial bundle size | < 200KB gzipped | Vite build |
| Route change | < 300ms | Custom metric |

### Técnicas de performance
- Code splitting por ruta (React.lazy + Suspense)
- Lazy loading de imágenes de portada
- Prefetch de rutas probables (hover en nav)
- React Query stale-while-revalidate
- Virtualización para listas > 50 items (react-window)

## Accessibility (WCAG 2.1 AA)
- Contraste mínimo 4.5:1 para texto, 3:1 para elementos grandes
- Focus visible en todos los elementos interactivos
- Skip to main content link
- Aria-labels en iconos sin texto
- Keyboard navigation completa (Tab, Enter, Escape)
- Screen reader announcements para cambios dinámicos (aria-live)
- Reducir animaciones si `prefers-reduced-motion` está activo
