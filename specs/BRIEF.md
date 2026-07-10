# Brief del Ejercicio

Este documento es el punto de partida del taller. Lo que sigue es el brief funcional del módulo que vas a construir. Tu trabajo en la Sesión 1 es transformarlo en una especificación técnica completa (`specs/SPEC.md`) usando Claude Code.

> **Este brief es deliberadamente incompleto.** Tu trabajo es descubrir lo que falta — los edge cases, las validaciones, los flujos negativos — con ayuda de Claude Code. No intentes resolverlo todo leyendo este documento; úsalo como input para `/discovery`.

## 1. Contexto de negocio

Una empresa necesita un módulo de administración de usuarios para su plataforma interna. El módulo permite crear cuentas de usuario, asignarles roles con permisos específicos, y controlar quién puede hacer qué dentro del sistema. Es el componente de seguridad y gobernanza que toda plataforma necesita.

## 2. Roles

El sistema tiene 3 roles predefinidos. No se pueden crear roles nuevos ni eliminar los existentes.

| Rol | Descripción | Alcance general |
|---|---|---|
| Admin | Administrador del sistema | Acceso total. Gestiona usuarios, roles y permisos. |
| Editor | Colaborador con permisos de escritura | Puede crear y editar contenido. Ve usuarios pero no los gestiona. |
| Viewer | Usuario de solo lectura | Solo consulta. No modifica nada excepto su propio perfil. |

## 3. Recursos del sistema

Los permisos se aplican sobre 4 recursos. Cada recurso soporta 4 acciones: crear (C), leer (R), actualizar (U) y eliminar (D).

| Recurso | Descripción |
|---|---|
| users | Cuentas de usuario del sistema |
| roles | Definiciones de roles (admin, editor, viewer) |
| permissions | Configuración de permisos por rol |
| reports | Reportes e informes del sistema |

## 4. Matriz de permisos

Esta es la configuración base de permisos. C = Create, R = Read, U = Update, D = Delete.

| Recurso | Admin | Editor | Viewer |
|---|---|---|---|
| users | CRUD | R | — |
| roles | CRUD | R | — |
| permissions | CRUD | R | — |
| reports | CRUD | CRUD | R |

Nota: Editor y Viewer pueden actualizar su propio perfil (recurso `users`, acción `update`) pero solo su propia cuenta, no la de otros usuarios.

## 5. Pantallas esperadas

El módulo necesita al menos estas 4 pantallas:

- **Tabla de usuarios:** lista paginada con búsqueda por nombre/email, filtro por rol, filtro por estado (activo/inactivo), acciones por fila.
- **Formulario de creación/edición:** nombre, email, contraseña (solo al crear), selector de rol, toggle de estado.
- **Matriz de permisos:** vista de permisos por rol, toggles para activar/desactivar permisos individuales.
- **Confirmación de eliminación:** modal que muestra información del usuario, advertencias si aplica, requiere confirmación explícita.

## 6. Reglas de negocio clave

Estas son las reglas que tu spec DEBE cubrir. Los tests validan que se cumplan.

- No se puede eliminar el último usuario con rol Admin.
- Un usuario no puede modificar su propio rol.
- Un usuario no puede asignarse permisos a sí mismo.
- El email debe ser único en todo el sistema.
- Todo usuario debe tener al menos un rol asignado.
- Un usuario inactivo no puede autenticarse.

## Tu siguiente paso

Abre Claude Code, ejecuta `/discovery`, y usa este brief como input. Claude te va a ayudar a descubrir las validaciones, los endpoints, los edge cases y los criterios de aceptación que faltan. Ese es el ejercicio.
