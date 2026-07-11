# Contexto del proyecto — sesión de Julián

> Esqueleto de contexto que Julián confirma/refina a partir de su caso. Solo lo explícito en el brief; las reglas ocultas se descubren en las fases.

## Caso
Módulo de administración de usuarios, roles y permisos para la plataforma interna de una empresa (CIDENET).

## Contexto (1-2 frases)
Componente de seguridad y gobernanza que permite crear cuentas de usuario, asignarles roles con permisos específicos y controlar quién puede hacer qué dentro del sistema.

## Actores / roles (explícitos en el caso)
- **Admin** — acceso total; gestiona usuarios, roles y permisos.
- **Editor** — permisos de escritura sobre contenido; ve usuarios pero no los gestiona.
- **Viewer** — solo lectura; solo modifica su propio perfil.

## Recursos del sistema
users · roles · permissions · reports (cada uno con acciones C/R/U/D).

## Procesos principales (explícitos)
- Crear / editar / eliminar cuentas de usuario.
- Asignar roles a usuarios.
- Configurar la matriz de permisos por rol.
- Autenticación (implícita: usuario inactivo no puede autenticarse).

## Decisiones de diseño (confirmadas por Julián)
- **1 rol por usuario** (obligatorio). R5 "al menos un rol" se interpreta como *exactamente uno, siempre presente*. Modelo single-role, no multi-role.

## Integraciones
- (Por descubrir.)

## Pantallas (explícitas)
Tabla de usuarios · Formulario crear/editar · Matriz de permisos · Confirmación de eliminación.
