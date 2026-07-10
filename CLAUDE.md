# CLAUDE.md

Instrucciones de proyecto para Claude Code en `workshop-cidenet`. Este es el repo del ejercicio del taller AI-First: partiendo de un **caso de negocio que el alumno aporta** (el repo no trae ninguno), construyes el módulo que salga, ejecutando el flujo completo **discovery (BDD 2.0, 5 etapas) → auditoría del discovery → looping engineering (TDD) → auditoría de implementación**.

**El repo no conoce ningún caso de negocio, a propósito.** No asumas ni inventes un dominio. El caso llega del alumno (un documento que carga en el chat al iniciar `/discovery`). Trabaja siempre sobre ese caso.

## El loop de este taller

```
/discovery → entrevista BDD 2.0, 5 etapas, una pregunta a la vez →
             specs/epicas/ + specs/historias/ + specs/criterios/*.yaml + features/*.feature + specs/SPEC.md
/plan      → specs/PLAN.md (iteraciones con "Done-when")
/test      → tests desde los Gherkin (tests primero)
/iterate   → implementa hasta que pasen los tests de la iteración, commitea
/audit     → cobertura funcional y técnica: tu propio código vs tu propia spec
```

`/discovery` (o su alias `/spec`) **no genera la spec de una pasada** — te entrevista por las 5 etapas de BDD 2.0 (Épicas VUIFED → Historias INVEST → Criterios SMART → Completitud 10 áreas del ciclo de vida → Gherkin). Al arrancar pregunta tu nombre y crea `sessions/<tu-nombre>/`, donde documenta la sesión y guarda el estado (`SHARED-MEMORY.md`) para reanudar con `/discovery resume`. La mecánica vive en `.claude/skills/bdd-discovery/` (orquestador) + un skill por etapa (`bdd-epicas`, `bdd-historias`, `bdd-criterios`, `bdd-completitud`, `bdd-gherkin`).

Repite `/test` → `/iterate` por cada iteración del plan. El commit de cada iteración pasa por un hook de pre-commit que corre `dotnet format` + `dotnet test` (y `npm test` si tocaste `frontend/`) — si algo falla, el commit se bloquea.

## Punto de partida

El caso lo aporta el alumno (`sessions/<slug>/caso.md`, guardado al iniciar `/discovery`). Es deliberadamente incompleto: descubrir las validaciones, edge cases y reglas que faltan, con ayuda de Claude, es el ejercicio — no adivinarlas de una sola leída. `specs/BRIEF.md` es solo una nota que explica que el caso lo trae el facilitador.

## Estructura del repo

- `src/Domain`, `src/Application`, `src/Infrastructure`, `src/Api` — las 4 capas (DDD light). Es **plomería sin dominio**: EF/Postgres cableados y un `AppDbContext` vacío. Las entidades, invariantes y reglas las construyes tú a partir de tu spec.
- `frontend/` — Vite + React + TS, con un `App.tsx` mínimo (badge de `/health`). Construyes tus pantallas durante la iteración de frontend.
- `specs/` — `BRIEF.md` (nota: el caso lo trae tu facilitador), `SPEC.md`/`PLAN.md` y `criterios/*.yaml` (los generas tú con `/discovery` y `/plan`).
- `sessions/` — la documentación de tu sesión de discovery: `sessions/<tu-nombre>/` con `SHARED-MEMORY.md` (estado), `project-context.md`, `discovery-log.md` (bitácora) y `dqs-lite.md`. Se crea al correr `/discovery`.
- `features/` — Gherkin. `ejemplo_formato.feature` es solo referencia de formato (dominio de biblioteca, no el módulo real) — bórralo cuando generes tus propios `.feature`.
- `.claude/agents/` — 4 agentes especializados (arquitecto, backend, frontend, calidad), cada uno enfocado en su capa.
- `.claude/skills/bdd-*` — la máquina de discovery BDD 2.0 (orquestador + un skill por etapa + protocolo de entrevista). `.claude/commands/` — los 5 comandos del loop (`/discovery`+`/spec`, `/plan`, `/test`, `/iterate`, `/audit`).

## Convenciones

- Reglas de negocio, criterios y specs en español; código, nombres de clases/variables en inglés.
- Los criterios de aceptación se escriben primero en YAML (etapa Criterios) y el **Gherkin se genera después, en la etapa 5 del discovery**, a partir de esos criterios. Es secuencia, no exclusión: el `.feature` **sí** es el entregable final del discovery — la spec ejecutable que alimenta el TDD. No lo escribas a mano desde el caso; pasa por `/discovery`.
- **TDD puro:** en cada iteración el test se escribe (desde el Gherkin) y se ve fallar **antes** de implementar. Nunca código primero.
- **GitFlow puro:** una rama por feature, **un commit por iteración** con sus tests en verde, PR + review cruzado antes de mergear. No acumules varias iteraciones en un solo commit — el hook y el review del sábado esperan ese grano.

## Qué no hacer

- No te saltes `/discovery` para ir directo a código — el ejercicio completo incluye descubrir la spec, no solo implementarla.
- No implementes varias pantallas de una sola vez sin tests de componente — sigue el mismo loop de test-primero que en backend.
