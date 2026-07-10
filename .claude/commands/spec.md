---
description: "Alias de /discovery. Arranca o reanuda la entrevista de discovery BDD 2.0 Lite del taller."
argument-hint: "[resume — opcional]"
---

Alias de `/discovery`. Hace exactamente lo mismo: invoca el skill **`bdd-discovery`** (`.claude/skills/bdd-discovery/SKILL.md`) para conducir la entrevista de discovery del módulo de usuarios, creando o reanudando la sesión del alumno en `sessions/<slug>/`.

Existe porque algunas slides del taller nombran el comando como `/spec` y otras como `/discovery` — ambos funcionan igual.
