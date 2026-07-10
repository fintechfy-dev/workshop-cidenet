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

- 🔍 **Abierta** — exploración ("¿qué objetivos...?"). Añade siempre el detector de confusión `❓ ¿Esta pregunta está clara? 🤔`.
- 📋 **Cerrada** — sí/no, sirve de compuerta a una pregunta de seguimiento.
- 📊 **Opción** — opciones etiquetadas A/B/C/D.
- 🏆 **Ranking** — priorizar una lista (pide un número por ítem).

## Emojis de contexto

Fases: 🎯 Épicas · 📝 Historias · ⚡ Criterios · 🔍 Completitud · 🔧 Gherkin
Estados: ✅ Completado · 🔄 En progreso · ⏳ Pendiente · ❌ Bloqueado
Dominios: 👥 Usuarios · ⚙️ Procesos · 📊 Datos · 🔒 Seguridad · 🗃️ Auditoría · 🧪 Testing

## Formato de pregunta universal

```
🗂️ [FASE ACTUAL] ===
❓ PREGUNTA [X] DE [Y] — TIPO: [emoji] [TIPO]

[emoji de dominio] [la pregunta, concreta]

💡 [pista opcional: ejemplos o por qué se pregunta]

[si es abierta: ❓ ¿Esta pregunta está clara? 🤔]

⏳ Esperando tu respuesta...
```

El contador `[X] DE [Y]` es un piso orientativo, no un techo: inserta preguntas de seguimiento cuando el dominio lo justifique.

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

Este es un taller donde el participante debe **descubrir** las reglas de negocio ocultas. Nunca le des la respuesta: pregunta de forma que la piense. Ejemplo — en vez de "recuerda que no se puede desactivar al último admin", pregunta "¿qué debería pasar si alguien intenta desactivar al último admin activo del sistema?". La diferencia entre enseñar y regalar la respuesta es exactamente el punto del ejercicio.
