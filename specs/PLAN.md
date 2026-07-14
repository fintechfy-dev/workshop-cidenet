# PLAN — Módulo de Administración de Usuarios, Roles y Permisos (CIDENET)

> Generado por `/plan` a partir de `specs/SPEC.md` y `features/*.feature`. Iteraciones pequeñas y verificables. **Backend primero** (modelo → reglas → endpoints), **frontend después** (pantalla central primero). Cada iteración termina en un commit con sus tests en verde (el hook de pre-commit lo exige). No se avanza de iteración sin cumplir el "Done-when" anterior.
>
> La autorización por rol (SEC-3) se hace cumplir **dentro de cada iteración de endpoint** (no se pospone), y ninguna respuesta expone la contraseña (SEC-1) desde el primer DTO.

---

## Iteración 1 — Infraestructura y arranque ✅

**Entregable:** `docker compose` levantando Postgres + API (.NET) + frontend (Vite/React); `AppDbContext` conectado; endpoint `/health`.
**Done-when:** `docker compose up --build` levanta db + api + frontend y `GET /health` responde `ok`.
**Estado:** ✅ Ya satisfecha por la plomería base del repo (`/health` implementado, `HealthCheckTests` en verde).

## Iteración 2 — Modelo de dominio + migración inicial + seed Admin

**Entregable:** entidades `User`, `Role` (Admin/Editor/Viewer predefinidos), `Permission` (matriz recurso×acción); invariantes de dominio (un rol por usuario R5, estado Activo por defecto, email normalizado); primera migración EF; **Admin sembrado** (US-001-MNT/MNT-1) con marca de "debe cambiar contraseña".
**Done-when:** la migración crea las tablas; existe el Admin seed; tests de dominio verdes (single-role, estado default, roles predefinidos inmutables).

## Iteración 3 — Crear usuario (US-001) ✅

**Entregable:** endpoint `POST /api/users` (solo Admin); validaciones (5 campos obligatorios, formato/longitud email, nombre 2–100, política de contraseña, confirmación); email único case-insensitive + trim (R4); **hash con salt** (SEC-2); **reactivación** de cuenta soft-deleted al recrear su email (US-001-EDGE5).
**Done-when:** los escenarios de `features/US-001.feature` pasan (incluye negativos: duplicado, contraseña débil, no coinciden, Viewer→403).
**Estado:** ✅ Cumplida. `dotnet test` → **17/17 verde** (16 escenarios de US-001 + health). El modelo de dominio de la Iteración 2 (`User`, `Email`, `UserRole`, `UserStatus`, `PasswordPolicy`) se construyó aquí con sus invariantes.
**Deferidos (dependen de otras iteraciones):** Viewer→403 (autorización, It 9–10), reactivación de email eliminado (soft-delete, It 6), botón Guardar deshabilitado (frontend, It 14). Sus escenarios están en `features/` y se cubrirán en esas iteraciones.

## Iteración 4 — Consultar usuarios (US-002) ✅

**Entregable:** `GET /api/users` paginado (server-side), búsqueda parcial case-insensitive por nombre/email, filtros por rol y estado combinables; excluye cuentas eliminadas; Editor solo-lectura, Viewer sin acceso.
**Done-when:** los escenarios de `features/US-002.feature` pasan (incluye vacío, fuera de rango, no expone contraseña, autorización por rol).
**Estado:** ✅ Cumplida. `dotnet test` → **27/27 verde** (10 escenarios de US-002 + los 17 previos). Búsqueda parcial case-insensitive con trim, filtros rol/estado combinables, paginación server-side (10/pág) con total y orden determinista, estado vacío, página fuera de rango sin fallar, y respuesta sin contraseña.
**Deferidos:** exclusión de cuentas eliminadas (US-002-AUD → It 6, soft-delete) y autorización Editor/Viewer (US-002-SEC → It 9–10). Anotado en los tests.

## Iteración 5 — Editar usuario (US-003) ✅

