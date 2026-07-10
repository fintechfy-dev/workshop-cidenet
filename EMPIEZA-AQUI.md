# 🚀 Empieza aquí — Workshop AI-First

Vas a construir un módulo de software **real** sin escribir el código a mano: dirigiendo a Claude paso a paso. Es **AI-first** — tú piensas y decides, Claude ejecuta.

> El repo **no incluye ningún caso de negocio**. Tu facilitador te lo entrega como un documento aparte, que cargas en el chat al iniciar el discovery.

## 📅 Cómo se divide

- **Día 1 — Discovery:** conviertes el caso en tu especificación, entrevistado por Claude. **Setup mínimo, cero infra.**
- **Día 2 — Construcción (TDD + GitFlow):** levantas la infraestructura y construyes el módulo por iteraciones.

---

# DÍA 1 — Discovery 🎤

## ✅ Solo esto para hoy

- VS Code con la extensión **Claude Code** (y tu suscripción activa).
- El repo en tu máquina (Paso 1).

> Docker, .NET SDK, Node y `gh` los necesitas **mañana**, no hoy. Hoy solo descubrimos.

## 1 · Trae el repo (es público)

Clona la versión pública del repo:

- **VS Code:** `Ctrl/Cmd + Shift + P` → **"Git: Clone"** → pega `https://github.com/fintechfy-dev/workshop-cidenet` → elige una carpeta.
- O con terminal: `git clone https://github.com/fintechfy-dev/workshop-cidenet.git`

## 2 · Arranca tu discovery

Claude te va a **entrevistar**, una pregunta a la vez, para convertir tu caso en una especificación técnica. Te dará opciones para responder — y siempre puedes escribir la tuya.

1. Abre Claude Code en la carpeta del repo.
2. **Carga en el chat el documento del caso** que te entregamos (arrástralo o pégalo).
3. Escribe:
   ```
   /discovery
   ```
4. Claude te pedirá tu nombre y tu caso, y arrancará la entrevista. **Responde con lo que sepas** — no necesitas tenerlo todo claro; para eso es la conversación.

> 💡 **Hoy no necesitas commitear.** Tu discovery queda guardado en archivos (`sessions/tu-nombre/`). Si paras a mitad, retomas con `/discovery resume`. Mañana configuras git y empiezas a versionar.

Al cerrar el Día 1 tendrás tu **spec ejecutable** (`features/*.feature`) lista para construir mañana.

---

# DÍA 2 — Construcción: 🧪 TDD puro + 🌳 GitFlow puro

## ✅ Instálalo la noche del viernes

- **Docker Desktop** (corre la app con `docker compose`).
- **.NET 9 SDK** y **Node.js 22** (para los tests del TDD).
- **GitHub CLI (`gh`)** → [cli.github.com](https://cli.github.com).

## 3 · Prepara git e infra (tu primera iteración)

1. Autentica `gh` una vez (así **Claude pilotea git y GitHub por ti** — fork, commits, PRs):
   ```
   gh auth login
   ```
   (GitHub.com → HTTPS → login con el navegador). Luego pídele a Claude: *"Haz un fork de este repo a mi cuenta y déjalo como mi origin."*
2. **Levanta la infraestructura** — puede ser tu primera iteración del plan:
   ```
   docker compose up --build
   ```
   Baja Postgres y construye api/frontend. Verifica: **API** → http://localhost:5000/health responde `{"status":"ok"}` · **App** → http://localhost:5173. (La base arranca vacía; tú creas tu esquema con migraciones cuando modeles tu dominio.)

## 4 · Construye por iteraciones

- **🧪 TDD puro:** el test va **siempre antes** del código, escrito desde tu Gherkin, se ve fallar, y luego implementas lo mínimo para que pase.
- **🌳 GitFlow puro:** una rama por feature, **un commit por iteración** (con tests en verde), un PR, review cruzado, y merge.

El loop, con Claude piloteando:

1. `/plan` → Claude divide tu spec en iteraciones con un "listo cuando…" concreto (la **primera puede ser la infra** del Paso 3).
2. Por cada iteración: `/test` (tests primero) → `/iterate` (código + commit).
3. `/audit` → revisa que tu código cubra tu spec.

## 🧭 Reglas del juego

- **Tú decides, Claude ejecuta.** Lee lo que produce y corrígelo — nada de aceptar a ciegas.
- **Responde honesto.** Las mejores specs salen de tus dudas, no de fingir que ya lo sabes todo.
- **Sin prisa por el código.** Primero la especificación; el código llega después, guiado por tus tests.
- **¿Atascado más de 5 minutos?** Llama a un facilitador. Para eso estamos.

Al cerrar el taller vas a tener un módulo funcionando, con tests, que **especificaste y construiste dirigiendo IA**. 🙌
