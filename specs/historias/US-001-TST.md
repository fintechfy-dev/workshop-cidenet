# US-001-TST · Política de cobertura de pruebas

> **Historia de gap** — definida en la fase de Completitud (área 🧪 TST, cierre).
> `origin: analisis_completitud` · `area: TST`

**Como** responsable de calidad
**quiero** que cada regla descubierta tenga pruebas que la violen a propósito
**para** garantizar que las reglas se cumplen, no solo que el camino feliz funciona.

## Política

- **TST-1 · 🔥 Crítico** — Cada regla clasificada 🔥 Crítica y ⚡ Importante tiene, además del escenario de éxito, **al menos un escenario negativo**: el intento prohibido que el sistema debe rechazar (con el mensaje/comportamiento esperado).
- **TST-2 · ⚡ Importante** — La cobertura busca ser **lo más exhaustiva posible** (decisión explícita de Julián): validaciones de campo, límites, autorización (peticiones directas al backend saltándose la UI), concurrencia donde aplique, y reglas de negocio (R1–R6 y las derivadas).
- **TST-3** — Estos escenarios se materializan en la Etapa 5 (Gherkin) como `Scenario` negativos junto a los positivos, y de ahí alimentan el TDD (`/test` → `/iterate`).

## Reglas 🔥/⚡ que exigen escenario negativo (resumen)
- SEC: contraseña nunca en respuestas · hash con salt · autorización en backend (peticiones directas rechazadas).
- Negocio: no eliminar el último Admin (R1) · no quitar rol Admin al último Admin · no autoeliminarse · no cambiar el propio rol (R2) · no auto-asignarse permisos (R3) · email único absoluto (R4) · rol obligatorio (R5) · inactivo no autentica (R6).
- USR: sesión cortada de inmediato al desactivar.
- AUD: soft-delete (no se borra físicamente) · reactivación al recrear email eliminado · audit trail inmutable.
- MNT: Admin seed + cambio de contraseña forzado al primer acceso.
- Login: mensaje genérico anti-enumeración.

## Clasificación
🔥 Crítico — sin cobertura negativa, las reglas descubiertas no quedan realmente garantizadas.