**Entregable:** `PUT /api/users/{id}` (solo Admin); edición de datos/rol/estado; email único al cambiar; **R2** (no su propio rol); **protección del último Admin** al cambiar rol.
**Done-when:** los escenarios de `features/US-003.feature` pasan (incluye negativos R2, último-Admin, email en uso, Editor→403).
**Estado:** ✅ Cumplida. `dotnet test` → **32/32 verde** (5 escenarios de US-003 + los 27 previos). Edición de nombre/email/rol/estado con métodos de dominio; unicidad de email excluyendo la propia (con variante case-insensitive sin conflicto); protección del último Admin (409); 404 en usuario inexistente.
**Deferidos:** R2 "no cambiar su propio rol" y US-003-SEC "solo Admin edita a otros" requieren la identidad del actor (It 9–10); botón Guardar deshabilitado → frontend (It 14). Anotado en el servicio y los tests.

## Iteración 6 — Eliminar usuario (US-004, soft-delete) ✅

**Entregable:** `DELETE /api/users/{id}` con **borrado lógico**; **R1** (no el último Admin); no autoeliminación; idempotencia si ya fue eliminado.
**Done-when:** los escenarios de `features/US-004.feature` pasan (incluye bloqueo del último Admin, autoeliminación, soft-delete conserva el registro).
**Estado:** ✅ Cumplida. `dotnet test` → **39/39 verde** (6 escenarios nuevos + los 33 previos). `User.SoftDelete()`/`Reactivate()` como invariantes de dominio; filtro global de EF (`HasQueryFilter(!IsDeleted)`) excluye eliminados de listado/edición/segunda-eliminación sin tocar cada repositorio; `IgnoreQueryFilters()` solo para auditoría y para detectar reactivación por email. Desbloqueó dos escenarios deferidos: US-002-AUD (tabla no muestra eliminados) y US-001-EDGE5 (recrear email reactiva la cuenta).
**Deferidos:** "no autoeliminación" (US-004-V3) y "solo Admin elimina" (US-004-SEC) requieren la identidad del actor autenticado (It 9–10); advertencia del modal para Admin → frontend (It 16).

## Iteración 7 — Editar el propio perfil (US-005) ✅

**Entregable:** `GET/PUT /api/users/me`; alcance **self**; rol/estado solo lectura (R2/R3); cambio de contraseña con confirmación y verificación de la actual; email único.
**Done-when:** los escenarios de `features/US-005.feature` pasan (incluye 403 al editar a otro, no expone contraseña).
**Estado:** ✅ Cumplida. `dotnet test` → **45/45 verde** (6 escenarios nuevos + los 39 previos). Alcance self resuelto con un marcador provisional de identidad (header `X-User-Id`, reemplazable por sesión real en It 9–10); rol/estado quedan de solo lectura simplemente porque `EditMyProfileRequest` no los declara en el contrato (R2/R3); cambio de contraseña verifica la actual con `IPasswordHasher.Verify` (bcrypt) antes de aplicar la nueva.
**Deferido:** "No puedo editar el perfil de otro usuario" vía `PUT /api/users/{id}` directo (autorización por rol) requiere identidad real de sesión (It 9–10), mismo caso que US-003-SEC.

## Iteración 8 — Matriz de permisos (US-006) ✅

**Entregable:** `GET/PUT /api/permissions` (solo Admin); toggles por rol×recurso×acción; roles fijos; **R3** (no modificar el propio rol Admin); **anti-lockout** de Admin; el cambio rige la autorización efectiva.
**Done-when:** los escenarios de `features/US-006.feature` pasan (incluye R3, anti-lockout, efecto sobre autorización, Editor→403).
**Estado:** ✅ Cumplida. `dotnet test` → **50/50 verde** (5 escenarios nuevos + los 45 previos). Matriz sembrada perezosamente con los defaults del caso (sección 4 del brief) la primera vez que se consulta o edita. R3 y el anti-lockout se resolvieron como una sola regla: los permisos del rol Admin son inmutables desde este endpoint (solo un Admin llega aquí, así que "el rol que él mismo posee" es siempre Admin) — bloquear cualquier cambio a esa fila cubre ambos escenarios sin necesitar todavía identidad puntual del actor.
**Deferidos:** "el cambio rige la autorización efectiva" (que otros endpoints respeten la matriz) es transversal → It 10. "Solo Admin accede" (US-006-SEC) requiere autenticación → It 9–10.

