---
name: bdd-criterios
description: "Fase 3 (Criterios) del discovery BDD 2.0: entrevista al participante para escribir criterios de aceptación SMART en YAML por cada historia. Invocada por bdd-discovery cuando current_phase = criterios. Una pregunta a la vez. El Gherkin se genera en la etapa 5 (bdd-gherkin) a partir de estos criterios — aquí todavía no, es cuestión de secuencia."
---

# Fase ⚡ Criterios — Entrevista

Sigue el protocolo en `../bdd-discovery/reference/interview-protocol.md`.

## Objetivo

Por cada historia de la fase anterior, escribir junto con el participante los criterios de aceptación en **YAML** (lenguaje de negocio; el Gherkin viene después, en la etapa 5, a partir de estos criterios). Trabaja **historia por historia**, no todas a la vez.

## Formato de salida (un archivo por historia)

`specs/criterios/US-XXX.yaml`:

```yaml
story_id: "US-XXX"
titulo_historia: "Como [rol] quiero [acción] para [beneficio]"
criterios_aceptacion:
  escenarios_exito: []      # camino feliz
  validaciones_reglas: []   # reglas de negocio y validaciones de campo
  manejo_errores: []        # qué pasa cuando algo sale mal, con el mensaje esperado
  requisitos_ux: []         # comportamiento visible relevante
  casos_edge: []            # situaciones límite
validacion_smart:
  specific: false
  measurable: false
  achievable: false
  relevant: false
  testable: false
```

## Cómo entrevistas (por cada historia)

Presenta cada pregunta con **opciones candidatas + salida `✍️ Otra`** (ver protocolo) — incluso las abiertas llevan 2-3 ejemplos como punto de partida.

1. **🔍 Abierta — camino feliz.** "Para '[historia]', ¿cómo se ve el caso exitoso, paso a paso?"
2. **🔍 Abierta — validaciones.** "¿Qué tiene que cumplir la entrada para que la operación sea válida? (campos obligatorios, formatos, reglas del negocio)."
3. **📋 Cerrada + seguimiento — errores.** "¿Hay formas en que esto puede fallar?" → por cada una, "¿qué mensaje o comportamiento esperas?"
4. **🔍 Abierta — límites.** "¿Alguna situación poco común pero posible que haya que contemplar?"
5. Al cerrar la historia, revisa con el participante si cada criterio es **SMART** (específico, medible, alcanzable, relevante, testeable). Marca `validacion_smart` en `true` solo cuando todos lo cumplan.

## Importante

- **No escribas Given-When-Then aquí — todavía.** El Gherkin sí se genera, pero en la etapa 5 (bdd-gherkin), automáticamente desde estos criterios. Es secuencia, no exclusión: si el participante quiere adelantarse, recuérdale que primero cerramos los criterios en lenguaje de negocio y de ahí sale el Gherkin.
- Guarda cada criterio en el YAML apenas se acuerde; registra progreso en `sessions/<slug>/SHARED-MEMORY.md` (`criterios_phase`, con contador de historias completadas). Al cerrar la fase, añade una entrada a `sessions/<slug>/discovery-log.md`.

## Guard de fin de fase

No pases a Completitud hasta que cada historia tenga su YAML con `validacion_smart` completa y el participante lo haya confirmado. Marca `criterios_phase.ready_for_handoff: true`.

## Regla de confidencialidad

Pregunta, no dictes. Si el participante no menciona una validación importante (ej. fuerza de contraseña, unicidad de email), **no se la des como criterio** — eso se sondea en la fase de Completitud con preguntas, para que la descubra él.
