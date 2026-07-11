import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import UsersTablePage from "./UsersTable";
import { clearSession, setSession } from "../auth/session";

function paginaDe(items: unknown[], total = items.length) {
  return { items, total, page: 1, pageSize: 10 };
}

function mockFetchJson(status: number, body: unknown) {
  return vi.fn().mockResolvedValue({
    ok: status >= 200 && status < 300,
    status,
    json: async () => body,
  });
}

describe("UsersTablePage", () => {
  beforeEach(() => {
    setSession({ id: "admin-1", rol: "Admin" });
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    clearSession();
  });

  // Scenario: Ver la tabla paginada con sus columnas
  it("muestra las columnas y el total de la tabla paginada", async () => {
    vi.stubGlobal(
      "fetch",
      mockFetchJson(
        200,
        paginaDe([
          { id: "1", nombre: "Ana Gomez", email: "ana@cidenet.com", rol: "Editor", estado: "Activo" },
        ]),
      ),
    );

    render(<UsersTablePage />);

    expect(await screen.findByText("Ana Gomez")).toBeInTheDocument();
    expect(screen.getByText("ana@cidenet.com")).toBeInTheDocument();
    expect(screen.getByRole("cell", { name: "Editor" })).toBeInTheDocument();
    expect(screen.getByRole("cell", { name: "Activo" })).toBeInTheDocument();
    expect(screen.getByText(/Total: 1/)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /editar/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /eliminar/i })).toBeInTheDocument();
  });

  // Scenario Outline: Buscar por nombre o email de forma parcial
  it("busca por nombre o email enviando el término al backend", async () => {
    const fetchMock = mockFetchJson(200, paginaDe([]));
    vi.stubGlobal("fetch", fetchMock);

    render(<UsersTablePage />);
    await waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(1));

    fireEvent.change(screen.getByLabelText(/buscar/i), { target: { value: "ana" } });

    await waitFor(() => {
      const url = fetchMock.mock.calls.at(-1)?.[0] as string;
      expect(url).toContain("search=ana");
    });
  });

  // Scenario: Combinar filtros de rol y estado con la búsqueda
  it("combina filtros de rol y estado", async () => {
    const fetchMock = mockFetchJson(200, paginaDe([]));
    vi.stubGlobal("fetch", fetchMock);

    render(<UsersTablePage />);
    await waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(1));

    fireEvent.change(screen.getByLabelText(/^rol$/i), { target: { value: "Editor" } });
    fireEvent.change(screen.getByLabelText(/^estado$/i), { target: { value: "Activo" } });

    await waitFor(() => {
      const url = fetchMock.mock.calls.at(-1)?.[0] as string;
      expect(url).toContain("rol=Editor");
      expect(url).toContain("estado=Activo");
    });
  });

  // Scenario: Búsqueda o filtros sin resultados
  it("muestra un estado vacío sin resultados, sin mostrar error", async () => {
    vi.stubGlobal("fetch", mockFetchJson(200, paginaDe([])));

    render(<UsersTablePage />);

    expect(await screen.findByText(/no hay usuarios/i)).toBeInTheDocument();
    expect(screen.queryByRole("alert")).not.toBeInTheDocument();
  });

  // Estado de error si la petición falla
  it("muestra un estado de error si la petición falla", async () => {
    vi.stubGlobal("fetch", mockFetchJson(500, { error: "boom" }));

    render(<UsersTablePage />);

    expect(await screen.findByRole("alert")).toHaveTextContent(/error/i);
  });

  // Scenario (US-002-SEC): El Editor ve la tabla en solo lectura
  it("un Editor no ve las acciones de gestión", async () => {
    setSession({ id: "editor-1", rol: "Editor" });
    vi.stubGlobal(
      "fetch",
      mockFetchJson(
        200,
        paginaDe([
          { id: "1", nombre: "Ana Gomez", email: "ana@cidenet.com", rol: "Editor", estado: "Activo" },
        ]),
      ),
    );

    render(<UsersTablePage />);

    await screen.findByText("Ana Gomez");
    expect(screen.queryByRole("button", { name: /editar/i })).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: /eliminar/i })).not.toBeInTheDocument();
  });

  // Scenario (US-002-SEC): El Viewer no accede a la tabla
  it("un Viewer no tiene acceso a la tabla", async () => {
    setSession({ id: "viewer-1", rol: "Viewer" });
    vi.stubGlobal("fetch", mockFetchJson(403, { error: "forbidden" }));

    render(<UsersTablePage />);

    expect(await screen.findByRole("alert")).toHaveTextContent(/no tienes acceso/i);
  });
});
