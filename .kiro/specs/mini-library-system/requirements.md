# Requirements Document

## Introduction

MiniLibrary es un sistema de gestión de biblioteca completo que permite administrar el catálogo de libros, gestionar préstamos (check-in/check-out), y ofrecer funcionalidades avanzadas como búsqueda semántica y recomendaciones impulsadas por IA. El sistema soporta tres roles de usuario (Admin, Librarian, Member) con autenticación SSO mediante Google y Microsoft OAuth. La aplicación se despliega mediante contenedores Docker con CI/CD automatizado a través de GitHub Actions.

## Glossary

- **Sistema**: El sistema MiniLibrary en su conjunto (backend API + frontend web)
- **API**: El backend ASP.NET Core 8 Web API que expone endpoints RESTful
- **Frontend**: La aplicación React 18 SPA que consume la API
- **Libro**: Entidad del dominio que representa un libro en la biblioteca con metadatos asociados
- **Préstamo**: Registro de un libro prestado a un miembro, incluyendo fechas de inicio y devolución
- **Catálogo**: Colección completa de libros registrados en el sistema
- **Admin**: Rol con acceso completo al sistema, gestión de usuarios y asignación de roles
- **Librarian**: Rol que gestiona el catálogo de libros, préstamos y reportes
- **Member**: Rol que puede buscar libros, solicitar préstamos y ver su historial
- **Check-out**: Acción de registrar un préstamo de libro a un miembro
- **Check-in**: Acción de registrar la devolución de un libro prestado
- **Búsqueda_Semántica**: Búsqueda potenciada por IA que entiende el significado de la consulta, no solo coincidencias textuales
- **Motor_Recomendaciones**: Componente que utiliza OpenAI API para sugerir libros relevantes al usuario
- **Dashboard**: Panel de control con estadísticas y métricas del sistema

## Requirements

### Requisito 1: Gestión del Catálogo de Libros

**Historia de Usuario:** Como Librarian, quiero agregar, editar y eliminar libros del catálogo, para mantener el inventario de la biblioteca actualizado.

#### Criterios de Aceptación

1. WHEN un Librarian envía un formulario de creación de libro con datos válidos (título: máximo 255 caracteres, autor: máximo 200 caracteres, ISBN: formato ISBN-13 de 13 dígitos, año de publicación: entre 1450 y el año actual, descripción: máximo 2000 caracteres, categoría: máximo 100 caracteres), THE API SHALL crear el Libro con estado inicial "Available" y retornar el recurso creado con código HTTP 201
2. WHEN un Librarian envía una solicitud de actualización con datos válidos para un Libro existente, THE API SHALL actualizar el Libro y retornar el recurso actualizado con código HTTP 200
3. WHEN un Librarian envía una solicitud de eliminación para un Libro existente sin préstamos activos, THE API SHALL eliminar el Libro y retornar código HTTP 204
4. IF un Librarian intenta eliminar un Libro con préstamos activos, THEN THE API SHALL rechazar la operación y retornar código HTTP 409 con un mensaje indicando que el libro no puede eliminarse porque tiene préstamos activos
5. IF un Librarian envía datos de creación o actualización que no pasan la validación (título vacío o mayor a 255 caracteres, ISBN con formato inválido o duplicado en el catálogo, año de publicación fuera del rango 1450 al año actual, autor vacío), THEN THE API SHALL retornar código HTTP 422 con los errores de validación por cada campo que falló
6. THE API SHALL almacenar los siguientes metadatos para cada Libro: título, autor, ISBN, año de publicación, descripción, categoría y estado (Available, CheckedOut)
7. WHEN un Admin envía solicitudes de gestión de libros, THE API SHALL procesarlas con los mismos permisos que un Librarian
8. IF un Librarian o Admin envía una solicitud de actualización o eliminación para un Libro que no existe, THEN THE API SHALL retornar código HTTP 404 con un mensaje indicando que el libro no fue encontrado
9. IF un usuario con rol Member envía una solicitud de creación, actualización o eliminación de libros, THEN THE API SHALL rechazar la operación y retornar código HTTP 403

### Requisito 2: Sistema de Préstamos (Check-in/Check-out)

**Historia de Usuario:** Como Member, quiero poder tomar prestados y devolver libros, para utilizar los recursos de la biblioteca.

#### Criterios de Aceptación

