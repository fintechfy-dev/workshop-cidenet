---
description: "Arranca (o reanuda) la entrevista de discovery BDD 2.0 Lite del taller: convierte specs/BRIEF.md en tu spec, una pregunta a la vez, a través del skill bdd-discovery."
argument-hint: "[resume — opcional, para retomar desde el punto guardado]"
---

Invoca el skill **`bdd-discovery`** (`.claude/skills/bdd-discovery/SKILL.md`) para conducir el discovery del módulo de usuarios.

- Sin argumentos → lee `specs/SHARED-MEMORY.md` y arranca o reanuda automáticamente según `current_phase`.
- Con `$ARGUMENTS` = `resume` → fuerza la reanudación desde el punto exacto guardado.

Recuerda el contrato del taller: es una **entrevista**, una pregunta a la vez. No generes la spec de una sola pasada. La fase de completitud enfocada es donde el participante descubre las reglas que el brief no dice — pregunta, no dictes.
