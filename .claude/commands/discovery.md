---
description: "BDD 2.0 Lite: convierte specs/BRIEF.md en specs/SPEC.md + criterios YAML + features/*.feature, con completitud enfocada en Seguridad/Auditoría/Usuarios/Testing."
---

Actúa como el agente `arquitecto` (ver `.claude/agents/arquitecto.md`) y ejecuta el discovery completo sobre `specs/BRIEF.md`:

1. **Historias de usuario** a partir de los roles y procesos del brief. No repitas la épica (ya está dada: el módulo de usuarios).
2. **Criterios de aceptación SMART en YAML** por historia, guardados en `specs/criterios/<historia>.yaml`, con este formato:

```yaml
story_id: "..."
titulo_historia: "Como [rol] quiero [acción] para [beneficio]"
criterios_aceptacion:
  escenarios_exito: []
  validaciones_reglas: []
  manejo_errores: []
  requisitos_ux: []
  casos_edge: []
validacion_smart:
  specific: false
  measurable: false
  achievable: false
  relevant: false
  testable: false
```

   **No escribas Given-When-Then todavía** — eso es el paso 4.

3. **Completitud enfocada.** Antes de marcar `validacion_smart` como completa, revisa cada historia contra estas 4 áreas (pregúntate esto, no asumas que el brief ya lo cubrió):
   - **Seguridad:** ¿quién puede ejecutar esta acción? ¿alguna acción restringida a un solo rol? ¿qué pasa con acciones sobre uno mismo? ¿algún campo que nunca debería exponerse?
   - **Auditoría:** ¿relaciones dependientes al eliminar? ¿datos de referencia que no deberían modificarse/eliminarse? ¿algo que normalizar antes de comparar?
   - **Usuarios (lifecycle):** ¿límites — el último, el único? ¿desactivar es igual a eliminar? ¿qué invariante nunca se rompe?
   - **Testing:** ¿cada regla tiene un escenario que la viola a propósito?

   Si al revisar una historia contra estas preguntas surge una regla nueva, agrégala a `validaciones_reglas` o `casos_edge` según corresponda, y siéntete libre de crear historias nuevas si el gap lo amerita (ej. una historia de auditoría o de seguridad que no estaba en el brief original).

4. **Gherkin.** Para cada historia con `validacion_smart` completa, genera el `.feature` correspondiente en `features/` siguiendo el formato de `features/ejemplo_formato.feature` (tags `@story_id`, `@priority`, `@complexity`, Background, Scenario + Scenario Outline donde aplique).

5. **specs/SPEC.md.** Consolida historias + reglas + un borrador de endpoints y pantallas usando `specs/SPEC.template.md` como formato.

6. **DQS-lite.** Cierra con un reporte: qué tan cubierta quedó cada una de las 4 áreas de completitud, y si el balance camino-feliz/camino-negativo es razonable. No dediques tiempo a inventar un "score" numérico contra un total — no hay un total conocido; el objetivo es que la cobertura se sienta sólida, no completar un porcentaje.