1. WHEN un Member solicita un check-out de un Libro con estado Available y el Member tiene menos de 5 Préstamos activos, THE API SHALL crear un registro de Préstamo con fecha de préstamo igual a la fecha actual y fecha de vencimiento igual a 14 días después de la fecha de préstamo, cambiar el estado del Libro a CheckedOut, y retornar los detalles del Préstamo con código HTTP 200
2. WHEN un Member solicita un check-in de un Libro que tiene prestado, THE API SHALL actualizar el registro de Préstamo con la fecha de devolución igual a la fecha actual, cambiar el estado del Libro a Available, y retornar código HTTP 200
3. IF un Member solicita un check-out de un Libro con estado CheckedOut, THEN THE API SHALL rechazar la operación y retornar código HTTP 409 indicando que el Libro no está disponible
4. IF un Member solicita un check-in de un Libro que no tiene prestado, THEN THE API SHALL rechazar la operación y retornar código HTTP 403
5. WHEN un Librarian solicita un check-in de cualquier Libro prestado, THE API SHALL procesarlo independientemente de quién sea el prestatario
6. IF un Member solicita un check-out y ya tiene 5 Préstamos activos, THEN THE API SHALL rechazar la operación y retornar código HTTP 409 indicando que el Member ha alcanzado el límite máximo de préstamos simultáneos
7. WHEN un Member consulta su historial de préstamos, THE API SHALL retornar una lista paginada con un máximo de 20 registros por página de todos sus Préstamos ordenados por fecha de préstamo descendente
8. WHEN un Member consulta su historial de préstamos proporcionando un número de página, THE API SHALL retornar la página solicitada junto con el total de registros y el total de páginas disponibles

### Requisito 3: Búsqueda de Libros

**Historia de Usuario:** Como Member, quiero buscar libros por título, autor u otros campos, para encontrar rápidamente los recursos que necesito.

#### Criterios de Aceptación

1. WHEN un usuario autenticado envía una consulta de búsqueda con al menos 1 carácter y máximo 200 caracteres, THE API SHALL retornar una lista paginada de Libros que coincidan con el criterio en título, autor, ISBN o categoría, con un tamaño de página por defecto de 20 resultados y un máximo configurable de 100 resultados por página
2. WHEN un usuario autenticado envía una consulta de búsqueda vacía, THE API SHALL retornar la lista completa de Libros paginada con el mismo tamaño de página por defecto de 20 resultados
3. THE API SHALL soportar filtrado por categoría, estado (Available, CheckedOut) y rango de año de publicación (año mínimo y año máximo entre 1000 y el año actual) como parámetros opcionales combinables con la consulta de búsqueda
4. THE API SHALL retornar los resultados de búsqueda en un tiempo menor a 500ms para un catálogo de hasta 10,000 libros
5. WHEN un usuario no autenticado intenta realizar una búsqueda, THE API SHALL retornar código HTTP 401
6. WHEN un usuario autenticado envía una consulta de búsqueda y no existen coincidencias, THE API SHALL retornar una lista vacía con metadatos de paginación indicando total de 0 resultados y código HTTP 200
7. IF un usuario autenticado envía un valor de página menor a 1 o un tamaño de página fuera del rango 1-100, THEN THE API SHALL retornar código HTTP 400 con un mensaje de error indicando los valores válidos de paginación
8. IF un usuario autenticado envía un filtro de categoría o estado con un valor no reconocido por el sistema, THEN THE API SHALL retornar código HTTP 400 con un mensaje de error indicando los valores permitidos

### Requisito 4: Búsqueda Semántica con IA

**Historia de Usuario:** Como Member, quiero realizar búsquedas en lenguaje natural sobre el catálogo, para encontrar libros relevantes incluso cuando no conozco los términos exactos.

#### Criterios de Aceptación

1. WHEN un usuario autenticado envía una consulta de Búsqueda_Semántica en lenguaje natural con una longitud entre 1 y 500 caracteres, THE API SHALL utilizar OpenAI API para calcular el embedding de la consulta, compararlo mediante similitud coseno contra los embeddings almacenados de los Libros, y retornar un máximo de 20 Libros ordenados de mayor a menor puntuación de relevancia dentro de un tiempo de respuesta máximo de 5 segundos
2. WHEN un Libro es creado o actualizado, THE API SHALL generar el embedding vectorial del Libro utilizando los campos título, autor y descripción, y almacenarlo en formato binario antes de confirmar la operación de creación o actualización al cliente
3. IF la API de OpenAI no está disponible o no responde dentro de 3 segundos, THEN THE API SHALL realizar un fallback a la búsqueda textual estándar (coincidencia parcial en título, autor y descripción), retornar los resultados con un campo booleano indicando que se utilizó búsqueda básica, y preservar el mismo formato de respuesta paginada
4. IF la consulta de Búsqueda_Semántica excede 500 caracteres, THEN THE API SHALL truncar la consulta a 500 caracteres antes de procesarla y continuar con el flujo normal de búsqueda sin notificar al usuario del truncamiento
5. THE API SHALL retornar los resultados de Búsqueda_Semántica con una puntuación de relevancia numérica entre 0.0 y 1.0 para cada Libro, donde 1.0 indica coincidencia perfecta y 0.0 indica ninguna relevancia
6. IF la consulta de Búsqueda_Semántica está vacía o contiene únicamente espacios en blanco, THEN THE API SHALL rechazar la solicitud con un error de validación indicando que la consulta no puede estar vacía
7. WHEN los resultados de Búsqueda_Semántica son calculados, THE API SHALL excluir del resultado aquellos Libros cuya puntuación de relevancia sea inferior a 0.3
8. IF la generación de embedding falla durante la creación o actualización de un Libro, THEN THE API SHALL completar la operación de creación o actualización del Libro sin el embedding y registrar el fallo para reprocesamiento posterior

