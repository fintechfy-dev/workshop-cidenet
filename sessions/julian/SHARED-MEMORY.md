```yaml
metadata: { version: "1.0", alumno: "Julián", last_updated: "2026-07-10", session_status: "paused" }
project_state:
  current_phase: "criterios"   # epicas | historias | criterios | completitud | gherkin | completed
  current_skill: "bdd-criterios"
  resume_hint: "Fase Criterios NO iniciada. Al reanudar: entrar a bdd-criterios y empezar por US-001, una historia a la vez."
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
criterios_phase:   { status: "not_started", answers: {}, ready_for_handoff: false }
completitud_phase:
  status: "not_started"
  current_area: null        # CFG | USR | SEC | AUD | MON | INT | MNT | RPT | BCK | TST
  cobertura_areas: {}       # por área: cuántos gaps y su clasificación
  gaps_descubiertos: []     # por descripción, nunca códigos
  ready_for_handoff: false
gherkin_phase:     { status: "not_started", features_generated: [], ready_for_handoff: false }
handoff_history:
  - { from: "epicas", to: "historias", at: "2026-07-10" }
  - { from: "historias", to: "criterios", at: "2026-07-10" }
```
