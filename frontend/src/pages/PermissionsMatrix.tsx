import { useEffect, useState } from "react";
import { ApiError, getPermissionMatrix, updatePermissions, type PermissionCell } from "../api/client";

const ROLES = ["Admin", "Editor", "Viewer"] as const;
const RECURSOS = ["Users", "Roles", "Permissions", "Reports"] as const;
const ACCIONES = ["Create", "Read", "Update", "Delete"] as const;

type Status = "loading" | "ok" | "error";

export default function PermissionsMatrixPage() {
  const [celdas, setCeldas] = useState<PermissionCell[]>([]);
  const [status, setStatus] = useState<Status>("loading");
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getPermissionMatrix()
      .then((matriz) => {
        setCeldas(matriz);
        setStatus("ok");
      })
      .catch(() => setStatus("error"));
  }, []);

  async function alternar(rol: string, recurso: string, accion: string, permitido: boolean) {
    const anterior = celdas;
    setCeldas((prev) =>
      prev.map((c) => (c.rol === rol && c.recurso === recurso && c.accion === accion ? { ...c, permitido } : c)),
    );
    setError(null);

    try {
      const actualizada = await updatePermissions([{ rol, recurso, accion, permitido }]);
      setCeldas(actualizada);
    } catch (err) {
      setCeldas(anterior); // revertir el toggle optimista
      setError(err instanceof ApiError ? err.message : "No se pudo guardar el cambio.");
    }
  }

  if (status === "loading") {
    return <p>Cargando…</p>;
  }

  if (status === "error") {
    return <p role="alert">Ocurrió un error al cargar la matriz de permisos.</p>;
  }

  function permitidoDe(rol: string, recurso: string, accion: string): boolean {
    return celdas.find((c) => c.rol === rol && c.recurso === recurso && c.accion === accion)?.permitido ?? false;
  }

  return (
    <section>
      <h1>Matriz de permisos</h1>
      {error && <p role="alert">{error}</p>}
      <table>
        <thead>
          <tr>
            <th>Rol</th>
            <th>Recurso</th>
            {ACCIONES.map((accion) => (
              <th key={accion}>{accion}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {ROLES.map((rol) =>
            RECURSOS.map((recurso) => (
              <tr key={`${rol}-${recurso}`}>
                <td>{rol}</td>
                <td>{recurso}</td>
                {ACCIONES.map((accion) => (
                  <td key={accion}>
                    <input
                      type="checkbox"
                      aria-label={`${rol} ${recurso} ${accion}`}
                      checked={permitidoDe(rol, recurso, accion)}
                      disabled={rol === "Admin"}
                      title={rol === "Admin" ? "Los permisos de Admin no se pueden modificar." : undefined}
                      onChange={(e) => alternar(rol, recurso, accion, e.target.checked)}
                    />
                  </td>
                ))}
              </tr>
            )),
          )}
        </tbody>
      </table>
    </section>
  );
}
