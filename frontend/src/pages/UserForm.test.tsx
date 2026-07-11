import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import UserForm from "./UserForm";
import { clearSession, setSession } from "../auth/session";
import type { UserRow } from "../api/client";

function mockFetchJson(status: number, body: unknown) {
  return vi.fn().mockResolvedValue({
    ok: status >= 200 && status < 300,
    status,
    json: async () => body,
  });
}

const usuarioExistente: UserRow = {
  id: "user-1",
  nombre: "Ana Gomez",
  email: "ana@cidenet.com",
  rol: "Editor",
  estado: "Activo",
};

describe("UserForm", () => {
  beforeEach(() => {
    setSession({ id: "admin-1", rol: "Admin" });
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    clearSession();
  });

  function llenarFormularioValido() {
    fireEvent.change(screen.getByLabelText(/^nombre$/i), { target: { value: "Bruno Diaz" } });
    fireEvent.change(screen.getByLabelText(/^email$/i), { target: { value: "bruno@cidenet.com" } });
    fireEvent.change(screen.getByLabelText(/^contraseña$/i), { target: { value: "Abcdef1x" } });
    fireEvent.change(screen.getByLabelText(/confirmar contraseña/i), { target: { value: "Abcdef1x" } });
    fireEvent.change(screen.getByLabelText(/^rol$/i), { target: { value: "Editor" } });
  }

  // Scenario: El botón Guardar permanece deshabilitado mientras el formulario es inválido
  it("el botón Guardar está deshabilitado hasta que el formulario es válido", () => {
    render(<UserForm modo="crear" onGuardado={vi.fn()} />);

    expect(screen.getByRole("button", { name: /guardar/i })).toBeDisabled();

    llenarFormularioValido();

    expect(screen.getByRole("button", { name: /guardar/i })).toBeEnabled();
  });

  // Scenario: Rechazar creación con un campo obligatorio vacío
  it("rechaza cuando el nombre está vacío, con mensaje inline y resumen", () => {
    render(<UserForm modo="crear" onGuardado={vi.fn()} />);

    fireEvent.change(screen.getByLabelText(/^nombre$/i), { target: { value: "a" } });
    fireEvent.change(screen.getByLabelText(/^nombre$/i), { target: { value: "" } });

    // El mensaje aparece dos veces a propósito (inline + resumen); basta con que exista.
    expect(screen.getAllByText(/el nombre es obligatorio/i).length).toBeGreaterThan(0);
    expect(screen.getByLabelText(/resumen de errores/i)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /guardar/i })).toBeDisabled();
  });

  // Scenario: Rechazar email con formato inválido
  it("rechaza un email con formato inválido", () => {
    render(<UserForm modo="crear" onGuardado={vi.fn()} />);

    fireEvent.change(screen.getByLabelText(/^email$/i), { target: { value: "ana(arroba)cidenet" } });

    expect(screen.getAllByText(/formato.*no es válido/i).length).toBeGreaterThan(0);
  });

  // Scenario Outline: Validar la política de contraseña
  it.each([
    ["abcdef1x", /mayúscula/i],
    ["ABCDEF1X", /minúscula/i],
    ["Abcdefgh", /número/i],
    ["Ab1x", /8 caracteres/i],
  ])("rechaza la contraseña %s por incumplir la política", (password, mensajeEsperado) => {
    render(<UserForm modo="crear" onGuardado={vi.fn()} />);

    fireEvent.change(screen.getByLabelText(/^contraseña$/i), { target: { value: password } });

    expect(screen.getAllByText(mensajeEsperado).length).toBeGreaterThan(0);
  });

  // Scenario: Rechazar cuando la contraseña y su confirmación no coinciden
  it("rechaza cuando la contraseña y su confirmación no coinciden", () => {
    render(<UserForm modo="crear" onGuardado={vi.fn()} />);

    fireEvent.change(screen.getByLabelText(/^contraseña$/i), { target: { value: "Abcdef1x" } });
    fireEvent.change(screen.getByLabelText(/confirmar contraseña/i), { target: { value: "Distinta9" } });

    expect(screen.getAllByText(/no coinciden/i).length).toBeGreaterThan(0);
  });

  // Scenario: Rechazar cuando no se selecciona un rol válido
  it("exige seleccionar un rol", () => {
    render(<UserForm modo="crear" onGuardado={vi.fn()} />);

    expect(screen.getAllByText(/debes seleccionar un rol/i).length).toBeGreaterThan(0);
  });

  // Scenario: Crear una cuenta con datos válidos
  it("crea la cuenta con datos válidos y notifica el resultado", async () => {
    const creado: UserRow = { id: "new-1", nombre: "Bruno Diaz", email: "bruno@cidenet.com", rol: "Editor", estado: "Activo" };
    const fetchMock = mockFetchJson(201, creado);
    vi.stubGlobal("fetch", fetchMock);
    const onGuardado = vi.fn();

    render(<UserForm modo="crear" onGuardado={onGuardado} />);
    llenarFormularioValido();
    fireEvent.click(screen.getByRole("button", { name: /guardar/i }));

    await waitFor(() => expect(onGuardado).toHaveBeenCalledWith(creado));
    expect(fetchMock).toHaveBeenCalledWith(
      expect.stringContaining("/api/users"),
      expect.objectContaining({ method: "POST" }),
    );
  });

  // Scenario: Rechazar email duplicado (respuesta 409 del backend)
  it("muestra el conflicto de email que devuelve el backend", async () => {
    vi.stubGlobal("fetch", mockFetchJson(409, { error: "El email ya está registrado." }));

    render(<UserForm modo="crear" onGuardado={vi.fn()} />);
    llenarFormularioValido();
    fireEvent.click(screen.getByRole("button", { name: /guardar/i }));

    expect(await screen.findByText(/ya está registrado/i)).toBeInTheDocument();
  });

  // Scenario: Editar los datos de un usuario con éxito (formulario pre-cargado)
  it("el formulario de edición viene pre-cargado y Guardar está deshabilitado sin cambios", () => {
    render(<UserForm modo="editar" usuario={usuarioExistente} onGuardado={vi.fn()} />);

    expect(screen.getByLabelText(/^nombre$/i)).toHaveValue("Ana Gomez");
    expect(screen.getByLabelText(/^email$/i)).toHaveValue("ana@cidenet.com");
    expect(screen.getByRole("button", { name: /guardar/i })).toBeDisabled();

    fireEvent.change(screen.getByLabelText(/^nombre$/i), { target: { value: "Ana Maria Gomez" } });

    expect(screen.getByRole("button", { name: /guardar/i })).toBeEnabled();
  });

  // Scenario (R2): Un Admin no puede modificar su propio rol
  it("deshabilita el selector de rol al editar la propia cuenta", () => {
    setSession({ id: usuarioExistente.id, rol: "Admin" });

    render(<UserForm modo="editar" usuario={usuarioExistente} onGuardado={vi.fn()} />);

    expect(screen.getByLabelText(/^rol$/i)).toBeDisabled();
  });
});