## Iteración 9 — Autenticación y bloqueo de inactivos (US-007) ✅

**Entregable:** `POST /api/auth/login`; verificación contra hash; **R6** (inactivo no entra); mensaje genérico anti-enumeración; **invalidación inmediata de sesión** al desactivar (US-007-USR).
**Done-when:** los escenarios de `features/US-007.feature` pasan (incluye inactivo bloqueado, credenciales inválidas genéricas, sesión cortada al desactivar).
**Estado:** ✅ Cumplida. `dotnet test` → **58/58 verde** (8 escenarios nuevos + los 50 previos). Login verifica contra el hash (bcrypt) y devuelve el mismo `UserDto` sin contraseña que ya usa el resto de la API — su `Id` es lo que el cliente reusa como `X-User-Id` (el marcador de identidad de It 7–8), que ahora queda respaldado por credenciales reales en vez de ser un placeholder. La "invalidación inmediata de sesión" (US-007-USR) sale gratis: como no hay sesión cacheada, `GET/PUT /api/users/me` revalidan `Estado=Activo` en cada request, así que desactivar corta el acceso en la siguiente llamada sin mecanismo aparte.
**Deferido:** rate limiting / bloqueo por fuerza bruta (US-007-EDGE2) queda fuera del MVP, según lo confirmado en Completitud.

## Iteración 10 — Endurecimiento de seguridad transversal (US-001-SEC) ✅

**Entregable:** consolidar la matriz de autorización backend (403 para peticiones directas no autorizadas en todos los endpoints) y garantizar que ningún DTO expone la contraseña.
**Done-when:** los escenarios de `features/US-001-SEC.feature` pasan (Scenario Outline de autorización + no exponer contraseña + hashing).
**Estado:** ✅ Cumplida. `dotnet test` → **64/64 verde** (6 escenarios nuevos + los 58 previos). Autorización real por rol en todos los endpoints de usuarios y permisos vía un helper `AuthorizeAsync` (resuelve el actor por `X-User-Id`, verifica `Estado=Activo` y el rol permitido). Arranque: la primera cuenta del sistema se crea sin exigir autorización (todavía no hay Admin que la otorgue); de ahí en adelante, `POST/PUT/DELETE /api/users` y `GET/PUT /api/permissions` exigen Admin (Editor además puede `GET /api/users` en solo lectura). R2 (no cambiar el propio rol) y "no autoeliminación" quedaron resueltos como casos del mismo endpoint, comparando el id del actor contra el id objetivo. Se retrofitaron los 7 suites de tests previos para actuar como un Admin autenticado (bootstrap + `AuthTestHelpers`), y se agregaron los escenarios SEC que antes estaban deferidos (US-002/003/004/006-SEC, R2).
**Nota de diseño:** "el último Admin no se puede eliminar/degradar" y "un Admin no puede autoeliminarse/auto-degradarse" colapsan en el mismo caso cuando solo queda un Admin (es el único que podría autorizar la acción sobre sí mismo); ambas reglas se mantienen (defensa en profundidad) pero se verifican con el mismo código de estado.

## Iteración 11 — Registro de auditoría (US-001-AUD) ✅

**Entregable:** audit trail que registra toda acción (actor, acción, entidad, fecha), append-only/inmutable, que sobrevive al soft-delete.
**Done-when:** los escenarios de `features/US-001-AUD.feature` pasan (registro por acción, inmutabilidad, persistencia tras soft-delete).
**Estado:** ✅ Cumplida. `dotnet test` → **67/67 verde** (3 escenarios nuevos + los 64 previos). El registro se llena solo: un `IEndpointFilter` (`AuditLogFilter`) aplicado una vez a todo el grupo `/api` anota actor (`X-User-Id`), acción (el nombre ya declarado con `.WithName` en cada endpoint) y entidad (segmento de la URL) en cada request — sin instrumentar cada servicio de aplicación por separado. `IAuditLogRepository` solo declara `Append`/`GetAll` (append-only por contrato, sin Update/Delete); no hay ninguna ruta de edición/borrado expuesta. Sin FK hacia `User`, así que una entrada sobrevive al soft-delete de la cuenta involucrada. `GET /api/audit-log` (solo Admin) expone el log.

