# US-001-SEC · Manejo seguro de credenciales

> **Historia de gap** — descubierta en la fase de Completitud (área 🔒 SEC).
> `origin: analisis_completitud` · `area: SEC`

**Como** responsable de seguridad del sistema
**quiero** que las credenciales de los usuarios se manejen de forma segura de extremo a extremo
**para** que una fuga de datos o un endpoint indebido nunca expongan contraseñas.

## Reglas descubiertas

- **SEC-1 · 🔥 Crítico** — La contraseña **nunca** aparece en ninguna respuesta del backend, en ninguna forma (ni texto plano, ni enmascarada, ni hash). Ningún endpoint (listar usuarios, ver/editar perfil, detalle de usuario, etc.) incluye el campo contraseña en su salida.
- **SEC-2 · 🔥 Crítico** — La contraseña se almacena con un **hash de un solo sentido con salt** usando un algoritmo moderno de hashing de contraseñas: **Argon2id** (preferido) o **bcrypt** (alternativa). Nunca en texto plano ni con cifrado reversible. El salt es único por usuario. Al autenticar (US-007) se recomputa el hash y se compara; el original nunca se recupera.
- **SEC-3 · 🔥 Crítico** — La autorización se valida **en el backend en cada petición** que cambia datos o accede a recursos protegidos. Ocultar/deshabilitar controles en la UI no es suficiente: aunque una petición esquive la pantalla y llegue directa al endpoint, el servidor vuelve a verificar el rol/permiso del solicitante y rechaza (403) si no está autorizado. La UI es conveniencia; el backend es la frontera de seguridad.

## Alcance (transversal)
Aplica a todas las historias que devuelven datos de usuario: US-002 (tabla), US-003 (editar), US-005 (perfil propio), US-007 (autenticación).

## Clasificación
🔥 Crítico — su incumplimiento rompe la seguridad del sistema. Va sí o sí en el MVP.
