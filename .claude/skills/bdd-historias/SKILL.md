---
name: bdd-historias
description: "Fase 1 del discovery BDD 2.0 Lite: entrevista al participante para derivar historias de usuario INVEST del módulo, a partir de specs/BRIEF.md. Invocada por el orquestador bdd-discovery cuando current_phase = historias. Una pregunta a la vez."
---

# Fase 📝 Historias — Entrevista

Sigue el protocolo en `../bdd-discovery/reference/interview-protocol.md` (una pregunta a la vez, validación, guardado en `sessions/<slug>/SHARED-MEMORY.md`).

## Objetivo

Derivar, junto con el participante, las historias de usuario del módulo (formato INVEST). El brief ya lista los roles (admin/editor/viewer), los recursos y las pantallas — tu trabajo es que el participante traduzca eso a historias concretas, no dárselas hechas.

## Banco de preguntas (piso, no techo)

Formula estas una por una, con el formato universal. Ajusta el total real según lo que responda.

1. **🔍 Abierta — flujos principales.** "Mirando el brief, ¿cuáles son las acciones principales que un **admin** necesita hacer sobre los usuarios del sistema?" (pista: pensar en el ciclo de vida de una cuenta).
2. **📊 Opción — alcance por rol.** "¿Qué rol quieres modelar primero en tus historias? A) admin, B) editor, C) viewer, D) los tres en paralelo." Úsalo para ordenar el trabajo.
3. **🔍 Abierta — por cada rol relevante.** "¿Qué puede y qué NO puede hacer un **[rol]** según el brief? Descríbelo como historias 'Como [rol] quiero [acción] para [beneficio]'."
4. **📋 Cerrada + seguimiento — perfil propio.** "El brief dice que editor y viewer pueden editar su propio perfil. ¿Eso lo modelas como una historia aparte?" → si sí, ayúdalo a redactarla.
5. **🏆 Ranking — prioridad.** Presenta las historias que fueron surgiendo y pídele ordenarlas por prioridad (número por historia).

## Por cada historia, valida INVEST

Antes de darla por buena, revisa con el participante (en prosa, no como checklist abrumador): ¿es **I**ndependiente, **N**egociable, **V**aliosa, **E**stimable, **S**mall, **T**estable? Si alguna falla, ayúdalo a partir o ajustar la historia.

## Salida

Por cada historia acordada, crea/actualiza `specs/historias/US-XXX.md` con el formato "Como [rol] quiero [acción] para [beneficio]" + su nota INVEST. Registra los `story_id` en `sessions/<slug>/SHARED-MEMORY.md` bajo `historias_phase`. Al cerrar la fase, añade una entrada a `sessions/<slug>/discovery-log.md` resumiendo las historias acordadas.

## Guard de fin de fase

No pases a Criterios hasta que: (a) haya al menos una historia por cada rol con acciones de gestión relevantes, (b) cada historia pasó INVEST, (c) el participante confirmó la lista. Marca `historias_phase.ready_for_handoff: true` y devuelve control al orquestador.

## Regla de confidencialidad

No enumeres tú las reglas de negocio ni los permisos exactos por endpoint. Deja que las historias salgan de lo que el participante deduce del brief; las reglas finas se descubren en la fase de Completitud, no aquí.
