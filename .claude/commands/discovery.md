---
description: "Arranca (o reanuda) la entrevista de discovery BDD 2.0 del taller: convierte specs/BRIEF.md en tu spec, una pregunta a la vez, a través del skill bdd-discovery."
argument-hint: "[resume — opcional, para retomar desde el punto guardado]"
---

Invoca el skill **`bdd-discovery`** (`.claude/skills/bdd-discovery/SKILL.md`) para conducir el discovery del módulo de usuarios.

- Sin argumentos → si aún no hay sesión, pregunta tu nombre y crea `sessions/<tu-nombre>/`; si ya existe, lee `sessions/<slug>/SHARED-MEMORY.md` y reanuda según `current_phase`.
- Con `$ARGUMENTS` = `resume` → fuerza la reanudación desde el punto exacto guardado.

Recuerda el contrato del taller: es una **entrevista** por las 5 etapas (Épicas → Historias → Criterios → Completitud → Gherkin), una pregunta a la vez. No generes la spec de una sola pasada. La fase de Completitud (las 10 áreas del ciclo de vida) es donde el participante descubre las reglas que el brief no dice — pregunta, no dictes.
