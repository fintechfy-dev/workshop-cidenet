# Sesiones de Discovery

Cada alumno documenta aquí su sesión de descubrimiento, siguiendo la convención de BDD 2.0: un discovery = una carpeta de sesión.

Cuando corras `/discovery` (o `/spec`) por primera vez, el skill te pregunta tu nombre y crea automáticamente:

```
sessions/<tu-nombre>/
├── README.md            — qué es esta sesión, quién la corrió, estado
├── SHARED-MEMORY.md     — el estado del discovery (fase actual, respuestas) — permite reanudar con /discovery resume
├── project-context.md   — el contexto del módulo que vas confirmando (roles, recursos, pantallas)
├── discovery-log.md     — bitácora: qué se cubrió en cada fase, decisiones, huecos que descubriste
└── dqs-lite.md          — reporte de cobertura al cerrar (se genera en la fase de Gherkin)
```

Esta carpeta es **tu artefacto de trabajo**: commitéala junto con tus specs. Es la constancia de tu proceso de descubrimiento, no solo del resultado.

> Los artefactos que el resto del loop consume (`specs/historias/`, `specs/criterios/`, `specs/SPEC.md`, `features/`) se guardan en su ubicación de siempre en la raíz — la carpeta de sesión documenta el **cómo** llegaste ahí.
