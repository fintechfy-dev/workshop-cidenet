---
description: "Genera specs/PLAN.md: iteraciones numeradas con Done-when, a partir de specs/SPEC.md y features/*.feature."
---

Lee `specs/SPEC.md` y todos los `features/*.feature` generados por `/discovery`. Genera `specs/PLAN.md` usando `specs/PLAN.template.md` como formato:

- Divide el trabajo en iteraciones pequeñas y verificables — cada una con un **Done-when concreto** ("tests de X pasando", no "backend listo").
- Backend primero (modelo → reglas de negocio → endpoints), frontend después (las 4 pantallas del brief, tabla y formulario primero).
- No inventes número de iteraciones fijo — comprime o divide según la complejidad real de lo que salió del discovery.
- Cada iteración debe terminar en un commit (el hook de pre-commit exige tests en verde antes de dejar commitear).

Si detectas que una historia del `SPEC.md` no tiene ningún escenario Gherkin correspondiente, señálalo antes de planificar — significa que el discovery quedó incompleto para esa historia.
