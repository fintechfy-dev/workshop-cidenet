---
name: bdd-gherkin
description: "Fase 5 (final) del discovery BDD 2.0: genera automáticamente features/*.feature (Given-When-Then) desde los criterios YAML validados, y cierra con el reporte DQS-lite de cobertura. Invocada por bdd-discovery cuando current_phase = gherkin. Esta fase NO hace preguntas."
---

# Fase 🔧 Gherkin + DQS-lite

A diferencia de las fases anteriores, esta **no entrevista** — transforma. Tu insumo son los criterios YAML ya validados (`specs/criterios/US-XXX.yaml`, todos con `validacion_smart` completa) y las reglas descubiertas en Completitud.

> **Este es el entregable final del discovery.** Los `features/*.feature` que generas aquí son la **especificación ejecutable** del módulo — el artefacto que estábamos construyendo todo el discovery, y el que arranca la fase de desarrollo: de estos escenarios salen los tests del TDD (`/test`), antes de escribir una línea de código.

## Paso 1 — Generar Gherkin

Por cada historia, genera un archivo `features/US-XXX.feature` (o agrupa por área si tiene sentido) siguiendo el formato de `features/ejemplo_formato.feature`:

- Tags obligatorios por escenario: `@story_id:US-XXX`, `@origin:discovery_inicial` (historias de las fases 1-3) o `@origin:analisis_completitud` (historias de gap `US-XXX-<AREA>` que salieron de la fase de Completitud), `@priority:N`, `@complexity:low|medium|high`.
- `Background` con las precondiciones compartidas.
- Un `Scenario` por criterio de éxito, y **un `Scenario` negativo por cada regla/validación** (el escenario que la viola a propósito). Un `.feature` que solo tiene caminos felices está incompleto.
- `Scenario Outline` + `Examples` donde haya datos variables (ej. validaciones de un campo con varios valores).

Estos escenarios **son los tests de aceptación** que luego alimentan `/test` en el loop TDD. Escríbelos pensando en que serán ejecutables.

## Paso 2 — Reporte DQS-lite (auditoría del discovery)

Escribe el reporte en `sessions/<slug>/dqs-lite.md` (y muéstraselo también al participante en el chat). En prosa, cubre:

- **Cobertura por área de completitud:** para cada una de las 10 áreas del ciclo de vida (CFG/USR/SEC/AUD/MON/INT/MNT/RPT/BCK/TST), ¿quedó explorada, con gaps, o floja? Nombra las que quedaron flojas y los gaps 🔥 críticos sin resolver.
- **Balance camino-feliz / camino-negativo:** ¿cada regla tiene su escenario que la viola, o hay reglas sin test negativo?
- **Huecos evidentes:** si notas un área que quedó sin explorar, dilo — es una invitación a volver a `bdd-completitud`, no un reproche.

**No pongas un número objetivo** tipo "cubriste X de N reglas". No hay un total conocido para el participante; el objetivo es que la cobertura se sienta sólida por área, no completar un porcentaje. (El facilitador tiene su propia referencia para calibrar; el participante no.)

## Cierre del discovery

1. Consolida `specs/SPEC.md` a partir de `specs/SPEC.template.md`: historias + reglas descubiertas + endpoints/pantallas esbozados + referencia a `specs/criterios/`.
2. Marca `gherkin_phase.ready_for_handoff: true` y `project_state.current_phase: completed` en `sessions/<slug>/SHARED-MEMORY.md`, y añade la entrada de cierre a `sessions/<slug>/discovery-log.md`.
3. Avisa al participante que el discovery está completo, que su sesión quedó documentada en `sessions/<slug>/`, y que el siguiente paso es `/plan`.

## Confidencialidad

El reporte DQS-lite describe cobertura por área en palabras, nunca con códigos "RN-XX" ni con un total de reglas. Los `.feature` reflejan lo que el participante descubrió, no una lista de referencia externa.
