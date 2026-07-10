---
description: "Alias de /discovery. Arranca o reanuda la entrevista de discovery BDD 2.0 del taller."
argument-hint: "[resume — opcional]"
---

Alias de `/discovery`. Hace exactamente lo mismo: invoca el skill **`bdd-discovery`** (`.claude/skills/bdd-discovery/SKILL.md`) para conducir la entrevista de discovery sobre el caso que aporta el alumno, creando o reanudando la sesión en `sessions/<slug>/`.

Existe porque algunas slides del taller nombran el comando como `/spec` y otras como `/discovery` — ambos funcionan igual.
