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

## Iteración 6 — Eliminar usuario (US-004, soft-delete)

**Entregable:** `DELETE /api/users/{id}` con **borrado lógico**; **R1** (no el último Admin); no autoeliminación; idempotencia si ya fue eliminado.
**Done-when:** los escenarios de `features/US-004.feature` pasan (incluye bloqueo del último Admin, autoeliminación, soft-delete conserva el registro).

## Iteración 7 — Editar el propio perfil (US-005)

**Entregable:** `GET/PUT /api/users/me`; alcance **self**; rol/estado solo lectura (R2/R3); cambio de contraseña con confirmación y verificación de la actual; email único.
**Done-when:** los escenarios de `features/US-005.feature` pasan (incluye 403 al editar a otro, no expone contraseña).

## Iteración 8 — Matriz de permisos (US-006)

**Entregable:** `GET/PUT /api/permissions` (solo Admin); toggles por rol×recurso×acción; roles fijos; **R3** (no modificar el propio rol Admin); **anti-lockout** de Admin; el cambio rige la autorización efectiva.
**Done-when:** los escenarios de `features/US-006.feature` pasan (incluye R3, anti-lockout, efecto sobre autorización, Editor→403).

## Iteración 9 — Autenticación y bloqueo de inactivos (US-007)

**Entregable:** `POST /api/auth/login`; verificación contra hash; **R6** (inactivo no entra); mensaje genérico anti-enumeración; **invalidación inmediata de sesión** al desactivar (US-007-USR).
**Done-when:** los escenarios de `features/US-007.feature` pasan (incluye inactivo bloqueado, credenciales inválidas genéricas, sesión cortada al desactivar).

## Iteración 10 — Endurecimiento de seguridad transversal (US-001-SEC)

**Entregable:** consolidar la matriz de autorización backend (403 para peticiones directas no autorizadas en todos los endpoints) y garantizar que ningún DTO expone la contraseña.
**Done-when:** los escenarios de `features/US-001-SEC.feature` pasan (Scenario Outline de autorización + no exponer contraseña + hashing).

## Iteración 11 — Registro de auditoría (US-001-AUD)

**Entregable:** audit trail que registra toda acción (actor, acción, entidad, fecha), append-only/inmutable, que sobrevive al soft-delete.
**Done-when:** los escenarios de `features/US-001-AUD.feature` pasan (registro por acción, inmutabilidad, persistencia tras soft-delete).

## Iteración 12 — Observabilidad y alertas (US-001-MON)

**Entregable:** logging estructurado/métricas para los eventos definidos (fuerza bruta, pocos Admin, errores de backend) con umbrales configurables.
**Done-when:** los escenarios de `features/US-001-MON.feature` pasan (los tres eventos son observables/alertables).

---

## Iteración 13 — Frontend: tabla de usuarios (pantalla central)

**Entregable:** componente de tabla conectado a `GET /api/users` (paginación, búsqueda, filtros, acciones por fila); estados de carga/vacío/error.
**Done-when:** tests de componente (Vitest) de la tabla en verde contra el API real.

## Iteración 14 — Frontend: formulario crear/editar

**Entregable:** formulario con validación inline por campo + mensaje resumen + botón Guardar deshabilitado hasta que sea válido; confirmación de contraseña; conectado a crear/editar.
**Done-when:** tests de componente del formulario en verde (incluye estados inválidos y Guardar deshabilitado).

## Iteración 15 — Frontend: matriz de permisos

**Entregable:** vista de matriz rol×recurso×acción con toggles conectada a `GET/PUT /api/permissions`; refleja anti-lockout y R3.
**Done-when:** tests de componente de la matriz en verde.

## Iteración 16 — Frontend: modal de eliminación + login

**Entregable:** modal de confirmación de eliminación (datos del usuario, advertencia si es Admin) + pantalla de login (bloqueo de inactivos, mensaje genérico).
**Done-when:** tests de componente del modal y del login en verde.

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