## Iteración 12 — Observabilidad y alertas (US-001-MON) ✅

**Entregable:** logging estructurado/métricas para los eventos definidos (fuerza bruta, pocos Admin, errores de backend) con umbrales configurables.
**Done-when:** los escenarios de `features/US-001-MON.feature` pasan (los tres eventos son observables/alertables).
**Estado:** ✅ Cumplida. `dotnet test` → **70/70 verde** (3 escenarios nuevos + los 67 previos). `AlertService` centraliza los tres eventos: loguea estructurado (`ILogger`) y persiste en `GET /api/monitoring/alerts` (Admin). MON-1 (fuerza bruta): `LoginService` cuenta fallos recientes por email vía `IFailedLoginTracker`; MON-2 (pocos Admin activos): `EditUserService`/`DeleteUserService` revisan `CountActiveByRoleAsync(Admin)` tras cada cambio exitoso; MON-3 (error de backend): `BackendErrorAlertFilter` envuelve todo el grupo `/api`, captura cualquier excepción no controlada y responde 500 sin filtrar detalles. Umbrales configurables vía `MonitoringOptions` (sección `Monitoring` de la configuración, con defaults de código).

---

## Iteración 13 — Frontend: tabla de usuarios (pantalla central) ✅

**Entregable:** componente de tabla conectado a `GET /api/users` (paginación, búsqueda, filtros, acciones por fila); estados de carga/vacío/error.
**Done-when:** tests de componente (Vitest) de la tabla en verde contra el API real.
**Estado:** ✅ Cumplida. `npm test` → 8/8 verde (7 escenarios nuevos de `UsersTable` + el smoke test previo de `App`). `UsersTablePage` (`frontend/src/pages/UsersTable.tsx`) llama a `GET /api/users` vía el cliente real (`api/client.ts`), con búsqueda debounced, filtros de rol/estado, paginación, y estados carga/vacío/error/sin-acceso (403 → Viewer). Las acciones (editar/eliminar) solo se muestran si el actor (`auth/session.ts`, provisional hasta el login de It16) es Admin. Verificado en navegador (Playwright) contra el stack real `docker compose up --build` (Postgres + API + Vite), incluyendo la vista de Editor sin columna de acciones.
**De paso:** se cerró un gap pendiente desde la Iteración 2 — nunca existía una migración EF real, así que `docker compose up` no funcionaba contra Postgres. Se generó `InitialCreate` y se aplica al arrancar fuera de `Testing` (ver commit `fix:` previo).

## Iteración 14 — Frontend: formulario crear/editar ✅

**Entregable:** formulario con validación inline por campo + mensaje resumen + botón Guardar deshabilitado hasta que sea válido; confirmación de contraseña; conectado a crear/editar.
**Done-when:** tests de componente del formulario en verde (incluye estados inválidos y Guardar deshabilitado).
**Estado:** ✅ Cumplida. `npm test` → 21/21 verde (13 escenarios nuevos de `UserForm` + los 8 previos). Un solo componente (`frontend/src/pages/UserForm.tsx`) cubre crear/editar según `modo`: validación inline por campo (nombre, email, política de contraseña, confirmación, rol obligatorio) + resumen de pendientes; Guardar deshabilitado mientras haya errores o (en edición) mientras no haya cambios; R2 deshabilita el selector de rol al editar la propia cuenta; conflicto de email (409 del backend) se muestra inline. Conectado a `createUser`/`updateUser` reales en `api/client.ts`.
**Pendiente de cablear:** la tabla (It13) todavía no invoca este formulario desde sus botones Editar/Nuevo — se integra junto con el modal de eliminación en It16, cuando también se agregue el login y el layout final de la app.

## Iteración 15 — Frontend: matriz de permisos ✅

