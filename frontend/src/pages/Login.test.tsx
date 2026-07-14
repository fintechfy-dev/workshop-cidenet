import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import Login from "./Login";
import { clearSession, getSession } from "../auth/session";

function mockFetchJson(status: number, body: unknown = {}) {
  return vi.fn().mockResolvedValue({
    ok: status >= 200 && status < 300,
    status,
    json: async () => body,
  });
}

describe("Login", () => {
  beforeEach(() => {
    clearSession();
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    clearSession();
  });

  // Scenario: Autenticar a un usuario activo con credenciales correctas
  it("autentica a un usuario activo con credenciales correctas y guarda la sesión", async () => {
    const usuario = { id: "user-1", nombre: "Ana Gomez", email: "ana@cidenet.com", rol: "Editor", estado: "Activo" };
    const fetchMock = mockFetchJson(200, usuario);
    vi.stubGlobal("fetch", fetchMock);
    const onLogin = vi.fn();

    render(<Login onLogin={onLogin} />);

    fireEvent.change(screen.getByLabelText(/^email$/i), { target: { value: "ana@cidenet.com" } });
    fireEvent.change(screen.getByLabelText(/^contraseña$/i), { target: { value: "Abcdef1x" } });
    fireEvent.click(screen.getByRole("button", { name: /iniciar sesión/i }));

    await waitFor(() => expect(onLogin).toHaveBeenCalled());
    expect(fetchMock).toHaveBeenCalledWith(
      expect.stringContaining("/api/auth/login"),
      expect.objectContaining({ method: "POST" }),
    );
    expect(getSession()).toEqual({ id: "user-1", rol: "Editor" });
  });

  // Scenario: Bloquear a un usuario inactivo aunque sus credenciales sean correctas (R6)
  it("bloquea a un usuario inactivo y muestra el mensaje del backend", async () => {
    vi.stubGlobal(
      "fetch",
      mockFetchJson(403, { error: "La cuenta está inactiva. Contacte al administrador." }),
    );
    const onLogin = vi.fn();

    render(<Login onLogin={onLogin} />);

    fireEvent.change(screen.getByLabelText(/^email$/i), { target: { value: "ana@cidenet.com" } });
    fireEvent.change(screen.getByLabelText(/^contraseña$/i), { target: { value: "Abcdef1x" } });
    fireEvent.click(screen.getByRole("button", { name: /iniciar sesión/i }));

    expect(await screen.findByRole("alert")).toHaveTextContent(/inactiva/i);
    expect(onLogin).not.toHaveBeenCalled();
    expect(getSession()).toBeNull();
  });

  // Scenario: Credenciales inválidas devuelven un mensaje genérico (anti-enumeración)
  it("muestra un mensaje genérico ante credenciales inválidas, sin revelar el motivo", async () => {
    vi.stubGlobal(
      "fetch",
      mockFetchJson(401, { error: "El email o la contraseña son incorrectos." }),
    );
    const onLogin = vi.fn();

    render(<Login onLogin={onLogin} />);

    fireEvent.change(screen.getByLabelText(/^email$/i), { target: { value: "inexistente@cidenet.com" } });
    fireEvent.change(screen.getByLabelText(/^contraseña$/i), { target: { value: "Cualquiera1" } });
    fireEvent.click(screen.getByRole("button", { name: /iniciar sesión/i }));

    const alerta = await screen.findByRole("alert");
    expect(alerta).toHaveTextContent(/el email o la contraseña son incorrectos/i);
    expect(alerta).not.toHaveTextContent(/no existe|no registrado/i);
    expect(onLogin).not.toHaveBeenCalled();
    expect(getSession()).toBeNull();
  });

  // Scenario: Email y contraseña son obligatorios
  it("exige email y contraseña antes de permitir el envío", () => {
    const fetchMock = vi.fn();
    vi.stubGlobal("fetch", fetchMock);

    render(<Login onLogin={vi.fn()} />);

    expect(screen.getByRole("button", { name: /iniciar sesión/i })).toBeDisabled();

    fireEvent.change(screen.getByLabelText(/^email$/i), { target: { value: "ana@cidenet.com" } });
    expect(screen.getByRole("button", { name: /iniciar sesión/i })).toBeDisabled();

    fireEvent.change(screen.getByLabelText(/^contraseña$/i), { target: { value: "Abcdef1x" } });
    expect(screen.getByRole("button", { name: /iniciar sesión/i })).toBeEnabled();

    expect(fetchMock).not.toHaveBeenCalled();
  });
});
