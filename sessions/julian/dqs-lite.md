# Reporte DQS-lite — Discovery de Julián

> Auditoría del propio discovery: qué tan sólida quedó la cobertura por área y el balance camino-feliz / camino-negativo. No hay un número objetivo; la meta es que cada área se sienta bien explorada.

## Cobertura por las 10 áreas del ciclo de vida

- **⚙️ CFG · Configuración** — Explorada. Un gap (💡): tamaño de página configurable. La política de contraseña y la matriz base quedan fijas por decisión (la matriz se ajusta en runtime vía US-006). Cobertura suficiente para el MVP.
- **👥 USR · Usuarios / acceso** — Explorada. Gap (⚡): al desactivar, la sesión activa se corta de inmediato. Se decidió no agregar más estados de cuenta (activo/inactivo bastan). Sólida.
- **🔒 SEC · Seguridad** — **Bien cubierta, el área más rica.** Tres reglas 🔥 críticas: contraseña nunca en respuestas, hash con salt (Argon2id/bcrypt), autorización validada en el backend por petición. Complementa a R2/R3 y a la política de contraseña. Sin gaps críticos sin resolver.
- **🗃️ AUD · Auditoría / integridad** — Bien cubierta. Soft-delete (nada se borra por defecto), reactivación al recrear un email eliminado, audit trail integral e inmutable, retención 12/24 meses. Coherente con el objetivo de gobernanza.
- **📈 MON · Monitoreo** — Explorada. Tres alertas (⚡): fuerza bruta, pocos/un solo Admin, errores de backend. El mecanismo concreto se define en implementación.
- **🔗 INT · Integraciones** — Explorada y cerrada: el módulo es autónomo en el MVP, sin dependencias externas.
- **🔧 MNT · Mantenimiento** — Bien cubierta. Admin sembrado (🔥, resuelve el arranque en base vacía), cambio de contraseña forzado al primer acceso, retención/purga, migraciones EF.
- **📊 RPT · Reportes** — Explorada y cerrada: fuera del MVP, coherente con dejar `reports` como solo-permiso.
- **💾 BCK · Backup** — Explorada: delegado a la infraestructura/plataforma en el MVP; sin estrategia a nivel de módulo (decisión consciente).
- **🧪 TST · Testing** — Explorada y definida como política: cobertura negativa **exhaustiva** — cada regla 🔥/⚡ tiene su escenario que la viola.

## Balance camino-feliz / camino-negativo

Sólido. Cada `.feature` incluye escenarios de éxito y escenarios negativos por regla:
- Validaciones de campo con `Scenario Outline` (contraseña, longitud de nombre, búsqueda).
- Autorización probada con peticiones directas al backend (no solo la UI) para Viewer/Editor.
- Reglas de negocio R1–R6 y sus derivadas, cada una con su intento prohibido.
- Reglas transversales (no exponer contraseña, hashing, audit trail inmutable, soft-delete, seed) con su escenario dedicado.

No quedan reglas 🔥/⚡ sin al menos un escenario negativo.

## Huecos / observaciones honestas

- **BCK y RPT** quedaron cerradas por decisión de alcance (delegadas/fuera del MVP), no por exploración profunda. Si el proyecto crece, conviene revisitarlas — no es un reproche, es dónde volver si hace falta.
- Varias reglas de **MON** y la **retención (MNT-3)** describen el *qué* pero dejan el *cómo* (umbrales, mecanismo) para implementación: correcto para esta etapa.
- La verificación de **hash** y el **rate limiting** de login viven como reglas; su implementación concreta se aterriza en el loop TDD.

## Veredicto

El discovery cubrió las 10 áreas con foco fuerte en Seguridad, Auditoría y Mantenimiento —las críticas para un módulo de gobernanza de accesos—. La especificación ejecutable está lista para alimentar `/plan` y el TDD.
