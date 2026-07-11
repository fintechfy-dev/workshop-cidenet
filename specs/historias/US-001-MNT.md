# US-001-MNT · Arranque y mantenimiento de datos

> **Historia de gap** — descubierta en la fase de Completitud (área 🔧 MNT).
> `origin: analisis_completitud` · `area: MNT`

**Como** operador del sistema
**quiero** un arranque bien definido y un mantenimiento de datos en el tiempo
**para** que el sistema sea usable desde el despliegue y sostenible a largo plazo.

## Reglas descubiertas

- **MNT-1 · 🔥 Crítico** — El sistema se despliega con un **Admin inicial sembrado (seed)**. Sin él, nadie podría crear el primer usuario (solo Admin puede). Resuelve el arranque en base vacía.
- **MNT-2 · ⚡ Importante** — Las credenciales del Admin inicial **deben cambiarse en el primer acceso**: hasta que no se cambie la contraseña sembrada, el sistema fuerza el cambio antes de permitir operar. (Aplica también como buena práctica a cualquier cuenta creada con contraseña provisional.)
- **MNT-3 · 💡 Mejora** — Proceso de **retención/purga** de datos: un job posterior (configurable, p. ej. 12 o 24 meses) puede purgar físicamente los registros con soft-delete antiguos (ver US-004-EDGE2). No forma parte del flujo de eliminación por defecto.
- **MNT-4 · 💡 Mejora** — Los cambios de estructura de la base se versionan mediante **migraciones** (EF Core migrations), para evolucionar el esquema de forma controlada.

## Clasificación
Mixta: MNT-1 es 🔥 (sin seed no se puede usar el sistema); el resto ⚡/💡.
