---
description: "Auditoría de implementación: compara el código actual contra specs/SPEC.md y reporta cobertura funcional y técnica."
---

Actúa como el agente `calidad` (ver `.claude/agents/calidad.md`) y produce un reporte de auditoría:

**Cobertura funcional** — recorre cada regla/criterio de `specs/SPEC.md` y confirma si hay código + test que la valide. Sé específico por regla, no des un veredicto global sin desglose.

**Cobertura técnica:**
- `dotnet test` en verde
- `npm test` en verde (si el módulo de frontend está en alcance de esta corrida)
- `docker compose up --build` levanta los 3 servicios sin error
- Historial de commits: ¿hay un commit identificable por iteración, o todo quedó en uno solo?

**Reporte final:** lista de gaps concretos (qué falta, no "incompleto"), y una recomendación honesta de si esto está listo para PR o qué le falta primero. Si el resultado no es 100%, no es un fracaso — documenta los gaps como issues, eso también es un entregable válido de este taller.