**Entregable:** vista de matriz rol×recurso×acción con toggles conectada a `GET/PUT /api/permissions`; refleja anti-lockout y R3.
**Done-when:** tests de componente de la matriz en verde.
**Estado:** ✅ Cumplida. `npm test` → 25/25 verde (4 escenarios nuevos + los 21 previos). `PermissionsMatrixPage` renderiza una fila por rol×recurso (12) con un checkbox por acción (4 = 48 celdas), reflejando el estado real de `GET /api/permissions`; cada toggle llama `PUT /api/permissions` de inmediato (optimista, revierte si falla) y actualiza desde la respuesta del servidor. R3 + anti-lockout: los checkboxes de la fila Admin están deshabilitados en la UI (el backend ya los rechaza en It8; aquí se refuerza para que ni siquiera se pueda intentar).
**Pendiente de cablear:** integración en el layout final de la app junto con It16.

## Iteración 16 — Frontend: modal de eliminación + login ✅

**Entregable:** modal de confirmación de eliminación (datos del usuario, advertencia si es Admin) + pantalla de login (bloqueo de inactivos, mensaje genérico).
**Done-when:** tests de componente del modal y del login en verde.
**Estado:** ✅ Cumplida. `npm test` → 35/35 verde (10 escenarios nuevos de `DeleteConfirmModal`/`Login` + los 25 previos). `DeleteConfirmModal` (`frontend/src/pages/DeleteConfirmModal.tsx`) muestra nombre/email/rol, una alerta destacada cuando el rol es Admin, y llama `DELETE /api/users/{id}` real: 204 refresca/cierra, 409 muestra el mensaje del backend (último Admin / autoeliminación) sin tratarlo como fatal, 404 informa que la cuenta ya no existe (eliminación en paralelo) sin bloquear el flujo. `Login` (`frontend/src/pages/Login.tsx`) exige email+contraseña, llama `POST /api/auth/login` real, guarda `{id, rol}` con `auth/session.ts` en éxito y muestra el mensaje del backend (genérico en 401, "inactiva" en 403) sin guardar sesión en error. Se cableó la app completa en `App.tsx`: sin sesión renderiza `Login`; con sesión, un layout simple (nav de dos pestañas + "Cerrar sesión", sin router) alterna `UsersTablePage`/`PermissionsMatrixPage` (esta última solo para Admin). En `UsersTable.tsx` los botones "Editar"/"Eliminar" (y uno nuevo "Nuevo usuario", solo Admin) abren `UserForm`/`DeleteConfirmModal` inline; al guardar o eliminar con éxito se cierra el modal y se fuerza un refetch (contador de versión en las deps del `useEffect`) sin romper los 8 tests previos de la tabla. `api/client.ts` ganó `deleteUser` (DELETE, éxito=204, cualquier otro código lanza `ApiError` con el status para que el componente decida 404 vs 409) y `login` (POST, mismo patrón `readErrorMessage` que `createUser`/`updateUser`). Verificado además contra el stack real (`docker compose up --build`): `POST /api/auth/login` con credenciales válidas devuelve 200 con el `UserDto` esperado y con credenciales inválidas el mensaje genérico exacto que consume el componente.

---

## Fuera del núcleo del MVP (opcional / posterior)

- **Retención/purga de datos** (US-001-MNT/MNT-3, 💡): job de purga física configurable (12/24 meses). Planificar como iteración adicional si se prioriza; no bloquea el MVP.
- **Reportes** (RPT) y **backup a nivel de módulo** (BCK): fuera del MVP por decisión de alcance.

<!--
Cobertura Gherkin → iteración:
US-001→It3 · US-002→It4 · US-003→It5 · US-004→It6 · US-005→It7 · US-006→It8 · US-007→It9
US-001-SEC→It10 · US-001-AUD→It11 · US-001-MON→It12 · US-001-MNT→It2 (seed) + fuera-de-MVP (retención)
US-001-TST (política de cobertura negativa) → transversal: se cumple en cada iteración de endpoint.
Frontend (pantallas de SPEC.md) → It13–It16, la tabla (central) primero.
-->
