---
description: "Ejecuta una vuelta del loop TDD: implementa hasta que los tests de la iteración actual pasen, luego commitea."
argument-hint: "[número de iteración, opcional]"
---

Actúa como el agente `backend` o `frontend` (ver `.claude/agents/`), según qué capa toque la iteración actual de `specs/PLAN.md` (o `$ARGUMENTS` si se especificó una).

1. Corre los tests de la iteración — deben estar en rojo.
2. Implementa el código mínimo necesario para hacerlos pasar, respetando DDD light en backend (invariantes en `Domain`, no en servicios sueltos) y conexión al API real en frontend (nunca datos mockeados).
3. Corre `dotnet build && dotnet test` (y `npm test` si tocaste `frontend/`) hasta que todo esté en verde.
4. Marca la iteración como cumplida en `specs/PLAN.md` (el "Done-when" debe quedar verificado, no solo tachado).
5. Commitea con un mensaje que identifique la iteración (ej. `feat: iteración 2 — reglas de negocio`). El hook de pre-commit va a correr `dotnet format` + tests automáticamente y bloquea el commit si algo falla — si eso pasa, corrige antes de reintentar, no fuerces el commit.
