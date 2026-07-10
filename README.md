# workshop-cidenet — Taller AI-First

Taller práctico de 2 sesiones donde vas a ejecutar, sobre un caso de negocio real, el flujo completo de desarrollo asistido por IA: **discovery (BDD 2.0, 5 etapas) → auditoría del discovery → looping engineering (TDD) → auditoría de implementación**. El producto del taller es el flujo — el caso es solo el vehículo para practicarlo.

> **El repo no incluye ningún caso de negocio a propósito.** Es una plantilla reutilizable: tu facilitador te entrega el caso como un documento aparte, que cargas en el chat al iniciar el discovery. Así el mismo taller sirve para cualquier caso, y el descubrimiento es real (ni el repo ni Claude entran sabiendo las respuestas).

## Qué vas a construir

El módulo que salga del caso que te entreguen (backend .NET 9 + Postgres, frontend React), partiendo de un scaffold que trae la **plomería** lista (4 capas compilando, EF+Postgres, Docker, `/health` + test) pero **sin dominio** — el dominio lo construyes tú. Vas a usar Claude Code para descubrir la spec desde el caso, generar Gherkin, escribir tests antes que el código, implementar, y auditar tu propio trabajo — todo dentro de tu propio fork.

## Agenda

| Sesión | Cuándo | Qué hacen |
|---|---|---|
| Setup | Antes del viernes | Instalar prerequisitos + clone + primer `docker compose up` local |
| Sesión 1 | Viernes 5-7pm | Teoría + setup en vivo + arranque de `/discovery` |
| Sesión 2 | Sábado 8am-12pm | Reglas de negocio, endpoints, frontend, Docker, PR y auditoría |

Trabajo individual — cada participante ejecuta el flujo completo en su propio fork.

## Prerrequisitos (antes del viernes)

- [ ] VS Code con la extensión de Claude Code, y suscripción activa.
- [ ] **Docker Desktop** instalado y corriendo (corre la app con `docker compose`).
- [ ] **.NET 9 SDK** y **Node.js 22** instalados localmente (para el loop de TDD: `dotnet test`, `npm test`).
- [ ] **GitHub CLI (`gh`)** instalado ([cli.github.com](https://cli.github.com)) y cuenta de GitHub, sin bloqueo de VPN. Autenticas `gh` durante el setup — eso le permite a **Claude pilotar git y GitHub por ti** (fork, commits, PRs), que es como se trabaja en este taller.

> Todo corre **local**: la app en `docker compose` (Docker baja Postgres y construye api/frontend), y el loop de desarrollo con tu SDK/Node local. No hay que descargar ninguna imagen de entorno aparte.

## Quickstart

1. **Clona la versión pública** del repo (es público, no necesitas autenticarte para esto):
   - **VS Code:** `Ctrl/Cmd+Shift+P` → **"Git: Clone"** → pega `https://github.com/fintechfy-dev/workshop-cidenet` → elige carpeta.
   - O con terminal: `git clone https://github.com/fintechfy-dev/workshop-cidenet.git`
2. Abre la carpeta en VS Code. Levanta el stack completo (local):
   ```
   docker compose up --build
   ```
   La primera vez, Docker baja Postgres y construye las imágenes de api/frontend (tarda un poco; es una sola vez). Levanta tres servicios: `db` (Postgres vacío), `api` (`http://localhost:5000/health` debe responder `{"status":"ok"}`) y `frontend` (`http://localhost:5173`). La base arranca vacía — tú creas tu esquema con migraciones cuando modeles tu dominio.
3. **Conéctate a GitHub** una vez:
   ```
   gh auth login
   ```
   (GitHub.com → HTTPS → login con el navegador). Luego pídele a Claude: *"Haz un fork de este repo a mi cuenta y déjalo como mi origin."* — así Claude puede commitear y abrir tus PRs por ti. (Fallback sin `gh`: haz el fork por la web y agrega tu remoto a mano; Claude no podrá pilotar git.)
4. Abre Claude Code, **carga el documento del caso** que te dieron, y ejecuta tu primer comando:
   ```
   /discovery
   ```
   Esto arranca la **entrevista** de discovery (una pregunta a la vez). Lo primero que hace es preguntarte tu nombre y crear `sessions/<tu-nombre>/`, donde queda **documentada tu sesión de descubrimiento** (contexto, estado, bitácora, reporte de cobertura). Sigue con `/plan`, y luego `/test` → `/iterate` por cada iteración.

## El loop de comandos

```
/discovery → entrevista BDD 2.0 (5 etapas) → specs/epicas/ + specs/historias/ + specs/criterios/*.yaml + features/*.feature + specs/SPEC.md
/plan      → specs/PLAN.md (iteraciones con "Done-when")
/test      → tests desde Gherkin (tests primero)
/iterate   → implementa hasta que pasen, commitea
/audit     → cobertura funcional y técnica: tu propio código vs tu propia spec
```

`/discovery` (alias `/spec`) te entrevista por las 5 etapas de BDD 2.0 (Épicas VUIFED → Historias INVEST → Criterios SMART → Completitud 10 áreas → Gherkin) en vez de generar la spec de golpe; documenta tu sesión en `sessions/<tu-nombre>/` (incluido el estado) y se retoma con `/discovery resume`. **Su entregable final son los `features/*.feature` (Gherkin): tu spec ejecutable, y el punto de partida del desarrollo.**

Terminado el discovery, el desarrollo es **iterativo, con TDD puro y GitFlow puro**: `/plan` divide la spec en iteraciones pequeñas; por cada una, `/test` escribe los tests **primero** (desde tu Gherkin) y `/iterate` implementa lo mínimo para que pasen y commitea. `/audit` cierra validando cobertura. Cada commit pasa por un hook de pre-commit (`dotnet format` + `dotnet test`, y `npm test` si tocaste frontend) — si algo falla, el commit se bloquea.

## Gitflow del taller (puro)

```
fork → clone → feature/<tu-nombre> → 1 commit por iteración (tests en verde) → PR → review cruzado → merge
```

**Reglas duras:** el test va **antes** del código (TDD puro), y **un commit = una iteración** con sus tests pasando (GitFlow puro). Claude pilotea git por ti (por eso autenticas `gh` en el setup): crea la rama, commitea cada iteración y abre tu PR.

## Estructura del repo

```
.claude/           — skills (BDD 2.0, 5 etapas), agentes, comandos, hook de pre-commit
src/               — Domain, Application, Infrastructure, Api (DDD light, plomería sin dominio)
tests/             — xUnit
frontend/          — Vite + React + TS (App mínimo; construyes tus pantallas)
specs/             — BRIEF.md (nota: el caso lo trae tu facilitador), SPEC.md/PLAN.md/criterios (los generas tú)
sessions/          — tu sesión de discovery documentada: sessions/<tu-nombre>/ (se crea al correr /discovery)
features/          — Gherkin (los generas tú; ejemplo_formato.feature es solo referencia)
docker-compose.yml — db + api + frontend
```

## Una nota sobre el caso

El repo **no trae ningún caso de negocio** — a propósito (ver `specs/BRIEF.md`). Tu facilitador te lo entrega como un documento aparte; lo cargas en el chat al iniciar `/discovery`. El caso es deliberadamente incompleto: descubrir lo que falta, con ayuda de Claude Code, es el ejercicio.
