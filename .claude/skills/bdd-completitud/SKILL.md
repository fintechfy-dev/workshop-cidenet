---
name: bdd-completitud
description: "Fase 4 (clave) del discovery BDD 2.0: autocrítica sistemática recorriendo las 10 áreas del ciclo de vida (CFG/USR/SEC/AUD/MON/INT/MNT/RPT/BCK/TST) para que el participante DESCUBRA lo que el brief no dice. Genera historias de gap US-XXX-AREA clasificadas 🔥/⚡/💡. Invocada por bdd-discovery cuando current_phase = completitud. Una pregunta a la vez; nunca dictar la regla, siempre preguntar."
---

# Fase 🔍 Completitud — Autocrítica por las 10 Áreas del Ciclo de Vida (fase clave del taller)

Sigue el protocolo en `../bdd-discovery/reference/interview-protocol.md`. **La regla de oro de confidencialidad aplica al máximo aquí:** haces preguntas que llevan al participante a darse cuenta de lo que falta — **nunca** enuncias la regla tú. Si preguntas "¿qué debería pasar si...?" y él responde con la regla, funcionó. Si tú dices "recuerda que la regla es...", arruinaste el ejercicio.

**Envuelve cada sonda en el FORMATO DE PREGUNTA UNIVERSAL** (encabezado de área, "❓ PREGUNTA X — TIPO", opciones A/B/C + `✍️ Otra`, "⏳ Esperando tu respuesta..."), una a la vez. Las sondas de abajo están en prosa por brevedad; tú las presentas con opciones candidatas + salida abierta.

**Opciones sin regalar la regla (crítico en esta fase).** Ofrece opciones para que el participante no se quede en blanco, pero:
- Que las opciones sean **alternativas plausibles y balanceadas**, con **largo y detalle parejos** y **ninguna justificada** — que una opción sea más larga o "explicada" es lo que delata cuál es la correcta, más que un relleno absurdo.
- Encabezado **sin denominador** aquí (`❓ PREGUNTA X`, no "X DE Y"): el número de huecos es desconocido y no debes insinuar un total.
- **No marques cuál es la correcta** ni reacciones como si hubiera una respuesta esperada. Solo registra su elección.
- Ejemplo bueno para "último admin": `A) se desactiva normalmente · B) se desactiva pero avisa · C) el sistema lo impide · ✍️ Otra`. El participante razona y elige; tú no dices cuál.
- Si una opción enunciaría la regla de forma demasiado directa, déjala como pregunta abierta con ejemplos en vez de opciones cerradas.

## Objetivo

El brief cubre lo obvio. Recorre las **10 áreas** buscando lo que falta. Por cada hueco que el participante descubra:
1. Si es una regla/validación de una historia existente → agrégala al YAML de esa historia (`validaciones_reglas`, `manejo_errores` o `casos_edge`).
2. Si es funcionalidad nueva que amerita su propia historia → crea una **historia de gap** `specs/historias/US-XXX-<AREA>.md` (ej. `US-001-SEC`), con `origin: analisis_completitud` y el área.
3. **Clasifica cada gap:** 🔥 Crítico (rompe seguridad/integridad; va sí o sí) · ⚡ Importante (debería estar en el MVP) · 💡 Mejora (nice-to-have).

Trabaja **área por área**. No hay un número fijo de reglas a encontrar; la meta es que las 10 áreas queden recorridas. Algunas áreas tendrán varios gaps, otras ninguno — está bien.

## Las 10 áreas

### CFG · ⚙️ Configuración
- "¿Hay algo del módulo que debería ser configurable en vez de estar fijo en código (ej. la definición de roles, la matriz de permisos)? ¿Quién y cómo lo cambia?"

### USR · 👥 Usuarios / Permisos (ciclo de vida)
- "¿Qué debería pasar en los **límites** del ciclo de vida de un usuario — el último de un tipo, el único que queda?"
- "'Desactivar' un usuario y 'eliminar' un usuario, ¿son lo mismo o comportamientos distintos con reglas distintas?"
- "¿Hay algún invariante que **nunca** pueda romperse con operaciones válidas (ej. quedarse sin cierto tipo de usuario, o alguien sin ningún rol)?"
- "Quitarle un rol a un usuario, ¿tiene los mismos límites que eliminarlo, o límites propios?"

