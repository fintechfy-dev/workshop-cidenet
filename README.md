# workshop-cidenet — Taller AI-First: Módulo de Usuarios, Roles y Permisos

Taller práctico de 2 sesiones donde vas a ejecutar, sobre un módulo real, el flujo completo de desarrollo asistido por IA: **discovery (BDD 2.0 Lite) → auditoría del discovery → looping engineering (TDD) → auditoría de implementación**. El producto del taller es el flujo — el módulo es solo el vehículo para practicarlo.

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
- [ ] Cuenta de GitHub, sin bloqueo de VPN hacia GitHub.
- [ ] **Fork de este repo** hecho con anticipación.
- [ ] **Pre-pull de la imagen del devcontainer la noche del viernes** (pesa ~1.7GB):
  ```
  docker pull ghcr.io/fintechfy-dev/workshop-cidenet-devcontainer:latest
  ```

## Quickstart

1. **Fork** este repo (botón "Fork" arriba a la derecha en GitHub).
2. Clona tu fork:
   ```
   git clone https://github.com/<tu-usuario>/workshop-cidenet.git
   cd workshop-cidenet
   ```
3. Abre la carpeta en VS Code y elige **"Reopen in Container"** (usa la imagen pre-publicada, no la reconstruye).
4. Dentro del devcontainer, levanta el stack completo:
   ```
   docker compose up --build
   ```
   Esto levanta `db` (Postgres, con el seed de 5 usuarios de prueba ya cargado), `api` (`http://localhost:5000/health` debe responder `{"status":"ok"}`) y `frontend` (`http://localhost:5173`).
5. Abre Claude Code y ejecuta tu primer comando:
   ```
   /discovery
   ```
   Esto lee `specs/BRIEF.md` y arranca el ciclo. Sigue con `/plan`, y luego `/test` → `/iterate` por cada iteración.

## El loop de comandos

```
/discovery → specs/SPEC.md + criterios YAML + features/*.feature
/plan      → specs/PLAN.md (iteraciones con "Done-when")
/test      → tests desde Gherkin (tests primero)
/iterate   → implementa hasta que pasen, commitea
/audit     → cobertura funcional y técnica de tu propia spec vs tu propio código
```

Cada commit pasa por un hook de pre-commit (`dotnet format` + `dotnet test`, y `npm test` si tocaste frontend) — si algo falla, el commit se bloquea hasta corregirlo.

## Gitflow del taller

```
fork → clone → feature/<tu-nombre> → commits por iteración → PR contra main de tu fork → review cruzado → merge
```

## Estructura del repo

```
.devcontainer/     — imagen de desarrollo (ya publicada en GHCR)
.claude/           — agentes, comandos, hook de pre-commit
src/               — Domain, Application, Infrastructure, Api (DDD light)
tests/             — xUnit
frontend/          — Vite + React + TS
specs/             — BRIEF.md (input), SPEC.md/PLAN.md (los generas tú)
features/          — Gherkin (los generas tú; ejemplo_formato.feature es solo referencia)
docker-compose.yml — db + api + frontend
```

## Una nota sobre el brief

`specs/BRIEF.md` está incompleto a propósito. No es un error ni falta de detalle — es el punto de partida real del ejercicio. Si algo no está especificado, es tu trabajo (con ayuda de Claude Code en `/discovery`) descubrirlo, no adivinarlo de una sola leída.
