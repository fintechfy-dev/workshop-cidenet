# US-001-AUD · Registro de auditoría (audit trail)

> **Historia de gap** — descubierta en la fase de Completitud (área 🗃️ AUD).
> `origin: analisis_completitud` · `area: AUD`

**Como** auditor / responsable de gobernanza
**quiero** que toda acción sobre el sistema quede registrada
**para** poder reconstruir después quién hizo qué, sobre quién y cuándo.

## Reglas descubiertas

- **AUD-1 · ⚡ Importante** — El sistema mantiene un registro de auditoría de **todas** las acciones, incluidas las lecturas/consultas. Cada entrada guarda como mínimo: actor (quién), acción realizada, entidad afectada (sobre quién/qué), y marca de tiempo (cuándo).
- **AUD-2 · ⚡ Importante** — El registro de auditoría es **inmutable/append-only**: no se puede editar ni borrar (de lo contrario no sirve para auditar). Sobrevive al soft-delete de las cuentas involucradas.
- **AUD-3 · 💡 Mejora** — Dado que se registran también las lecturas, el volumen puede crecer rápido; el almacenamiento y la retención del log se coordinan con la política de retención de datos (ver MNT) y con el monitoreo (ver MON).

## Alcance (transversal)
Aplica a todas las historias (US-001 … US-007): crear, consultar, editar, eliminar, editar perfil, configurar permisos y autenticar generan entradas de auditoría.

## Clasificación
⚡ Importante — es el propósito declarado del soft-delete y de la gobernanza del módulo.