### SEC · 🔒 Seguridad / autorización
- "Para cada acción sobre usuarios (crear, editar, borrar, asignar rol), ¿**quién** debería poder ejecutarla? ¿Todos los roles, o solo alguno?"
- "¿Qué debería pasar si un usuario intenta una acción **sobre sí mismo** que cambia sus privilegios (cambiarse el rol, darse permisos)?"
- "Cuando el API devuelve los datos de un usuario, ¿hay algún campo que **nunca** debería salir en la respuesta?"
- "La contraseña al crear un usuario: ¿aceptas cualquier texto o debería cumplir un mínimo?" → si solo menciona longitud, sigue: "además del largo, ¿algo más que la haga fuerte?".

### AUD · 🗃️ Auditoría / integridad
- "Cuando se elimina un usuario, ¿qué debería pasar con las relaciones que tenía (las que lo vinculan a sus roles)?" (abierta — no ofrezcas tú "huérfanas / se limpian").
- "El brief dice 3 roles predefinidos que no se crean. Pensando en esos roles, ¿qué operaciones deberían permitirse sobre ellos? ¿Alguna debería estar prohibida?" (que deduzca la inmutabilidad, no que la confirme).
- "Cuando exiges que el email sea único, ¿cómo decides si dos correos son 'el mismo'? ¿Bastan idénticos carácter por carácter?" (abierta — deja que emerja el tema de mayúsculas/espacios; no nombres tú la solución).
- "¿Qué acciones sobre usuarios/roles deberían quedar registradas para poder auditar quién hizo qué?"

### MON · 📈 Monitoreo
- "¿Qué del módulo querrías poder observar en producción (intentos de login fallidos, cambios de rol, errores)? ¿Algo que debería disparar una alerta?"

### INT · 🔗 Integraciones
- "¿El módulo necesita hablar con algo externo — un proveedor de identidad, envío de correo para recuperar contraseña, un SSO? ¿Qué pasa si ese externo no responde?"

### MNT · 🔧 Mantenimiento
- "Con el tiempo: ¿hay datos que haya que limpiar o migrar (usuarios inactivos viejos, cambios en la estructura de roles)? ¿Cómo se versiona eso?"

### RPT · 📊 Reportes
- "El brief menciona un recurso 'reports'. ¿Qué reportes del módulo tendrían sentido y **quién** puede verlos según su rol?"

### BCK · 💾 Backup
- "Si se corrompe o se pierde la base de usuarios, ¿qué necesitas para recuperarla? ¿Hay datos que no se pueden perder bajo ninguna circunstancia?"

### TST · 🧪 Testing (cierre)
- "Por cada regla que descubriste en las áreas anteriores, ¿tienes un escenario que la **viole a propósito**, no solo el camino feliz? Un test que solo prueba el caso bueno no valida la regla."
- Repasa: ¿cada gap crítico tiene su escenario negativo?

## Registro

Por cada gap: agrégalo al YAML de la historia (o crea `US-XXX-<AREA>.md`), clasifícalo 🔥/⚡/💡, confírmaselo al participante, y anótalo en `sessions/<slug>/discovery-log.md` (por descripción, nunca con códigos "RN-XX"). Actualiza `completitud_phase` en `sessions/<slug>/SHARED-MEMORY.md` con el área en curso y la cobertura por área.

## Guard de fin de fase

No pases a Gherkin hasta haber recorrido las 10 áreas y que el participante confirme que no ve más huecos evidentes. Marca `completitud_phase.ready_for_handoff: true`.

## Prohibido (confidencialidad)

- No enuncies las reglas tú: pregunta.
- No uses códigos "RN-XX" ni menciones un número total de reglas ("faltan N", "vas X de Y"). No sabes cuántas hay; solo recorres las 10 áreas a fondo.
- No presentes una lista de reglas para marcar — eso es regalar la respuesta. Sondea con preguntas abiertas.
