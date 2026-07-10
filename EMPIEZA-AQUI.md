# 🚀 Empieza aquí — Workshop AI-First

Vas a construir un módulo de software **real** sin escribir el código a mano: dirigiendo a Claude paso a paso. Este taller es **AI-first** — Claude maneja git y GitHub por ti (fork, commits, PRs); tú piensas y decides.

## ✅ Antes de empezar (una sola vez)

- VS Code con la extensión **Claude Code** (y tu suscripción activa)
- **Docker Desktop** abierto y corriendo
- **GitHub CLI (`gh`)** instalado → [cli.github.com](https://cli.github.com)
- Una cuenta de **GitHub**

## 1 · Trae el repo (es público, no necesitas permisos)

Clona la versión pública del repo:

- **VS Code:** `Ctrl/Cmd + Shift + P` → **"Git: Clone"** → pega `https://github.com/fintechfy-dev/workshop-cidenet` → elige una carpeta.
- ¿Prefieres terminal? `git clone https://github.com/fintechfy-dev/workshop-cidenet.git`

## 2 · Abre el entorno (viene todo listo)

1. Abre la carpeta en VS Code → cuando aparezca el aviso, **"Reopen in Container"**.
   > La primera vez baja una imagen (~1.7 GB). Si la pre-descargaste, es instantáneo.
2. Levanta la app:
   ```bash
   docker compose up --build
   ```
3. Confirma que respira:
   - **API** → http://localhost:5000/health debe responder `{"status":"ok"}`
   - **App** → http://localhost:5173

## 3 · Conéctate a GitHub (para que Claude pilotee por ti)

Ya estás dentro del contenedor — que es donde Claude trabaja. Autentícalo con GitHub **una vez**:

```bash
gh auth login
```

Elige **GitHub.com → HTTPS → login con el navegador** y pega el código que te muestra.

Ahora pídele a Claude que prepare tu copia:

> «Haz un fork de este repo a mi cuenta de GitHub y déjalo como mi `origin`, para poder commitear y abrir mi PR.»

Claude hace el fork y conecta tu copia. De aquí en adelante, cuando avances, **Claude commitea y abre tus PRs por ti**.

## 4 · Arranca tu discovery 🎤

Aquí empieza lo bueno. Claude te va a **entrevistar**, una pregunta a la vez, para convertir el caso de negocio en una especificación técnica. Te dará opciones para responder — y siempre puedes escribir la tuya si ninguna encaja.

1. En Claude Code, **carga el documento del caso** que te entregamos (arrástralo o pégalo).
2. Justo después, escribe:
   ```
   /discovery
   ```
3. Claude te pedirá tu nombre (para guardar tu sesión) y arrancará la entrevista. **Responde con lo que sepas** — no necesitas tenerlo todo claro; para eso es la conversación. Si algo no lo sabes, dilo y sigue.

> 💡 ¿Necesitas parar? Retomas con `/discovery resume`. Tu avance queda guardado en `sessions/tu-nombre/`.

## 🧭 Reglas del juego

- **Tú decides, Claude ejecuta.** Lee lo que produce y corrígelo — nada de aceptar a ciegas.
- **Responde honesto.** Las mejores specs salen de tus dudas, no de fingir que ya lo sabes todo.
- **Sin prisa por el código.** Primero la especificación; el código llega después, guiado por tus tests.
- **¿Atascado más de 5 minutos?** Llama a un facilitador. Para eso estamos.

Al cerrar el taller vas a tener un módulo funcionando, con tests, que **especificaste y construiste dirigiendo IA**. 🙌