### Requisito 5: Recomendaciones con IA

**Historia de Usuario:** Como Member, quiero recibir recomendaciones personalizadas de libros, para descubrir nuevos títulos que se ajusten a mis intereses.

#### Criterios de Aceptación

1. WHEN un Member autenticado solicita recomendaciones, THE Motor_Recomendaciones SHALL analizar el historial de préstamos del Member y retornar una lista de entre 1 y 10 Libros recomendados, donde cada recomendación incluye el título, autor, categoría y una justificación textual de máximo 200 caracteres indicando por qué se recomienda
2. WHEN un Member con menos de 3 préstamos en su historial solicita recomendaciones, THE Motor_Recomendaciones SHALL retornar los 10 Libros con mayor número de préstamos en los últimos 90 días que estén disponibles en el catálogo
3. IF la API de OpenAI no está disponible o no responde en un plazo de 10 segundos al solicitar recomendaciones, THEN THE Motor_Recomendaciones SHALL retornar hasta 10 Libros disponibles pertenecientes a las 3 categorías más frecuentes en el historial del Member, ordenados por número de préstamos totales
4. IF un Member sin historial de préstamos solicita recomendaciones y la API de OpenAI no está disponible, THEN THE Motor_Recomendaciones SHALL retornar los 10 Libros con mayor número de préstamos en los últimos 90 días
5. THE Motor_Recomendaciones SHALL excluir de las recomendaciones los Libros que el Member ya ha leído (préstamo devuelto) o tiene actualmente en préstamo activo
6. THE API SHALL cachear las recomendaciones por Member individual durante un período de 1 hora, e invalidar la caché de ese Member cuando registre un nuevo préstamo o una devolución
7. WHEN el Motor_Recomendaciones genera recomendaciones exitosamente, THE API SHALL responder en un tiempo máximo de 15 segundos incluyendo la llamada a OpenAI API, o de 3 segundos cuando se sirven desde caché

### Requisito 6: Autenticación y Autorización

**Historia de Usuario:** Como usuario, quiero iniciar sesión de forma segura mediante SSO con Google o Microsoft, para acceder al sistema sin crear credenciales adicionales.

#### Criterios de Aceptación

1. WHEN un usuario inicia sesión mediante Google OAuth 2.0, THE Sistema SHALL autenticar al usuario y crear una sesión válida con un JWT token
2. WHEN un usuario inicia sesión mediante Microsoft OAuth 2.0, THE Sistema SHALL autenticar al usuario y crear una sesión válida con un JWT token
3. WHEN un usuario nuevo se autentica por primera vez mediante SSO, THE Sistema SHALL crear una cuenta con rol Member por defecto
4. THE Sistema SHALL emitir JWT tokens con un tiempo de expiración de 60 minutos y soportar refresh tokens con expiración de 7 días
5. WHEN un token JWT expira y el usuario tiene un refresh token válido, THE API SHALL emitir un nuevo JWT token sin requerir re-autenticación
6. IF un usuario intenta acceder a un recurso sin permisos suficientes para su rol, THEN THE API SHALL retornar código HTTP 403 con un mensaje indicando el permiso requerido
7. WHEN un Admin asigna un nuevo rol a un usuario, THE Sistema SHALL actualizar los permisos del usuario de forma inmediata en la siguiente solicitud

### Requisito 7: Gestión de Usuarios y Roles

**Historia de Usuario:** Como Admin, quiero gestionar usuarios y asignar roles, para controlar el acceso al sistema según las responsabilidades de cada persona.

#### Criterios de Aceptación

