# EPIC-001 · Administración de usuarios y accesos

## Descripción
Gestión de cuentas de usuario, asignación de roles y configuración de la matriz de permisos como un mismo bloque de funcionalidad. Es el componente de seguridad y gobernanza de la plataforma interna: crear cuentas, asignarles roles con permisos específicos y controlar quién puede hacer qué.

## Scoring VUIFED

| Eje | Score | Nota |
|-----|-------|------|
| 💼 Valor de negocio | 10 | Crítico: sin esto la plataforma no opera de forma segura. |
| 👥 Usuarios | 10 | Impacta a todos los roles, uso diario y constante. |
| 🎯 Impacto | 7 | Muy relevante para el objetivo del sistema. |
| 🔧 Factibilidad técnica | 7 | Viable con algún reto puntual sobre .NET / Postgres / React. |
| 📏 Esfuerzo estimable | 7 | Estimable con algún supuesto. |
| 🔗 Dependencias | 10 | Autónoma, sin bloqueos externos. |
| **Total / Promedio** | **51 / 8.5** | |

## MVP
`mvp_included: true`

**Justificación (Julián):** es el corazón del módulo; el módulo entero es esta épica.
