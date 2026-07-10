---
name: bdd-epicas
description: "Fase 1 del discovery BDD 2.0: entrevista al participante para encuadrar y puntuar la épica del módulo con VUIFED (6 ejes, 1-10). Como la épica ya viene dada por el brief (módulo de usuarios), es una etapa corta que produce EPIC-001 con score MVP. Invocada por bdd-discovery cuando current_phase = epicas. Una pregunta a la vez."
---

# Fase 🎯 Épicas — Entrevista (VUIFED)

Sigue el protocolo en `../bdd-discovery/reference/interview-protocol.md` (una pregunta a la vez, formato universal, validación, guardado en `sessions/<slug>/SHARED-MEMORY.md`).

## Objetivo

En BDD 2.0 completo, esta fase identifica las grandes funcionalidades y decide cuáles entran al MVP. **En este taller la épica ya está dada** por el brief: el "Módulo de Usuarios, Roles y Permisos". Así que esta etapa es **corta**: el participante encuadra esa única épica y la puntúa con VUIFED, para que viva la mecánica completa del framework sin inventar épicas que no existen.

No te alargues: esta fase debe sentirse ágil (unos minutos), no una fase larga. El peso del taller está en Criterios y Completitud.

## VUIFED — los 6 ejes (1-10 cada uno)

Puntúa la épica dada con el participante, una pregunta por eje (o agrupa de a dos si el participante va rápido):

- **Valor de negocio** 💼 — ¿cuánto valor aporta este módulo al producto? ¿qué se pierde si no existe?
- **Usuarios y roles** 👥 — ¿a cuántos tipos de usuario y con qué frecuencia impacta?
- **Impacto estratégico** 🎯 — ¿qué tan crítico es para la seguridad/gobernanza de la plataforma?
- **Factibilidad técnica** 🔧 — con el stack dado (.NET/Postgres/React), ¿qué tan viable es?
- **Esfuerzo estimable** 📏 — ¿qué tan acotado y estimable es? (10 = muy acotado, 1 = enorme e incierto)
- **Dependencias** 🔗 — ¿qué tan libre está de bloqueos externos? (10 = autónomo, 1 = muy dependiente)

Para cada eje: formula la pregunta con el formato universal **ofreciendo opciones + salida `✍️ Otra`** (ej. para Valor: `A) 🔴 Crítico · B) 🟠 Alto · C) 🟡 Medio · D) 🟢 Bajo · ✍️ Otra`), traduce su elección a un número 1-10, espera la respuesta, valídala, y guárdala en `sessions/<slug>/SHARED-MEMORY.md` (`epicas_phase.vuifed`).

## Salida

Crea `specs/epicas/EPIC-001.md` con:
- El nombre y descripción de la épica (Módulo de Usuarios, Roles y Permisos).
- Los 6 scores VUIFED y su suma/promedio.
- `mvp_included: true` (es la única épica y es el objeto del taller) y una frase de justificación del participante.

Registra `EPIC-001` en `epicas_phase` y añade una entrada a `sessions/<slug>/discovery-log.md`.

## Guard de fin de fase

Cuando la épica esté encuadrada, puntuada en los 6 ejes, y el participante lo confirme → marca `epicas_phase.ready_for_handoff: true` y devuelve control al orquestador, que pasa a Historias.

## Confidencialidad

Aquí no se descubren reglas de negocio — solo se encuadra y puntúa la épica. No enumeres validaciones ni reglas; eso emerge en Criterios y Completitud.