1. WHEN un Admin solicita la lista de usuarios, THE API SHALL retornar una lista paginada de todos los usuarios registrados con su rol actual
2. WHEN un Admin asigna un rol (Admin, Librarian, Member) a un usuario existente, THE API SHALL actualizar el rol y retornar código HTTP 200
3. IF un Admin intenta asignarse a sí mismo un rol diferente y es el único Admin en el sistema, THEN THE API SHALL rechazar la operación para evitar la pérdida de acceso administrativo
4. THE Sistema SHALL mantener los siguientes permisos por rol: Admin (acceso completo, gestión de usuarios), Librarian (CRUD de libros, gestión de préstamos, reportes), Member (búsqueda, préstamos propios, historial propio)
5. WHEN un Librarian o Member intenta acceder a endpoints de gestión de usuarios, THE API SHALL retornar código HTTP 403

### Requisito 8: Dashboard y Estadísticas

**Historia de Usuario:** Como Librarian, quiero ver un panel con estadísticas de la biblioteca, para monitorear el uso del sistema y tomar decisiones informadas.

#### Criterios de Aceptación

1. WHEN un Librarian o Admin solicita las estadísticas del Dashboard, THE API SHALL retornar: total de libros, libros disponibles, libros prestados, total de préstamos activos, total de usuarios por rol
2. WHEN un Librarian o Admin solicita estadísticas de préstamos, THE API SHALL retornar métricas de préstamos por período (últimos 7 días, 30 días, 12 meses)
3. THE API SHALL retornar las categorías más populares basadas en número de préstamos
4. THE API SHALL retornar los libros más prestados como un ranking de los 10 títulos con mayor número de préstamos
5. IF un Member intenta acceder a los endpoints de Dashboard, THEN THE API SHALL retornar código HTTP 403

### Requisito 9: Interfaz de Usuario Responsiva

**Historia de Usuario:** Como usuario, quiero acceder al sistema desde cualquier dispositivo, para gestionar la biblioteca cómodamente desde escritorio, tablet o móvil.

#### Criterios de Aceptación

1. THE Frontend SHALL renderizar correctamente en viewports de 320px (móvil), 768px (tablet) y 1024px+ (escritorio)
2. THE Frontend SHALL utilizar Material UI como biblioteca de componentes para mantener consistencia visual
3. THE Frontend SHALL implementar navegación adaptativa: drawer lateral en escritorio y bottom navigation o hamburger menu en móvil
4. THE Frontend SHALL mostrar feedback visual (loading states, toasts de éxito/error) para todas las operaciones asíncronas en un tiempo menor a 100ms tras la interacción del usuario
5. THE Frontend SHALL cumplir con WCAG 2.1 nivel AA en contraste de colores y navegación por teclado

### Requisito 10: Despliegue y CI/CD

**Historia de Usuario:** Como desarrollador, quiero que la aplicación se despliegue automáticamente mediante contenedores, para tener un entorno reproducible y un pipeline de entrega continua.

#### Criterios de Aceptación

1. THE Sistema SHALL proveerse como imágenes Docker para API y Frontend publicadas en ghcr.io
2. WHEN un push se realiza a la rama main, THE Sistema SHALL ejecutar el pipeline de CI (build, tests, linting) y, si pasa, ejecutar el pipeline de CD para publicar las imágenes
3. THE Sistema SHALL proveer un archivo docker-compose.yml que levante el entorno completo (API, Frontend, SQL Server) con un solo comando
4. WHEN el pipeline de CI detecta fallos en tests o linting, THE Sistema SHALL bloquear el merge del PR y reportar los errores
5. THE Sistema SHALL proveer un script de seed que cargue datos de ejemplo en la base de datos para demostración

### Requisito 11: Persistencia de Datos

**Historia de Usuario:** Como desarrollador, quiero que los datos se almacenen de forma estructurada y confiable, para garantizar la integridad del sistema.

#### Criterios de Aceptación

1. THE API SHALL utilizar Entity Framework Core 8 con SQL Server 2022 como motor de base de datos
2. THE API SHALL gestionar los cambios de esquema exclusivamente mediante EF Core Migrations
3. THE API SHALL implementar soft-delete para Libros y usuarios, marcando registros como eliminados sin borrado físico
4. THE API SHALL validar la unicidad del ISBN al crear o actualizar un Libro
5. WHEN dos solicitudes concurrentes intentan realizar check-out del mismo Libro, THE API SHALL procesar solo la primera y rechazar la segunda con código HTTP 409

### Requisito 12: Validación y Manejo de Errores

**Historia de Usuario:** Como usuario, quiero recibir mensajes de error claros y específicos cuando ocurra un problema, para entender qué corregir.

#### Criterios de Aceptación

