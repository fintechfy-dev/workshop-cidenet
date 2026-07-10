#!/usr/bin/env bash
# PreToolUse hook sobre Bash: si el comando es un `git commit`, corre lint+tests
# antes de dejarlo pasar. Bloquea el commit (exit 2) si algo falla.
set -euo pipefail

INPUT=$(cat)
COMMAND=$(echo "$INPUT" | jq -r '.tool_input.command // empty')

if ! echo "$COMMAND" | grep -qE '(^|;|&&|\|)\s*git\s+commit'; then
  exit 0
fi

REPO_ROOT=$(git rev-parse --show-toplevel 2>/dev/null || pwd)
cd "$REPO_ROOT"

echo "[pre-commit-check] Verificando formato y tests antes del commit..." >&2

if ! dotnet format --verify-no-changes >&2; then
  echo "[pre-commit-check] BLOQUEADO: hay archivos .NET sin formatear. Corre 'dotnet format' y vuelve a intentar." >&2
  exit 2
fi

if ! dotnet test >&2; then
  echo "[pre-commit-check] BLOQUEADO: hay tests de backend fallando." >&2
  exit 2
fi

if git diff --cached --name-only | grep -q '^frontend/'; then
  if ! (cd frontend && npm test) >&2; then
    echo "[pre-commit-check] BLOQUEADO: hay tests de frontend fallando." >&2
    exit 2
  fi
fi

echo "[pre-commit-check] OK — tests en verde, commit permitido." >&2
exit 0
