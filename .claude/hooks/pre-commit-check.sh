#!/usr/bin/env bash
# PreToolUse hook sobre Bash: si el comando es un `git commit`, corre lint+tests
# antes de dejarlo pasar. Bloquea el commit (exit 2) si algo falla.
set -euo pipefail

INPUT=$(cat)

# Detectamos "git commit" en el campo command del JSON de entrada, sin jq
# (así el hook funciona en cualquier máquina local, sin dependencias extra).
if ! printf '%s' "$INPUT" | grep -qE '"command"[[:space:]]*:[[:space:]]*"[^"]*git[[:space:]]+commit'; then
  exit 0
fi

REPO_ROOT=$(git rev-parse --show-toplevel 2>/dev/null || pwd)
cd "$REPO_ROOT"

echo "[pre-commit-check] Verificando formato y tests antes del commit..." >&2

# Solo verificamos .NET si el commit toca código .NET (igual que hacemos con frontend/).
# Un commit de solo-docs (p. ej. el discovery del Día 1) no necesita el toolchain de .NET.
if git diff --cached --name-only | grep -qE '\.(cs|csproj|sln)$'; then
  if ! command -v dotnet >/dev/null 2>&1; then
    echo "[pre-commit-check] BLOQUEADO: el commit toca código .NET pero 'dotnet' no está instalado. Instálalo para poder formatear y correr los tests." >&2
    exit 2
  fi

  if ! dotnet format --verify-no-changes >&2; then
    echo "[pre-commit-check] BLOQUEADO: hay archivos .NET sin formatear. Corre 'dotnet format' y vuelve a intentar." >&2
    exit 2
  fi

  if ! dotnet test >&2; then
    echo "[pre-commit-check] BLOQUEADO: hay tests de backend fallando." >&2
    exit 2
  fi
else
  echo "[pre-commit-check] Sin código .NET en stage — se omiten dotnet format/test." >&2
fi

if git diff --cached --name-only | grep -q '^frontend/'; then
  if ! (cd frontend && npm test) >&2; then
    echo "[pre-commit-check] BLOQUEADO: hay tests de frontend fallando." >&2
    exit 2
  fi
fi

echo "[pre-commit-check] OK — tests en verde, commit permitido." >&2
exit 0
