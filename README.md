# workshop-cidenet — Taller AI-First: Módulo de Usuarios, Roles y Permisos

Taller práctico de 2 sesiones donde vas a ejecutar, sobre un módulo real, el flujo completo de desarrollo asistido por IA: **discovery (BDD 2.0, 5 etapas) → auditoría del discovery → looping engineering (TDD) → auditoría de implementación**. El producto del taller es el flujo — el módulo es solo el vehículo para practicarlo.

## Qué vas a construir

Un módulo de administración de usuarios con roles y permisos (backend .NET 9 + Postgres, frontend React), a partir de un brief deliberadamente incompleto. Vas a usar Claude Code para descubrir las reglas de negocio que faltan, generar Gherkin, escribir tests antes que el código, implementar, y auditar tu propio trabajo — todo dentro de tu propio fork.

## Agenda

| Sesión | Cuándo | Qué hacen |
|---|---|---|
| Setup | Antes del viernes | Fork + clone + pre-pull de la imagen del devcontainer |
| Sesión 1 | Viernes 5-7pm | Teoría + setup en vivo + arranque de `/discovery` |
| Sesión 2 | Sábado 8am-12pm | Reglas de negocio, endpoints, frontend, Docker, PR y auditoría |

Trabajo individual — cada participante ejecuta el flujo completo en su propio fork.

## Prerrequisitos (antes del viernes)

- [ ] VS Code con la extensión de Claude Code, y suscripción activa.
- [ ] Docker Desktop instalado y corriendo.
- [ ] **GitHub CLI (`gh`)** instalado ([cli.github.com](https://cli.github.com)) y cuenta de GitHub, sin bloqueo de VPN. Autenticas `gh` durante el setup — eso le permite a **Claude pilotar git y GitHub por ti** (fork, commits, PRs), que es como se trabaja en este taller.
- [ ] **Pre-pull de la imagen del devcontainer la noche del viernes** (pesa ~1.7GB):
  ```
  docker pull ghcr.io/fintechfy-dev/workshop-cidenet-devcontainer:latest
  ```

## Quickstart

1. **Clona la versión pública** del repo (es público, no necesitas autenticarte para esto):
   - **VS Code:** `Ctrl/Cmd+Shift+P` → **"Git: Clone"** → pega `https://github.com/fintechfy-dev/workshop-cidenet` → elige carpeta.
   - O con terminal: `git clone https://github.com/fintechfy-dev/workshop-cidenet.git`
2. Abre la carpeta en VS Code y elige **"Reopen in Container"** (usa la imagen pre-publicada, no la reconstruye).
3. Dentro del devcontainer, levanta el stack completo:
   ```
   docker compose up --build
   ```
   Esto levanta tres servicios: `db` (Postgres vacío), `api` (que al arrancar corre las migraciones y **carga el seed de 5 usuarios de prueba**; `http://localhost:5000/health` debe responder `{"status":"ok"}`) y `frontend` (`http://localhost:5173`).
4. **Conéctate a GitHub** desde dentro del contenedor (donde corre Claude), una vez:
   ```
   gh auth login
   ```
   (GitHub.com → HTTPS → login con el navegador). Luego pídele a Claude: *"Haz un fork de este repo a mi cuenta y déjalo como mi origin."* — así Claude puede commitear y abrir tus PRs por ti. (Fallback sin `gh`: haz el fork por la web y agrega tu remoto a mano; Claude no podrá pilotar git.)
5. Abre Claude Code, **carga el documento del caso** que te dieron, y ejecuta tu primer comando:
   ```
   /discovery
   ```
   Esto arranca la **entrevista** de discovery (una pregunta a la vez). Lo primero que hace es preguntarte tu nombre y crear `sessions/<tu-nombre>/`, donde queda **documentada tu sesión de descubrimiento** (contexto, estado, bitácora, reporte de cobertura). Sigue con `/plan`, y luego `/test` → `/iterate` por cada iteración.

## El loop de comandos

```
/discovery → entrevista BDD 2.0 (5 etapas) → specs/EPIC-001 + specs/historias/ + specs/criterios/*.yaml + features/*.feature + specs/SPEC.md
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
.devcontainer/     — imagen de desarrollo (ya publicada en GHCR)
.claude/           — skills (BDD 2.0, 5 etapas), agentes, comandos, hook de pre-commit
src/               — Domain, Application, Infrastructure, Api (DDD light)
tests/             — xUnit
frontend/          — Vite + React + TS
specs/             — BRIEF.md (input), SPEC.md/PLAN.md/criterios (los generas tú)
sessions/          — tu sesión de discovery documentada: sessions/<tu-nombre>/ (se crea al correr /discovery)
features/          — Gherkin (los generas tú; ejemplo_formato.feature es solo referencia)
docker-compose.yml — db + api + frontend
```

## Una nota sobre el brief

`specs/BRIEF.md` está incompleto a propósito. No es un error ni falta de detalle — es el punto de partida real del ejercicio. Si algo no está especificado, es tu trabajo (con ayuda de Claude Code en `/discovery`) descubrirlo, no adivinarlo de una sola leída.
