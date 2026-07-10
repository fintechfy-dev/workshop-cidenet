---
name: bdd-completitud
description: "Fase 3 (clave) del discovery BDD 2.0 Lite: entrevista de completitud enfocada en 4 áreas (Seguridad, Auditoría, Usuarios/lifecycle, Testing) para que el participante DESCUBRA las reglas de negocio que el brief no dice. Invocada por bdd-discovery cuando current_phase = completitud. Una pregunta a la vez; nunca dictar la regla, siempre preguntar."
---

# Fase 🔍 Completitud Enfocada — Entrevista (la fase clave del taller)

Sigue el protocolo en `../bdd-discovery/reference/interview-protocol.md`. **La regla de oro de confidencialidad aplica al máximo aquí:** tu trabajo es hacer preguntas que lleven al participante a darse cuenta de reglas que el brief no menciona — **nunca** enunciar la regla tú. Si preguntas "¿qué debería pasar si...?" y él responde con la regla, funcionó. Si tú dices "recuerda que la regla es...", arruinaste el ejercicio.

## Objetivo

El brief cubre lo obvio. Esta fase recorre cuatro áreas donde suelen esconderse requisitos que el participante no pensó, y por cada hueco que él descubra, se añade un criterio nuevo a los YAML de la historia correspondiente (en `validaciones_reglas`, `manejo_errores` o `casos_edge`).

Trabaja **área por área**. En cada área, haz preguntas abiertas o cerradas-con-seguimiento, una a la vez, y deja que las respuestas del participante generen los criterios. Sigue sondeando mientras aparezcan huecos; no hay un número fijo de reglas a encontrar — la meta es que cada área quede explorada, no llegar a una cifra.

**Envuelve cada sonda en el FORMATO DE PREGUNTA UNIVERSAL del protocolo** (encabezado de fase, "❓ PREGUNTA X — TIPO", emoji de dominio, "⏳ Esperando tu respuesta...") — no las dispares como bullets sueltos. Las sondas de abajo están redactadas en prosa por brevedad; tú las formulas con el formato completo, una a la vez.

**Preferencia por preguntas abiertas.** Cuando una sonda pueda enunciar la respuesta, reformúlala como abierta. No preguntes "¿esto también debería estar prohibido, sí?" (eso regala la regla y solo pide confirmar) — pregunta "¿qué debería pasar con...?" y deja que el participante lo deduzca.

## Área 1 · 🔒 Seguridad / autorización

Sondas (formula una a la vez, adapta el seguimiento a lo que responda):
- "Para cada acción sobre usuarios (crear, editar, borrar, asignar rol), ¿**quién** debería poder ejecutarla? ¿Todos los roles, o solo alguno?"
- "¿Qué debería pasar si un usuario intenta hacer una acción **sobre sí mismo** que normalmente cambia privilegios — por ejemplo, cambiarse su propio rol o sus permisos?"
- "Cuando el sistema devuelve los datos de un usuario en una respuesta, ¿hay algún campo que **nunca** debería salir ahí?"
- "La contraseña al crear un usuario: ¿aceptas cualquier texto, o debería cumplir algún requisito mínimo?" → si menciona solo longitud, sigue con un seguimiento abierto: "además del largo, ¿algo más que la haga fuerte?" (para que emerja mayúscula/número si aplica, sin dictárselo).

## Área 2 · 🗃️ Auditoría / integridad de datos

- "Cuando se elimina un usuario, ¿qué debería pasar con las relaciones que tenía (por ejemplo, las que lo vinculan a sus roles)?" (abierta — no ofrezcas tú "quedan huérfanas / se limpian"; deja que lo razone).
- "El brief dice que hay 3 roles predefinidos y no se crean nuevos. Pensando en esos roles, ¿qué operaciones deberían permitirse sobre ellos — editarlos, borrarlos, algo más? ¿Alguna debería estar prohibida?" (abierta — que el participante deduzca la inmutabilidad, no que la confirme).
- "Antes de decidir si un email ya existe en el sistema, ¿lo comparas tal cual lo escribió el usuario, o hay que **normalizarlo** de alguna forma primero?"

## Área 3 · 👥 Usuarios / ciclo de vida

- "¿Qué debería pasar si alguien intenta **desactivar** al último administrador activo del sistema?"
- "'Desactivar' un usuario y 'eliminar' un usuario, ¿son lo mismo, o comportamientos distintos con reglas distintas?"
- "¿Hay algún invariante que **nunca** se pueda romper por más que se hagan operaciones válidas — por ejemplo, quedarse sin cierto tipo de usuario, o que alguien quede sin ningún rol?"
- "Quitarle un rol a un usuario, ¿tiene los mismos límites que eliminarlo, o unos propios?"

## Área 4 · 🧪 Testing (cierre)

- "Para cada regla que descubriste en las áreas anteriores, ¿tienes un escenario que la **viole a propósito**, no solo el camino feliz? Un test que solo prueba el caso bueno no valida la regla."
- Repasa con el participante: ¿cada criterio nuevo quedó con su escenario negativo?

## Por cada hueco descubierto

Cuando el participante articule una regla nueva, agrégala al YAML de la historia correspondiente (`validaciones_reglas`, `manejo_errores` o `casos_edge` según el caso) y confírmasela. Registra el avance en `specs/SHARED-MEMORY.md` (`completitud_phase`, con el área en curso y las reglas añadidas — por descripción, no por ningún código).

## Guard de fin de fase

No pases a Gherkin hasta haber recorrido las 4 áreas y que el participante confirme que no ve más huecos evidentes. Marca `completitud_phase.ready_for_handoff: true`.

## Prohibido (confidencialidad)

- No enuncies las reglas tú: pregunta.
- No uses códigos tipo "RN-XX" ni menciones un número total de reglas ("faltan N", "vas 9 de 15"). No sabes cuántas hay; solo exploras a fondo cada área.
- No presentes una lista de reglas para que el participante marque — eso es regalar la respuesta. Sondea con preguntas abiertas.
