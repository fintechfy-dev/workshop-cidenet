# SPEC — [Nombre de tu caso]

> Generado por `/discovery` a partir del caso que aportaste. Este documento es la especificación técnica completa: historias, criterios de aceptación y reglas de negocio, ya validados contigo durante la entrevista.

## Historias de usuario

- Como **[rol]** quiero **[acción]** para **[beneficio]**.
- ...

## Reglas de negocio

Lista de todas las reglas descubiertas durante `/discovery` (las explícitas de tu caso + las que salieron de la fase de Completitud). Cada regla en una frase verificable:

- [Regla 1]
- [Regla 2]
- ...

## Endpoints

| Método | Ruta | Descripción | Quién puede ejecutarlo |
|---|---|---|---|
| | | | |

## Pantallas

Para cada pantalla que tu caso necesite: campos, validaciones visibles, estados (carga/error/éxito).

### [Pantalla 1]
- ...

### [Pantalla 2]
- ...

## Criterios de aceptación (YAML)

Ver `specs/criterios/` — un archivo YAML por historia, con el formato de categorías (escenarios_exito, validaciones_reglas, manejo_errores, requisitos_ux, casos_edge) y `validacion_smart`.

## Cobertura (DQS-lite)

Reporte de `/discovery` sobre qué tan cubiertas quedaron las 10 áreas del ciclo de vida (CFG/USR/SEC/AUD/MON/INT/MNT/RPT/BCK/TST) y el balance camino-feliz / camino-negativo. Sin comparar contra un número objetivo — eso lo hacen los facilitadores.
