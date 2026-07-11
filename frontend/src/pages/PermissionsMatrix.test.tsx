import { fireEvent, render, screen, waitFor, within } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import PermissionsMatrixPage from "./PermissionsMatrix";
import type { PermissionCell } from "../api/client";

const RECURSOS = ["Users", "Roles", "Permissions", "Reports"] as const;
const ACCIONES = ["Create", "Read", "Update", "Delete"] as const;

function matrizPorDefecto(): PermissionCell[] {
  const cells: PermissionCell[] = [];
  for (const recurso of RECURSOS) {
    for (const accion of ACCIONES) {
      cells.push({ rol: "Admin", recurso, accion, permitido: true });
      cells.push({
        rol: "Editor",
        recurso,
        accion,
        permitido: recurso === "Reports" || accion === "Read",
      });
      cells.push({
        rol: "Viewer",
        recurso,
        accion,
        permitido: recurso === "Reports" && accion === "Read",
      });
    }
  }
  return cells;
}

function mockFetchJson(status: number, body: unknown) {
  return vi.fn().mockResolvedValue({
    ok: status >= 200 && status < 300,
    status,
    json: async () => body,
  });
}

describe("PermissionsMatrixPage", () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  // Scenario: Ver la matriz de permisos con su estado actual
  it("muestra la matriz con el estado actual de cada rol/recurso/acción", async () => {
    vi.stubGlobal("fetch", mockFetchJson(200, matrizPorDefecto()));

    render(<PermissionsMatrixPage />);

    const fila = await screen.findByRole("row", { name: /editor.*reports/i });
    const casillas = within(fila).getAllByRole("checkbox");
    expect(casillas).toHaveLength(4);
    expect(casillas.every((c) => (c as HTMLInputElement).checked)).toBe(true); // Editor: CRUD sobre reports
  });

  // Scenario: Activar o desactivar un permiso individual
  it("desactivar un permiso individual lo persiste vía PUT", async () => {
    const fetchMock = mockFetchJson(200, matrizPorDefecto());
    vi.stubGlobal("fetch", fetchMock);

    render(<PermissionsMatrixPage />);

    const filaEditorReports = await screen.findByRole("row", { name: /editor.*reports/i });
    const checkboxCreate = within(filaEditorReports).getAllByRole("checkbox")[0];

    const actualizada = matrizPorDefecto().map((c) =>
      c.rol === "Editor" && c.recurso === "Reports" && c.accion === "Create" ? { ...c, permitido: false } : c,
    );
    fetchMock.mockResolvedValueOnce({ ok: true, status: 200, json: async () => actualizada });

    fireEvent.click(checkboxCreate);

    await waitFor(() => {
      const ultimaLlamada = fetchMock.mock.calls.at(-1);
      expect(ultimaLlamada?.[1]).toEqual(
        expect.objectContaining({
          method: "PUT",
          body: JSON.stringify({
            cambios: [{ rol: "Editor", recurso: "Reports", accion: "Create", permitido: false }],
          }),
        }),
      );
    });

    await waitFor(() => expect(checkboxCreate).not.toBeChecked());
  });

  // Scenario (R3 + anti-lockout): los permisos de Admin son inmutables desde la UI
  it("los checkboxes de la fila Admin están deshabilitados", async () => {
    vi.stubGlobal("fetch", mockFetchJson(200, matrizPorDefecto()));

    render(<PermissionsMatrixPage />);

    const filaAdminUsers = await screen.findByRole("row", { name: /admin.*users/i });
    const casillas = within(filaAdminUsers).getAllByRole("checkbox");
    expect(casillas.every((c) => (c as HTMLInputElement).disabled)).toBe(true);
  });

  // Estado de error si la petición falla
  it("muestra un estado de error si no se puede cargar la matriz", async () => {
    vi.stubGlobal("fetch", mockFetchJson(500, { error: "boom" }));

    render(<PermissionsMatrixPage />);

    expect(await screen.findByRole("alert")).toHaveTextContent(/error/i);
  });
});
