---
name: bdd-epicas
description: "Fase 1 del discovery BDD 2.0: entrevista al participante para identificar y puntuar con VUIFED (6 ejes, 1-10) las grandes funcionalidades (épicas) de SU caso. Produce EPIC-001… con score MVP. Invocada por bdd-discovery cuando current_phase = epicas. Una pregunta a la vez. Trabaja sobre el caso que aportó el alumno; no asume ningún dominio."
---

# Fase 🎯 Épicas — Entrevista (VUIFED)

Sigue el protocolo en `../bdd-discovery/reference/interview-protocol.md` (una pregunta a la vez, formato universal con opciones + `✍️ Otra`, validación, guardado en `sessions/<slug>/SHARED-MEMORY.md`).

## Objetivo

Identificar, a partir del **caso que aportó el alumno** (`sessions/<slug>/caso.md`), las **grandes funcionalidades (épicas)** del sistema, y puntuar cada una con VUIFED para decidir prioridad y MVP. Muchos casos de taller tendrán una sola épica clara o unas pocas — no fuerces épicas que el caso no sugiere.

Mantén esta fase **ágil** (unos minutos por épica). El peso del taller está en Criterios y Completitud.

## VUIFED — los 6 ejes (1-10 cada uno)

Por cada épica, puntúa con el participante, una pregunta por eje (o agrupa de a dos si va rápido):

- **Valor de negocio** 💼 — ¿cuánto valor aporta al producto? ¿qué se pierde si no existe?
- **Usuarios** 👥 — ¿a cuántos tipos de usuario y con qué frecuencia impacta?
- **Impacto** 🎯 — ¿qué tan crítica es para el objetivo del sistema?
- **Factibilidad técnica** 🔧 — con el stack del taller (.NET / Postgres / React), ¿qué tan viable es?
- **Esfuerzo estimable** 📏 — ¿qué tan acotada y estimable es? (10 = muy acotada, 1 = enorme e incierta)
- **Dependencias** 🔗 — ¿qué tan libre está de bloqueos externos? (10 = autónoma, 1 = muy dependiente)

Formula cada eje con el formato universal **ofreciendo opciones + salida `✍️ Otra`** (ej. para Valor: `A) 🔴 Crítico · B) 🟠 Alto · C) 🟡 Medio · D) 🟢 Bajo · ✍️ Otra`), traduce la elección a un número 1-10, valídala, y guárdala en `epicas_phase.vuifed`.

## Salida

Crea `specs/epicas/EPIC-001.md` (y EPIC-002… si hay más) con:
- Nombre y descripción de la épica (tomados del caso del alumno).
- Los 6 scores VUIFED y su suma/promedio.
- `mvp_included: true/false` con una frase de justificación del participante.

Registra las épicas en `epicas_phase` y añade una entrada a `sessions/<slug>/discovery-log.md`.

## Guard de fin de fase

Cuando las épicas del caso estén identificadas, puntuadas en los 6 ejes, y el participante confirme → marca `epicas_phase.ready_for_handoff: true` y devuelve control al orquestador, que pasa a Historias.

## Confidencialidad

Aquí no se descubren reglas de negocio — solo se encuadran y puntúan las épicas. No enumeres validaciones ni reglas; eso emerge en Criterios y Completitud.
