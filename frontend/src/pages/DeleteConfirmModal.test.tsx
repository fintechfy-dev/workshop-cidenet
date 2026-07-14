import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import DeleteConfirmModal from "./DeleteConfirmModal";
import { clearSession, setSession } from "../auth/session";
import type { UserRow } from "../api/client";

function mockFetchJson(status: number, body: unknown = {}) {
  return vi.fn().mockResolvedValue({
    ok: status >= 200 && status < 300,
    status,
    json: async () => body,
  });
}

const usuarioEditor: UserRow = {
  id: "user-1",
  nombre: "Ana Gomez",
  email: "ana@cidenet.com",
  rol: "Editor",
  estado: "Activo",
};

const usuarioAdmin: UserRow = {
  id: "admin-2",
  nombre: "Carlos Perez",
  email: "carlos@cidenet.com",
  rol: "Admin",
  estado: "Activo",
};

describe("DeleteConfirmModal", () => {
  beforeEach(() => {
    setSession({ id: "admin-1", rol: "Admin" });
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    clearSession();
  });

  // Scenario: Eliminar una cuenta con confirmación explícita
  it("muestra nombre, email y rol del usuario, y confirma la eliminación al aceptar", async () => {
    const fetchMock = mockFetchJson(204);
    vi.stubGlobal("fetch", fetchMock);
    const onEliminado = vi.fn();

    render(<DeleteConfirmModal usuario={usuarioEditor} onEliminado={onEliminado} onCancelar={vi.fn()} />);

    expect(screen.getByText("Ana Gomez")).toBeInTheDocument();
    expect(screen.getByText("ana@cidenet.com")).toBeInTheDocument();
    expect(screen.getByText("Editor")).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: /confirmar eliminación/i }));

    await waitFor(() => expect(onEliminado).toHaveBeenCalled());
    expect(fetchMock).toHaveBeenCalledWith(
      expect.stringContaining(`/api/users/${usuarioEditor.id}`),
      expect.objectContaining({ method: "DELETE" }),
    );
  });

  // Scenario: El modal advierte cuando el usuario a eliminar es Admin
  it("muestra una advertencia destacada cuando el usuario a eliminar es Admin", () => {
    vi.stubGlobal("fetch", vi.fn());

    render(<DeleteConfirmModal usuario={usuarioAdmin} onEliminado={vi.fn()} onCancelar={vi.fn()} />);

    expect(screen.getByRole("alert")).toHaveTextContent(/admin/i);
  });

  // Scenario: No se puede eliminar el último Admin (R1)
  it("muestra el mensaje del backend cuando no se puede eliminar el último Admin", async () => {
    vi.stubGlobal(
      "fetch",
      mockFetchJson(409, { error: "No se puede eliminar el último administrador del sistema." }),
    );
    const onEliminado = vi.fn();

    render(<DeleteConfirmModal usuario={usuarioAdmin} onEliminado={onEliminado} onCancelar={vi.fn()} />);
    fireEvent.click(screen.getByRole("button", { name: /confirmar eliminación/i }));

    expect(await screen.findByText(/no se puede eliminar el último administrador/i)).toBeInTheDocument();
    expect(onEliminado).not.toHaveBeenCalled();
  });

  // Scenario: Un Admin no puede eliminar su propia cuenta
  it("muestra el mensaje del backend cuando el Admin intenta eliminar su propia cuenta", async () => {
    vi.stubGlobal(
      "fetch",
      mockFetchJson(409, { error: "No puedes eliminar tu propia cuenta; debe hacerlo otro Admin." }),
    );
    const onEliminado = vi.fn();

    render(<DeleteConfirmModal usuario={usuarioAdmin} onEliminado={onEliminado} onCancelar={vi.fn()} />);
    fireEvent.click(screen.getByRole("button", { name: /confirmar eliminación/i }));

    expect(await screen.findByText(/debe hacerlo otro admin/i)).toBeInTheDocument();
    expect(onEliminado).not.toHaveBeenCalled();
  });

  // Scenario: Eliminar una cuenta que ya fue eliminada en paralelo
  it("informa sin fallar cuando la cuenta ya fue eliminada por otro Admin", async () => {
    vi.stubGlobal("fetch", mockFetchJson(404));

    render(<DeleteConfirmModal usuario={usuarioEditor} onEliminado={vi.fn()} onCancelar={vi.fn()} />);
    fireEvent.click(screen.getByRole("button", { name: /confirmar eliminación/i }));

    expect(await screen.findByText(/ya no existe/i)).toBeInTheDocument();
    expect(screen.queryByText(/ocurrió un error inesperado/i)).not.toBeInTheDocument();
  });

  // Scenario: Un botón "Cancelar" cierra el modal sin llamar al backend
  it("el botón Cancelar cierra el modal sin llamar al backend", () => {
    const fetchMock = vi.fn();
    vi.stubGlobal("fetch", fetchMock);
    const onCancelar = vi.fn();

    render(<DeleteConfirmModal usuario={usuarioEditor} onEliminado={vi.fn()} onCancelar={onCancelar} />);
    fireEvent.click(screen.getByRole("button", { name: /cancelar/i }));

    expect(onCancelar).toHaveBeenCalled();
    expect(fetchMock).not.toHaveBeenCalled();
  });
});
