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

## ⏸️ Sesión pausada — 2026-07-10

- Discovery detenido tras cerrar Historias, justo antes de iniciar ⚡ Criterios.
- Estado guardado en `SHARED-MEMORY.md` (`current_phase: criterios`, aún `not_started`).
- **Para retomar:** `/discovery resume` → arranca Criterios por US-001, una historia a la vez.
