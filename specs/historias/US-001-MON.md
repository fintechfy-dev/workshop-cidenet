# US-001-MON · Observabilidad y alertas

> **Historia de gap** — descubierta en la fase de Completitud (área 📈 MON).
> `origin: analisis_completitud` · `area: MON`

**Como** equipo de operación
**quiero** observar eventos críticos del módulo y recibir alertas cuando ocurran
**para** reaccionar a tiempo ante ataques, riesgos de gobernanza o fallos operativos.

## Reglas descubiertas

- **MON-1 · ⚡ Importante** — Se observa y alerta ante un **pico de inicios de sesión fallidos** (posible ataque de fuerza bruta). Conecta con la protección anti-fuerza-bruta pendiente en US-007 (rate limiting) y con el audit trail (US-001-AUD).
- **MON-2 · ⚡ Importante** — Se alerta cuando el sistema queda con **muy pocos (o un solo) Admin activo**, como señal temprana del riesgo que R1 previene (quedarse sin administradores).
- **MON-3 · ⚡ Importante** — Se observan y alertan los **errores del backend** en las operaciones de crear/editar/eliminar (fallos de la operación misma), para detectar degradación del servicio.

## Clasificación
⚡ Importante — operabilidad y detección temprana. El mecanismo concreto (logs estructurados, métricas, umbrales de alerta) se define en implementación.
