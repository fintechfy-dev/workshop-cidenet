# CLAUDE.md

Instrucciones de proyecto para Claude Code en `workshop-cidenet`. Este es el repo del ejercicio del taller AI-First: vas a construir un módulo de usuarios, roles y permisos ejecutando el flujo completo **discovery (BDD 2.0, 5 etapas) → auditoría del discovery → looping engineering (TDD) → auditoría de implementación**.

## El loop de este taller

```
/discovery → entrevista BDD 2.0, 5 etapas, una pregunta a la vez →
             specs/EPIC-001 + specs/historias/ + specs/criterios/*.yaml + features/*.feature + specs/SPEC.md
/plan      → specs/PLAN.md (iteraciones con "Done-when")
/test      → tests desde los Gherkin (tests primero)
/iterate   → implementa hasta que pasen los tests de la iteración, commitea
/audit     → cobertura funcional y técnica: tu propio código vs tu propia spec
```

`/discovery` (o su alias `/spec`) **no genera la spec de una pasada** — te entrevista por las 5 etapas de BDD 2.0 (Épicas VUIFED → Historias INVEST → Criterios SMART → Completitud 10 áreas del ciclo de vida → Gherkin). Al arrancar pregunta tu nombre y crea `sessions/<tu-nombre>/`, donde documenta la sesión y guarda el estado (`SHARED-MEMORY.md`) para reanudar con `/discovery resume`. La mecánica vive en `.claude/skills/bdd-discovery/` (orquestador) + un skill por etapa (`bdd-epicas`, `bdd-historias`, `bdd-criterios`, `bdd-completitud`, `bdd-gherkin`).

Repite `/test` → `/iterate` por cada iteración del plan. El commit de cada iteración pasa por un hook de pre-commit que corre `dotnet format` + `dotnet test` (y `npm test` si tocaste `frontend/`) — si algo falla, el commit se bloquea.

## Punto de partida

`specs/BRIEF.md` es intencionalmente incompleto — es el input real de `/discovery`, no una spec terminada. Tu trabajo es descubrir las validaciones, edge cases y reglas que faltan con ayuda de Claude, no adivinarlas de una sola leída.

## Estructura del repo

- `src/Domain`, `src/Application`, `src/Infrastructure`, `src/Api` — las 4 capas (DDD light). Domain trae las entidades `User`/`Role`/`Permission` con su forma básica; la lógica de negocio (invariantes, validaciones, reglas) la construyes tú.
- `frontend/` — Vite + React + TS, con las 4 pantallas del brief como stubs (`src/pages/`). Conéctalas al API real durante tu iteración de frontend.
- `specs/` — `BRIEF.md` (input), `SPEC.md`/`PLAN.md` y `criterios/*.yaml` (los generas tú con `/discovery` y `/plan`).
- `sessions/` — la documentación de tu sesión de discovery: `sessions/<tu-nombre>/` con `SHARED-MEMORY.md` (estado), `project-context.md`, `discovery-log.md` (bitácora) y `dqs-lite.md`. Se crea al correr `/discovery`.
- `features/` — Gherkin. `ejemplo_formato.feature` es solo referencia de formato (dominio de biblioteca, no el módulo real) — bórralo cuando generes tus propios `.feature`.
- `.claude/agents/` — 4 agentes especializados (arquitecto, backend, frontend, calidad), cada uno enfocado en su capa.
- `.claude/skills/bdd-*` — la máquina de discovery BDD 2.0 (orquestador + un skill por etapa + protocolo de entrevista). `.claude/commands/` — los 5 comandos del loop (`/discovery`+`/spec`, `/plan`, `/test`, `/iterate`, `/audit`).

## Convenciones

- Reglas de negocio, criterios y specs en español; código, nombres de clases/variables en inglés.
- Los criterios de aceptación (Fase 3-equivalente) van primero en YAML — no escribas Gherkin directo desde el brief, pasa por `/discovery`.
- Un commit por iteración, no acumules varias iteraciones en un solo commit — el hook y el review cruzado del sábado esperan ese grano.

## Qué no hacer

- No te saltes `/discovery` para ir directo a código — el ejercicio completo incluye descubrir la spec, no solo implementarla.
- No implementes las 4 pantallas de una sola vez sin tests de componente — sigue el mismo loop de test-primero que en backend.
