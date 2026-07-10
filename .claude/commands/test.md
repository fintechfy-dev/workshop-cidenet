---
description: "Genera tests (backend xUnit o frontend Vitest) desde los escenarios Gherkin de la iteración actual — tests primero, antes del código."
argument-hint: "[número de iteración, opcional — por defecto la siguiente pendiente en specs/PLAN.md]"
---

Actúa como el agente `calidad` (ver `.claude/agents/calidad.md`).

Identifica la iteración actual en `specs/PLAN.md` (o usa `$ARGUMENTS` si se especificó una). Encuentra los escenarios Gherkin relevantes en `features/` para esa iteración y genera los tests correspondientes:

- Backend → `tests/Api.Tests/`, un test xUnit por `Scenario`, nombrado por el escenario.
- Frontend → junto al componente en `frontend/src/pages/`, un test Vitest+Testing Library por `Scenario` relevante a esa pantalla.

Los tests deben compilar/correr y fallar (rojo) contra el código actual. Si algún test ya pasa sin implementación nueva, revisa si el escenario está mal traducido o si esa parte ya existía. No implementes el código que los hace pasar — eso es `/iterate`.
