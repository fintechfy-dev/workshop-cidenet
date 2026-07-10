# 🧠 SHARED MEMORY — Discovery del Módulo de Usuarios

> Estado del discovery BDD 2.0 Lite. El skill `bdd-discovery` lee y escribe este archivo.
> Guarda cada respuesta apenas la recibas: si la sesión se corta, `/discovery resume` retoma desde aquí.
> Es tu artefacto de trabajo — commitéalo junto con tus specs.

```yaml
metadata:
  version: "lite-1.0"
  last_updated: null            # se actualiza en cada escritura

project_state:
  # La épica ya está dada (módulo de usuarios), así que el discovery arranca en Historias.
  current_phase: "historias"    # historias | criterios | completitud | gherkin | completed
  current_skill: "bdd-historias"
  started_at: null
  last_activity: null

project_info:
  project_name: "Módulo de Usuarios, Roles y Permisos"
  input_brief: "specs/BRIEF.md"
  roles_dados: ["admin", "editor", "viewer"]

# ---------------------------------------------------------------------------
# Fase 1 — Historias (bdd-historias)
# ---------------------------------------------------------------------------
historias_phase:
  status: "not_started"         # not_started | in_progress | completed
  progress_percent: 0
  answers: {}                   # se llena con las respuestas del participante
  ready_for_handoff: false

stories_identified: []          # [{ story_id, titulo, invest_ok }]

# ---------------------------------------------------------------------------
# Fase 2 — Criterios SMART (bdd-criterios)
# ---------------------------------------------------------------------------
criterios_phase:
  status: "not_started"
  progress_percent: 0
  current_story: null
  answers: {}
  ready_for_handoff: false

criteria_files: []              # ["specs/criterios/US-001.yaml", ...]

# ---------------------------------------------------------------------------
# Fase 3 — Completitud enfocada (bdd-completitud) — la fase clave
# ---------------------------------------------------------------------------
completitud_phase:
  status: "not_started"
  progress_percent: 0
  current_area: null            # seguridad | auditoria | usuarios | testing
  answers: {}
  reglas_descubiertas: []       # por descripción, generadas por el participante
  ready_for_handoff: false

# ---------------------------------------------------------------------------
# Fase 4 — Gherkin + DQS-lite (bdd-gherkin)
# ---------------------------------------------------------------------------
gherkin_phase:
  status: "not_started"
  progress_percent: 0
  features_generated: []
  ready_for_handoff: false

# ---------------------------------------------------------------------------
# Bitácora
# ---------------------------------------------------------------------------
handoff_history: []             # transiciones entre fases
user_decisions: []              # decisiones que el participante confirmó
```
