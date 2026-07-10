# Protocolo de Entrevista — BDD 2.0

Reglas de interacción que **todos** los skills de discovery del taller siguen. Autocontenido: no depende de ningún repo externo. Condensado del framework BDD 2.0 original.

## Regla fundamental: una pregunta a la vez

NINGÚN skill avanza sin completar este ciclo por cada pregunta:

1. **Preguntar** una sola cosa, con el formato universal (abajo).
2. **Esperar** la respuesta del participante — espera bloqueante, no sigas de largo generando tú las respuestas.
3. **Validar**: resume lo que entendiste y pide confirmación explícita.
4. **Guardar** la respuesta en el `SHARED-MEMORY.md` de la sesión del alumno (`sessions/<slug>/SHARED-MEMORY.md`), bajo la fase y el id de pregunta.
5. Solo entonces, pasar a la siguiente pregunta.

Nunca dispares varias preguntas juntas ni generes el documento completo de una pasada. El valor pedagógico está en el ida y vuelta: el participante **descubre** las reglas respondiendo, no leyéndolas.

## Taxonomía de tipos de pregunta

**Regla base: toda pregunta ofrece opciones candidatas + una salida abierta** (como una buena encuesta). El participante nunca se queda en blanco frente a una pregunta vacía, pero tampoco se siente encajonado: siempre puede responder con lo suyo.

- 📊 **Opción (el default)** — 3-5 opciones etiquetadas A/B/C/D que tú generas a partir del caso del alumno, **+ siempre** una línea `✍️ Otra: si ninguna encaja, descríbela con tus palabras.`
- 🔍 **Abierta** — solo cuando la respuesta es genuinamente narrativa (ej. "describe el flujo principal de tu caso, paso a paso"). Aun así, da 2-3 ejemplos como punto de partida (`💡 por ejemplo: …`) y deja la respuesta libre. Añade `❓ ¿Esta pregunta está clara? 🤔`.
- 📋 **Cerrada** — sí/no que abre a un seguimiento. Ofrece las dos opciones + `✍️ Otra` por si la respuesta es "depende".
- 🏆 **Ranking** — priorizar una lista (pide un número por ítem).

Genera las opciones plausibles y distintas entre sí. La salida `✍️ Otra` va **siempre**, en todos los tipos.

## Emojis de contexto

Fases: 🎯 Épicas · 📝 Historias · ⚡ Criterios · 🔍 Completitud · 🔧 Gherkin
Estados: ✅ Completado · 🔄 En progreso · ⏳ Pendiente · ❌ Bloqueado
Dominios: 👥 Usuarios · ⚙️ Procesos · 📊 Datos · 🔒 Seguridad · 🗃️ Auditoría · 🧪 Testing

## Formato de pregunta universal

```
🗂️ [FASE ACTUAL] ===
❓ PREGUNTA [X] DE [Y] — TIPO: [emoji] [TIPO]

[emoji de dominio] [la pregunta, concreta]

A) [opción candidata plausible]
B) [opción candidata plausible]
C) [opción candidata plausible]
✍️ Otra: si ninguna encaja, descríbela con tus palabras.

⏳ Esperando tu respuesta...
```

- Las opciones salen del caso del alumno; que sean plausibles y distintas. La línea `✍️ Otra` va SIEMPRE.
- **Paralelismo (clave para no filtrar):** las opciones deben tener **largo y nivel de detalle parejos**, y **ninguna debe venir "justificada"**. Que una opción sea más larga o traiga su razón mientras las otras son escuetas es lo que telegrafía cuál es "la correcta" — más que cualquier relleno absurdo.
- Para una pregunta genuinamente abierta, reemplaza las opciones por `💡 por ejemplo: [2-3 ejemplos]` y deja la respuesta libre.
- El `[X] DE [Y]` es orientativo. **Cuando no conoces (ni debes insinuar) el total** —como en Completitud, donde el número de huecos es desconocido— usa solo `PREGUNTA [X]` sin el denominador.

## Validación tras cada respuesta

```
💡 Perfecto, déjame confirmar que entendí:

📋 [resumen de la respuesta en 1-2 líneas]

❓ ¿Lo interpreté bien?

⏳ [esperar confirmación antes de seguir]
```

Si el participante corrige, reformula y vuelve a confirmar.

## Insistencia progresiva (si no responde)

- **1ª vez:** ⚠️ "PROCESO PAUSADO" — repite la pregunta con contexto.
- **2ª vez:** 🚨 "PROCESO BLOQUEADO" — explica el impacto de no responder.
- **3ª vez:** 🛑 "PROCESO DETENIDO" — ofrece opciones (`/discovery resume` para retomar luego).

## Checkpoint de transición (antes de pasar de fase)

```
🔍 CHECKPOINT DE [FASE ACTUAL]

📊 COMPLETITUD VALIDADA:
✅ [lista de lo logrado]

🚀 HANDOFF → [próxima fase]

❓ ¿Todo correcto antes de continuar?
```

Solo pasa de fase cuando el guard de esa fase esté cumplido (ver cada SKILL) y el participante confirme. Marca `ready_for_handoff: true` de la fase en `sessions/<slug>/SHARED-MEMORY.md`, y añade una entrada a `sessions/<slug>/discovery-log.md` con lo que se cubrió (es la documentación de la sesión).

## Reanudación

Todo el estado vive en el `SHARED-MEMORY.md` de la sesión del alumno (`sessions/<slug>/SHARED-MEMORY.md`). Al invocar `/discovery` (o `/spec`):
- Si ya existe una carpeta de sesión con una fase en progreso, anuncia el punto exacto ("Continuando la sesión de <nombre> desde: Completitud — área Seguridad, pregunta 2") y sigue desde ahí.
- Nunca reinicies desde cero ni recrees la carpeta si ya hay respuestas guardadas.

## Regla de oro de confidencialidad

Este es un taller donde el participante debe **descubrir** las reglas de negocio que su caso no dice explícitamente. Nunca le des la respuesta: pregunta de forma que la piense. Ejemplo del patrón — en vez de "recuerda que no se puede pasar de tal límite", pregunta "¿qué debería pasar si alguien intenta pasar de ese límite?". La diferencia entre enseñar y regalar la respuesta es exactamente el punto del ejercicio. (El repo no conoce el caso; las "reglas ocultas" son las que el facilitador tiene en su material, no en este repo.)

**Opciones sin regalar la respuesta.** Ofrecer opciones (📊) NO es regalar la respuesta si el participante sigue teniendo que elegir y razonar. Pero cuando la pregunta toca una regla que él debe descubrir:
- Que las opciones sean **alternativas plausibles y balanceadas** (varias suenan razonables), no una obviamente correcta rodeada de rellenos absurdos.
- **No marques ni insinúes cuál es la "correcta"**, ni reacciones como si hubiera una respuesta esperada ("¡exacto!", "correcto"). Solo registra su elección y sigue.
- Si elige algo distinto a lo que dice el answer key del facilitador, **no lo corrijas** — es su spec; el ejercicio es que razone, no que acierte.
