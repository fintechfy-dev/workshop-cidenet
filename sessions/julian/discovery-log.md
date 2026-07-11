# Bitácora de discovery — Julián

## 🎯 Fase 1 — Épicas (completada)

- **EPIC-001 · Administración de usuarios y accesos** — épica única que engloba cuentas, roles y matriz de permisos (Julián optó por no partirla).
- VUIFED: Valor 10 · Usuarios 10 · Impacto 7 · Factibilidad 7 · Esfuerzo 7 · Dependencias 10 → **total 51 / promedio 8.5**.
- `mvp_included: true` — es el corazón del módulo.
- Artefacto: `specs/epicas/EPIC-001.md`.

## 📝 Fase 2 — Historias (completada)

- **Decisión de diseño clave:** 1 rol por usuario (single-role, obligatorio). R5 = exactamente un rol.
- **Alcance MVP:** entran gestión de cuentas, roles, matriz de permisos, perfil propio y autenticación; `reports` queda fuera como funcionalidad (solo permiso).
- 7 historias INVEST acordadas:
  - US-001 Crear cuenta · US-002 Consultar (tabla) · US-003 Editar · US-004 Eliminar
  - US-005 Editar propio perfil · US-006 Configurar matriz de permisos · US-007 Autenticación (mínima: login + bloqueo de inactivos)
- Rol se asigna dentro de crear/editar (no historia aparte).
- Artefactos: `specs/historias/US-001.md` … `US-007.md`.

## ⚡ Fase 3 — Criterios (completada)

- **US-001** trabajada en entrevista con Julián, criterio por criterio:
  - Estado inicial Activo por defecto.
  - Política de contraseña: ≥8 con mayúscula + minúscula + número.
  - Campo de confirmación de contraseña.
  - Email case-insensitive, trim de espacios, unicidad absoluta (incl. concurrencia).
  - Nombre 2–100, email ≤254 (RFC 5321).
  - Autorización: solo Admin ve/accede la creación.
  - Hashing de contraseña: diferido a Completitud.
- **US-002 … US-007** completadas de forma autónoma por Claude (delegación explícita de Julián), aplicando buenas prácticas y coherencia con US-001.
- **Decisiones de criterio tomadas por Claude (a revisar):** proteger al último Admin al cambiar rol (US-003), no autoeliminación (US-004), borrado físico vs lógico diferido (US-004), exigir contraseña actual al cambiarla (US-005), R3 sobre el propio rol + anti-lockout de Admin (US-006), mensaje genérico anti-enumeración (US-007).
- Artefactos: `specs/criterios/US-001.yaml` … `US-007.yaml` (todos con `validacion_smart` en true).

## 🔍 Fase 4 — Completitud (completada) — las 10 áreas

Fase clave: recorrido de las 10 áreas del ciclo de vida. Gaps descubiertos por Julián (una pregunta a la vez):

- **CFG** — tamaño de página configurable (💡).
- **USR** — desactivar corta la sesión activa de inmediato (⚡); activo/inactivo bastan (sin estados nuevos).
- **SEC** 🔥 — contraseña nunca en respuestas · hash con salt (Argon2id/bcrypt) · autorización validada en backend por petición (no confiar en la UI). → `US-001-SEC.md`
- **AUD** — soft-delete (nada se borra por defecto, para auditar) · reactivar cuenta al recrear un email eliminado · audit trail integral e inmutable de toda acción (incl. lecturas) · retención 12/24 meses. → `US-001-AUD.md`, `US-004`, `US-001-EDGE5`
- **MON** — alertas: login fallido (fuerza bruta), pocos/un solo Admin, errores de backend. → `US-001-MON.md`
- **INT** — autónomo, sin integraciones externas en el MVP (sin gap).
- **MNT** — Admin inicial sembrado (seed) 🔥 + cambio de contraseña forzado al primer acceso + retención/purga + migraciones EF. → `US-001-MNT.md`
- **RPT** — reportes fuera del MVP (sin gap).
- **BCK** — backup delegado a la infra en el MVP (sin gap propio).
- **TST** — cobertura negativa **exhaustiva**: cada regla 🔥/⚡ tendrá su escenario que la viola. → `US-001-TST.md`

Artefactos nuevos: `specs/historias/US-001-SEC.md`, `US-001-AUD.md`, `US-001-MON.md`, `US-001-MNT.md`, `US-001-TST.md`; actualizaciones en YAML de US-001/US-002/US-004/US-007.

## 🔧 Fase 5 — Gherkin + DQS-lite (completada) — DISCOVERY COMPLETO

- Generados 11 `features/*.feature` (US-001..US-007 + gaps SEC/AUD/MON/MNT), con escenarios de éxito **y** negativos por cada regla 🔥/⚡ (cobertura exhaustiva pedida por Julián).
- Borrado `features/ejemplo_formato.feature` (era solo referencia de formato).
- Consolidado `specs/SPEC.md` (historias + reglas explícitas y descubiertas + endpoints + pantallas).
- Reporte de cobertura en `sessions/julian/dqs-lite.md`: 10 áreas exploradas, foco fuerte en SEC/AUD/MNT; sin reglas 🔥/⚡ sin escenario negativo.
- Estado: `current_phase = completed`. **Siguiente paso: `/plan`.**

## ⏸️ Sesión pausada — 2026-07-10

- Discovery detenido tras cerrar Historias, justo antes de iniciar ⚡ Criterios.
- Estado guardado en `SHARED-MEMORY.md` (`current_phase: criterios`, aún `not_started`).
- **Para retomar:** `/discovery resume` → arranca Criterios por US-001, una historia a la vez.
