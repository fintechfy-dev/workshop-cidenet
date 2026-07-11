# SPEC — Módulo de Administración de Usuarios, Roles y Permisos (CIDENET)

> Generado por `/discovery` a partir del caso aportado por Julián. Especificación técnica completa: historias, criterios de aceptación y reglas de negocio, validados durante la entrevista BDD 2.0 (Épicas → Historias → Criterios → Completitud → Gherkin).

## Épica

- **EPIC-001 · Administración de usuarios y accesos** — gestión de cuentas, roles y matriz de permisos como un mismo bloque. VUIFED 51/60, incluida en MVP.

## Decisiones de diseño

- **Un rol por usuario** (single-role, obligatorio).
- Estado inicial de una cuenta: **Activo** por defecto.
- **Eliminación lógica (soft-delete):** nada se borra físicamente por defecto; se conserva para auditoría.
- Alcance del MVP: `reports` queda como permiso en la matriz, sin pantalla propia. Autenticación mínima (login + bloqueo de inactivos, sin recuperación de contraseña ni MFA).

## Historias de usuario

| ID | Historia |
|----|----------|
| US-001 | Como Admin quiero crear una cuenta de usuario para dar acceso a nuevas personas. |
| US-002 | Como Admin quiero ver una tabla paginada con búsqueda y filtros para gestionar cuentas. |
| US-003 | Como Admin quiero editar los datos, rol y estado de un usuario. |
| US-004 | Como Admin quiero eliminar una cuenta con confirmación explícita. |
| US-005 | Como usuario quiero editar mi propio perfil sin cambiar mi rol ni mis permisos. |
| US-006 | Como Admin quiero configurar la matriz de permisos por rol. |
| US-007 | Como usuario quiero autenticarme; y como sistema, bloquear a los inactivos. |

**Historias de gap (fase Completitud):** US-001-SEC (credenciales), US-001-AUD (auditoría), US-001-MON (monitoreo), US-001-MNT (arranque/mantenimiento), US-001-TST (política de pruebas).

## Reglas de negocio

**Explícitas del caso (R1–R6):**
- R1 — No se puede eliminar el último usuario con rol Admin.
- R2 — Un usuario no puede modificar su propio rol.
- R3 — Un usuario no puede asignarse permisos a sí mismo.
- R4 — El email debe ser único en todo el sistema.
- R5 — Todo usuario debe tener exactamente un rol asignado.
- R6 — Un usuario inactivo no puede autenticarse.

**Descubiertas en Completitud:**
- No se puede quitar el rol Admin al último Admin (extensión de R1).
- Un Admin no puede eliminar su propia cuenta.
- La contraseña se almacena con hash + salt (Argon2id/bcrypt); nunca en texto plano ni reversible.
- La contraseña nunca aparece en ninguna respuesta del backend.
- La autorización se valida en el backend en cada petición (no basta ocultar en la UI).
- La eliminación es lógica (soft-delete); el registro se conserva para auditoría.
- Al recrear el email de una cuenta eliminada, se reactiva la cuenta anterior (no se duplica).
- El email es único de forma absoluta y case-insensitive; se recortan espacios.
- Contraseña mínima: 8 caracteres con mayúscula, minúscula y número; requiere confirmación.
- Nombre 2–100 caracteres; email ≤254 (RFC 5321).
- Desactivar a un usuario corta su sesión activa de inmediato.
- Toda acción (incluidas lecturas) queda en un registro de auditoría inmutable.
- El sistema arranca con un Admin sembrado que debe cambiar su contraseña al primer acceso.
- Login con mensaje genérico (anti-enumeración de usuarios).
- No se puede dejar al rol Admin sin sus permisos de gestión (anti-lockout).

## Endpoints (esbozo)

| Método | Ruta | Descripción | Quién puede ejecutarlo |
|---|---|---|---|
| POST | /api/users | Crear cuenta | Admin |
| GET | /api/users | Listar/buscar/filtrar (paginado) | Admin, Editor (solo lectura) |
| GET | /api/users/{id} | Detalle de una cuenta | Admin |
| PUT | /api/users/{id} | Editar cuenta | Admin |
| DELETE | /api/users/{id} | Eliminar (lógico) | Admin |
| GET/PUT | /api/users/me | Ver/editar el propio perfil | Cualquier rol (self) |
| GET | /api/permissions | Ver la matriz de permisos | Admin |
| PUT | /api/permissions | Configurar la matriz | Admin |
| POST | /api/auth/login | Autenticar | Público (rechaza inactivos) |

## Pantallas

- **Tabla de usuarios** — lista paginada; búsqueda por nombre/email; filtros por rol y estado; acciones por fila; solo muestra cuentas no eliminadas.
- **Formulario crear/editar** — nombre, email, contraseña + confirmación (solo al crear), rol, estado; validación inline + resumen; Guardar deshabilitado hasta que sea válido.
- **Matriz de permisos** — rol × recurso × acción (CRUD) con toggles; anti-lockout de Admin.
- **Confirmación de eliminación** — modal con datos del usuario, advertencia si es Admin, confirmación explícita.
- **Login** — email + contraseña; bloqueo de inactivos; mensaje genérico ante credenciales inválidas.

## Criterios de aceptación (YAML)

Ver `specs/criterios/US-001.yaml` … `US-007.yaml` (categorías: escenarios_exito, validaciones_reglas, manejo_errores, requisitos_ux, casos_edge; todos con `validacion_smart` completa).

## Especificación ejecutable (Gherkin)

Ver `features/*.feature` — un archivo por historia + las historias de gap. Cada regla 🔥/⚡ tiene su escenario negativo (política de cobertura exhaustiva, US-001-TST). Estos escenarios alimentan el TDD (`/test` → `/iterate`).

## Cobertura (DQS-lite)

Ver `sessions/julian/dqs-lite.md`.
