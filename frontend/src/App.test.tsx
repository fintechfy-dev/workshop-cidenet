import { render, screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import App from "./App";

describe("App", () => {
  beforeEach(() => {
    vi.stubGlobal(
      "fetch",
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => ({ status: "ok" }),
      }),
    );
  });

  it("renders the module title", () => {
    render(<App />);
    expect(
      screen.getByText(/Módulo de Usuarios, Roles y Permisos/i),
    ).toBeInTheDocument();
  });
});
