---
name: bdd-completitud
description: "Fase 4 (clave) del discovery BDD 2.0: autocrítica sistemática recorriendo las 10 áreas del ciclo de vida (CFG/USR/SEC/AUD/MON/INT/MNT/RPT/BCK/TST) para que el participante DESCUBRA lo que su caso no dice. Genera historias de gap US-XXX-AREA clasificadas 🔥/⚡/💡. Invocada por bdd-discovery cuando current_phase = completitud. Una pregunta a la vez; nunca dictar la regla, siempre preguntar. Las sondas son genéricas y se aplican al caso que aportó el alumno."
---

# Fase 🔍 Completitud — Autocrítica por las 10 Áreas del Ciclo de Vida (fase clave del taller)

Sigue el protocolo en `../bdd-discovery/reference/interview-protocol.md`. **La regla de oro aplica al máximo aquí:** haces preguntas que llevan al participante a darse cuenta de lo que falta — **nunca** enuncias la regla tú. Si preguntas "¿qué debería pasar si...?" y él responde con la regla, funcionó. Si tú dices "recuerda que la regla es...", arruinaste el ejercicio.

**El caso lo aportó el alumno** (no hay dominio pre-cargado en el repo). Las sondas de abajo son **genéricas**: adáptalas al caso concreto del alumno — si su caso es de inventario, pregúntalo en términos de inventario; si es de reservas, en términos de reservas. No asumas que el caso es de usuarios/roles ni de ningún dominio en particular.

**Envuelve cada sonda en el FORMATO DE PREGUNTA UNIVERSAL** (encabezado de área, "❓ PREGUNTA X — TIPO", opciones A/B/C + `✍️ Otra`, "⏳ Esperando tu respuesta..."), una a la vez.

**Opciones sin regalar la regla (crítico en esta fase).** Ofrece opciones para que el participante no se quede en blanco, pero:
- Que las opciones sean **alternativas plausibles y balanceadas**, con **largo y detalle parejos** y **ninguna justificada** — que una opción sea más larga o "explicada" es lo que delata cuál es la correcta.
- Encabezado **sin denominador** (`❓ PREGUNTA X`, no "X DE Y"): el número de huecos es desconocido y no debes insinuar un total.
- **No marques cuál es la correcta** ni reacciones como si hubiera una respuesta esperada. Solo registra su elección.
- Ejemplo de patrón (para un límite cualquiera del caso): `A) lo permite normalmente · B) lo permite pero avisa · C) el sistema lo impide · ✍️ Otra`. El participante razona y elige; tú no dices cuál.
- Si una opción enunciaría la regla de forma demasiado directa, déjala como pregunta abierta con ejemplos.

## Objetivo

El caso cubre lo obvio. Recorre las **10 áreas** buscando lo que falta. Por cada hueco que el participante descubra:
1. Si es una regla/validación de una historia existente → agrégala al YAML de esa historia (`validaciones_reglas`, `manejo_errores` o `casos_edge`).
2. Si es funcionalidad nueva que amerita su propia historia → crea una **historia de gap** `specs/historias/US-XXX-<AREA>.md` (ej. `US-001-SEC`), con `origin: analisis_completitud` y el área.
3. **Clasifica cada gap:** 🔥 Crítico (rompe seguridad/integridad; va sí o sí) · ⚡ Importante (debería estar en el MVP) · 💡 Mejora (nice-to-have).

Trabaja **área por área**. No hay un número fijo de reglas a encontrar; la meta es que las 10 áreas queden recorridas. Algunas áreas tendrán varios gaps, otras ninguna — está bien; no todas las áreas aplican a todos los casos.

## Las 10 áreas (sondas genéricas — adáptalas al caso del alumno)

### CFG · ⚙️ Configuración
- "¿Hay parámetros o reglas de tu caso que deberían ser **configurables** en vez de fijos en el código? ¿Quién los cambia y cómo?"

### USR · 👥 Usuarios / acceso
- "¿Tu sistema distingue **quién** puede hacer qué? Si es así, ¿qué pasa en los límites del acceso — el último/único actor de cierto tipo, o alguien que se quede sin permiso?"
- "¿Hay estados de una cuenta/actor (activo, suspendido, etc.) que cambien lo que puede hacer?"

### SEC · 🔒 Seguridad / autorización
- "Para cada acción que **cambia datos**, ¿quién debería poder ejecutarla? ¿Cualquiera, o solo cierto actor?"
- "¿Qué pasa si un actor hace una acción **sobre sí mismo** que cambia sus propios privilegios o su propio estado?"
- "Cuando el sistema devuelve datos en una respuesta, ¿hay algún campo que **nunca** debería salir ahí?"
- "¿Alguna entrada del usuario necesita **reglas de validación fuertes** (formato, longitud, fortaleza) antes de aceptarse?"

### AUD · 🗃️ Auditoría / integridad
- "Al **eliminar** algo, ¿qué debería pasar con lo que dependía de ello?" (abierta — no ofrezcas tú la solución).
- "¿Hay **datos de referencia** en tu caso que no deberían poder modificarse ni eliminarse? ¿Cómo lo garantizas?"
- "Cuando comparas dos valores para decidir si son 'el mismo' (unicidad, duplicados), ¿basta idénticos carácter por carácter, o hay que **normalizar** antes (mayúsculas, espacios, acentos)?"
- "¿Qué acciones deberían quedar **registradas** para poder auditar quién hizo qué y cuándo?"

### MON · 📈 Monitoreo
- "¿Qué de tu sistema querrías poder **observar** en producción (errores, eventos importantes, intentos fallidos)? ¿Algo que debería disparar una alerta?"

### INT · 🔗 Integraciones
- "¿Tu caso necesita hablar con algo **externo** (otro servicio, correo, pagos, un proveedor)? ¿Qué debería pasar si ese externo falla o no responde a tiempo?"

### MNT · 🔧 Mantenimiento
- "Con el tiempo, ¿hay **datos que limpiar o migrar** (registros viejos, cambios de estructura)? ¿Cómo se versiona ese cambio?"

### RPT · 📊 Reportes
- "¿Qué **información agregada o reportes** tendrían sentido para tu caso, y quién puede verlos?"

### BCK · 💾 Backup
- "Si se **pierde o corrompe** la base, ¿qué necesitas para recuperar? ¿Hay datos que no se pueden perder bajo ninguna circunstancia?"

### TST · 🧪 Testing (cierre)
- "Por cada regla que descubriste en las áreas anteriores, ¿tienes un escenario que la **viole a propósito**, no solo el camino feliz? Un test que solo prueba el caso bueno no valida la regla."
- Repasa: ¿cada gap crítico tiene su escenario negativo?

## Registro

Por cada gap: agrégalo al YAML de la historia (o crea `US-XXX-<AREA>.md`), clasifícalo 🔥/⚡/💡, confírmaselo al participante, y anótalo en `sessions/<slug>/discovery-log.md` (por descripción). Actualiza `completitud_phase` en `sessions/<slug>/SHARED-MEMORY.md` con el área en curso y la cobertura.

## Guard de fin de fase

No pases a Gherkin hasta haber recorrido las 10 áreas y que el participante confirme que no ve más huecos evidentes. Marca `completitud_phase.ready_for_handoff: true`.

## Prohibido (confidencialidad)

- No enuncies las reglas tú: pregunta.
- No menciones un número total de reglas ("faltan N", "vas X de Y"). No sabes cuántas hay; solo recorres las 10 áreas a fondo.
- No presentes una lista de reglas para marcar — eso es regalar la respuesta. Sondea con preguntas abiertas + opciones balanceadas.
