```yaml
metadata: { version: "1.0", alumno: "Julián", last_updated: "2026-07-10", session_status: "paused" }
project_state:
  current_phase: "completed"   # epicas | historias | criterios | completitud | gherkin | completed
  current_skill: null
  resume_hint: "Discovery COMPLETO. Siguiente paso fuera del discovery: /plan (genera specs/PLAN.md desde specs/SPEC.md + features/)."
epicas_phase:
  status: "completed"
  epicas:
    - id: "EPIC-001"
      nombre: "Administración de usuarios y accesos"
      descripcion: "Gestión de cuentas de usuario, asignación de roles y configuración de la matriz de permisos como un mismo bloque."
      vuifed: { valor: 10, usuarios: 10, impacto: 7, factibilidad: 7, esfuerzo: 7, dependencias: 10 }  # total 51 / prom 8.5
      mvp_included: true
  ready_for_handoff: true
historias_phase:
  status: "completed"
  answers:
    roles_por_usuario: "exactamente 1 (single-role, obligatorio)"
    alcance_mvp: "A,B,C,D,E incluidas; F (reports) excluida como funcionalidad, queda solo como permiso"
    rol_se_asigna_en: "crear (US-001) y editar (US-003), no historia aparte"
    us_007_alcance: "mínimo: login + bloqueo de inactivos (sin recuperación de contraseña ni MFA)"
  story_ids: ["US-001", "US-002", "US-003", "US-004", "US-005", "US-006", "US-007"]
  ready_for_handoff: true
criterios_phase:
  status: "in_progress"
  status: "completed"
  current_story: null
  historias_completadas: ["US-001", "US-002", "US-003", "US-004", "US-005", "US-006", "US-007"]
  us_001_notas: "estado default Activo; pass ≥8 con may/min/num; confirmar contraseña; email case-insensitive+trim+unicidad absoluta; nombre 2-100, email ≤254; solo Admin ve/accede; hashing diferido a Completitud"
  us_002_007_modo: "US-002..US-007 completadas de forma autónoma por Claude (delegación explícita de Julián), con buenas prácticas y coherencia con las decisiones de US-001"
  decisiones_derivadas_a_revisar:
    - "US-003-ERR3: proteger al último Admin al cambiar rol (extensión de R1)"
    - "US-004-V3: un Admin no puede autoeliminarse"
    - "US-004-EDGE1: borrado físico vs lógico (diferido a Completitud)"
    - "US-005-V4: exigir contraseña actual para cambiarla"
    - "US-006-V2/ERR1: R3 sobre el propio rol + anti-lockout de Admin"
    - "US-007-ERR1: mensaje genérico anti-enumeración"
  ready_for_handoff: true
completitud_phase:
  status: "completed"
  current_area: null        # 10 áreas recorridas
  cobertura_areas:
    CFG: { gaps: 1, clasif: "1x💡" }
    USR: { gaps: 1, clasif: "1x⚡" }
    SEC: { gaps: 3, clasif: "3x🔥" }
    AUD: { gaps: 4, clasif: "3x⚡ 1x💡 (+soft-delete)" }
    MON: { gaps: 3, clasif: "3x⚡ (US-001-MON)" }
    INT: { gaps: 0, clasif: "autónomo, sin integraciones en MVP" }
    MNT: { gaps: 4, clasif: "1x🔥 1x⚡ 2x💡 (US-001-MNT)" }
    RPT: { gaps: 0, clasif: "reportes fuera del MVP" }
    BCK: { gaps: 0, clasif: "backup delegado a infra en MVP" }
    TST: { gaps: 3, clasif: "1x🔥 1x⚡ (US-001-TST, política cobertura negativa exhaustiva)" }
  gaps_descubiertos:
    - "CFG: tamaño de página configurable (US-002-CFG1, 💡)"
    - "USR: desactivar corta la sesión activa de inmediato (US-007-EDGE1, ⚡)"
    - "SEC: contraseña nunca en respuestas (US-001-SEC/SEC-1, 🔥)"
    - "SEC: hash con salt Argon2id/bcrypt (US-001-SEC/SEC-2, 🔥)"
    - "SEC: autorización validada en backend por petición (US-001-SEC/SEC-3, 🔥)"
    - "AUD: borrado lógico/soft-delete, nada se borra por defecto (US-004-EDGE1, ⚡)"
    - "AUD: reactivar cuenta al recrear email eliminado (US-001-EDGE5, ⚡)"
    - "AUD: audit trail integral e inmutable de toda acción (US-001-AUD, ⚡)"
    - "AUD: retención 12/24 meses para purga futura (US-004-EDGE2, 💡)"
    - "MON: alertas login fallido / pocos-Admin / errores backend (US-001-MON, ⚡)"
    - "INT: sin integraciones externas en el MVP (sin gap)"
    - "MNT: Admin seed inicial + cambio de contraseña al primer acceso + retención + migraciones (US-001-MNT)"
    - "RPT: reportes fuera del MVP (sin gap)"
    - "BCK: backup delegado a infra en MVP (sin gap propio)"
    - "TST: cobertura negativa exhaustiva de cada regla 🔥/⚡ (US-001-TST, 🔥)"
  ready_for_handoff: true
gherkin_phase:
  status: "completed"
  features_generated: ["US-001", "US-002", "US-003", "US-004", "US-005", "US-006", "US-007", "US-001-SEC", "US-001-AUD", "US-001-MON", "US-001-MNT"]
  spec_consolidada: "specs/SPEC.md"
  dqs_lite: "sessions/julian/dqs-lite.md"
  ready_for_handoff: true
handoff_history:
  - { from: "epicas", to: "historias", at: "2026-07-10" }
  - { from: "historias", to: "criterios", at: "2026-07-11" }
  - { from: "criterios", to: "completitud", at: "2026-07-11" }
  - { from: "completitud", to: "gherkin", at: "2026-07-11" }
  - { from: "gherkin", to: "completed", at: "2026-07-11" }
```
