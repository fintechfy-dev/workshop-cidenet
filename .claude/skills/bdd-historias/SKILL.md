---
name: bdd-historias
description: "Fase 2 (Historias) del discovery BDD 2.0: entrevista al participante para derivar historias de usuario INVEST a partir de su caso y sus épicas. Invocada por el orquestador bdd-discovery cuando current_phase = historias. Una pregunta a la vez. Trabaja sobre el caso que aportó el alumno; no asume ningún dominio."
---

# Fase 📝 Historias — Entrevista

Sigue el protocolo en `../bdd-discovery/reference/interview-protocol.md` (una pregunta a la vez, opciones + salida abierta, validación, guardado en `sessions/<slug>/SHARED-MEMORY.md`).

## Objetivo

Derivar, junto con el participante, las historias de usuario de **su caso** (`sessions/<slug>/caso.md`), en formato INVEST, a partir de las épicas de la fase anterior. Tu trabajo es que el participante traduzca su caso a historias concretas, no dárselas hechas.

## Banco de preguntas (piso, no techo — adáptalas al caso del alumno)

Formula estas una por una, con el formato universal — **cada una con opciones candidatas + salida `✍️ Otra`** (ver protocolo). Ajusta el total real según lo que responda.

1. **🔍 Abierta — flujos principales.** "En tu caso, ¿cuáles son las acciones principales que los usuarios necesitan hacer?" (pista: pensar en el ciclo de vida de la cosa central del caso — crearla, consultarla, cambiarla, terminarla).
2. **📊 Opción — por dónde empezar.** "¿Qué parte del caso quieres modelar primero en tus historias?" (ofrece 3-4 opciones tomadas de sus épicas + `✍️ Otra`). Úsalo para ordenar el trabajo.
3. **🔍 Abierta — por cada actor/rol que mencione su caso.** "¿Qué puede y qué NO puede hacer un **[actor]**? Descríbelo como historias 'Como [rol] quiero [acción] para [beneficio]'."
4. **📋 Cerrada + seguimiento.** Cuando surja una funcionalidad secundaria, "¿la modelas como una historia aparte?" → si sí, ayúdalo a redactarla.
5. **🏆 Ranking — prioridad.** Presenta las historias que fueron surgiendo y pídele ordenarlas por prioridad (número por historia).

## Por cada historia, valida INVEST

Antes de darla por buena, revisa con el participante (en prosa, no como checklist abrumador): ¿es **I**ndependiente, **N**egociable, **V**aliosa, **E**stimable, **S**mall, **T**estable? Si alguna falla, ayúdalo a partir o ajustar la historia.

## Salida

Por cada historia acordada, crea/actualiza `specs/historias/US-XXX.md` con el formato "Como [rol] quiero [acción] para [beneficio]" + su nota INVEST. Registra los `story_id` en `sessions/<slug>/SHARED-MEMORY.md` bajo `historias_phase`. Al cerrar la fase, añade una entrada a `sessions/<slug>/discovery-log.md` resumiendo las historias acordadas.

## Guard de fin de fase

No pases a Criterios hasta que: (a) las funcionalidades principales del caso tengan al menos una historia, (b) cada historia pasó INVEST, (c) el participante confirmó la lista. Marca `historias_phase.ready_for_handoff: true` y devuelve control al orquestador.

## Regla de confidencialidad

No enumeres tú las reglas de negocio ni los detalles finos. Deja que las historias salgan de lo que el participante deduce de su caso; las reglas finas se descubren en la fase de Completitud, no aquí.