1. WHEN la API recibe una solicitud con datos inválidos, THE API SHALL retornar código HTTP 422 con un objeto que contenga el campo con error y un mensaje descriptivo en formato ProblemDetails (RFC 7807)
2. IF ocurre un error interno no manejado, THEN THE API SHALL retornar código HTTP 500 con un identificador de correlación y registrar el error completo en los logs
3. THE API SHALL validar todos los campos de entrada utilizando FluentValidation antes de ejecutar la lógica de negocio
4. THE Frontend SHALL mostrar los mensajes de error de validación junto al campo correspondiente en el formulario
5. THE API SHALL retornar respuestas de error consistentes usando el formato ProblemDetails (RFC 7807) para todos los códigos de error HTTP

### Requisito 13: Paginación y Rendimiento de API

**Historia de Usuario:** Como usuario, quiero que las listas de datos se carguen de forma eficiente, para tener una experiencia fluida incluso con grandes volúmenes de información.

#### Criterios de Aceptación

1. THE API SHALL soportar paginación basada en offset (parámetros page y pageSize) para todos los endpoints que retornan listas
2. THE API SHALL limitar el tamaño máximo de página a 100 elementos y usar un valor por defecto de 20
3. THE API SHALL incluir metadatos de paginación en la respuesta: totalCount, pageSize, currentPage, totalPages, hasNext, hasPrevious
4. WHEN un cliente solicita una página fuera de rango, THE API SHALL retornar una lista vacía con los metadatos de paginación correctos
5. THE API SHALL implementar ordenamiento configurable (ascendente/descendente) por campos relevantes en cada recurso

### Requisito 14: Serialización y Formato de Respuestas API

**Historia de Usuario:** Como desarrollador frontend, quiero que la API retorne datos en un formato JSON consistente y predecible, para facilitar la integración.

#### Criterios de Aceptación

1. THE API SHALL serializar todas las respuestas en formato JSON utilizando camelCase para nombres de propiedades
2. THE API SHALL envolver las respuestas exitosas de listas en un objeto con propiedades data (array de elementos) y pagination (metadatos de paginación)
3. THE API SHALL serializar fechas en formato ISO 8601 (UTC) en todas las respuestas
4. FOR ALL objetos de respuesta válidos, serializar y deserializar el objeto SHALL producir un objeto equivalente al original (propiedad round-trip)
5. THE API SHALL incluir un header X-Correlation-Id en todas las respuestas para facilitar el rastreo de solicitudes

### Requisito 15: Experiencia de Usuario y Rendimiento Frontend

**Historia de Usuario:** Como usuario, quiero que la interfaz sea intuitiva, visualmente atractiva y responda de forma instantánea, para tener una experiencia agradable y productiva sin fricciones.

#### Criterios de Aceptación

1. THE Frontend SHALL alcanzar un First Contentful Paint (FCP) menor a 1.5 segundos y un Time to Interactive (TTI) menor a 3 segundos medidos con Lighthouse en condiciones de red 4G simulada
2. THE Frontend SHALL mostrar un skeleton loader en menos de 100ms tras cualquier interacción que dispare una carga de datos, y el contenido real debe reemplazar al skeleton en menos de 2 segundos bajo condiciones normales de red
3. THE Frontend SHALL implementar un design system consistente basado en Material UI con paleta de colores custom (primary: Indigo #1E3A5F, secondary: Amber #F59E0B), tipografía Inter/Roboto, y border-radius unificado de 8-12px
4. THE Frontend SHALL soportar modo claro y modo oscuro con toggle accesible, persistiendo la preferencia del usuario en localStorage y respetando prefers-color-scheme del sistema como valor por defecto
5. THE Frontend SHALL implementar code splitting por ruta usando React.lazy y Suspense, manteniendo el bundle inicial por debajo de 200KB gzipped
6. THE Frontend SHALL proporcionar empty states informativos con ilustración, mensaje descriptivo y call-to-action cuando no existan datos que mostrar en cualquier lista o sección
7. THE Frontend SHALL implementar optimistic updates para acciones frecuentes (check-out, check-in) mostrando el resultado esperado inmediatamente y revirtiendo con notificación si la API retorna error
8. THE Frontend SHALL mantener un Cumulative Layout Shift (CLS) menor a 0.1 en todas las páginas, reservando espacio para imágenes y contenido dinámico antes de su carga
9. THE Frontend SHALL pasar todas las pruebas de visual regression en los 5 viewports estándar (375x667, 390x844, 768x1024, 1440x900, 1920x1080) sin diferencias superiores al 0.1% respecto a los baselines aprobados
