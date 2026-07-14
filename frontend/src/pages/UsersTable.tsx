import { useEffect, useState } from "react";
import { ApiError, listUsers, type UserRow } from "../api/client";
import { getSession } from "../auth/session";
import UserForm from "./UserForm";
import DeleteConfirmModal from "./DeleteConfirmModal";

type Status = "loading" | "ok" | "empty" | "error" | "forbidden";

type ModalState =
  | { tipo: "crear" }
  | { tipo: "editar"; usuario: UserRow }
  | { tipo: "eliminar"; usuario: UserRow }
  | null;

const SEARCH_DEBOUNCE_MS = 300;

export default function UsersTablePage() {
  const session = getSession();
  const puedeGestionar = session?.rol === "Admin";

  const [search, setSearch] = useState("");
  const [rol, setRol] = useState("");
  const [estado, setEstado] = useState("");
  const [page, setPage] = useState(1);
  const [version, setVersion] = useState(0);

  const [items, setItems] = useState<UserRow[]>([]);
  const [total, setTotal] = useState(0);
  const [status, setStatus] = useState<Status>("loading");
  const [modal, setModal] = useState<ModalState>(null);

  useEffect(() => {
    let cancelled = false;
    setStatus("loading");

    const timeout = setTimeout(() => {
      listUsers({ search, rol, estado, page })
        .then((pagina) => {
          if (cancelled) return;
          setItems(pagina.items);
          setTotal(pagina.total);
          setStatus(pagina.items.length === 0 ? "empty" : "ok");
        })
        .catch((err) => {
          if (cancelled) return;
          setStatus(err instanceof ApiError && err.status === 403 ? "forbidden" : "error");
        });
    }, SEARCH_DEBOUNCE_MS);

    return () => {
      cancelled = true;
      clearTimeout(timeout);
    };
  }, [search, rol, estado, page, version]);

  function cerrarModalYRefrescar() {
    setModal(null);
    setVersion((v) => v + 1);
  }

  if (status === "forbidden") {
    return <p role="alert">No tienes acceso a esta sección.</p>;
  }

  return (
    <section>
      <h1>Usuarios</h1>

      {puedeGestionar && (
        <button type="button" onClick={() => setModal({ tipo: "crear" })}>
          Nuevo usuario
        </button>
      )}

      <div>
        <label>
          Buscar
          <input
            aria-label="Buscar"
            value={search}
            onChange={(e) => {
              setPage(1);
              setSearch(e.target.value);
            }}
          />
        </label>
        <label>
          Rol
          <select
            aria-label="Rol"
            value={rol}
            onChange={(e) => {
              setPage(1);
              setRol(e.target.value);
            }}
          >
            <option value="">Todos</option>
            <option value="Admin">Admin</option>
            <option value="Editor">Editor</option>
            <option value="Viewer">Viewer</option>
          </select>
        </label>
        <label>
          Estado
          <select
            aria-label="Estado"
            value={estado}
            onChange={(e) => {
              setPage(1);
              setEstado(e.target.value);
            }}
          >
            <option value="">Todos</option>
            <option value="Activo">Activo</option>
            <option value="Inactivo">Inactivo</option>
          </select>
        </label>
      </div>

      {status === "loading" && <p>Cargando…</p>}
      {status === "error" && <p role="alert">Ocurrió un error al cargar los usuarios.</p>}
      {status === "empty" && <p>No hay usuarios que coincidan con los criterios.</p>}

      {(status === "ok" || status === "empty") && (
        <>
          <table>
            <thead>
              <tr>
                <th>Nombre</th>
                <th>Email</th>
                <th>Rol</th>
                <th>Estado</th>
                {puedeGestionar && <th>Acciones</th>}
              </tr>
            </thead>
            <tbody>
              {items.map((u) => (
                <tr key={u.id}>
                  <td>{u.nombre}</td>
                  <td>{u.email}</td>
                  <td>{u.rol}</td>
                  <td>{u.estado}</td>
                  {puedeGestionar && (
                    <td>
                      <button type="button" onClick={() => setModal({ tipo: "editar", usuario: u })}>
                        Editar
                      </button>
                      <button type="button" onClick={() => setModal({ tipo: "eliminar", usuario: u })}>
                        Eliminar
                      </button>
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
          <p>Total: {total}</p>
          <div>
            <button type="button" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
              Anterior
            </button>
            <button
              type="button"
              disabled={page * 10 >= total}
              onClick={() => setPage((p) => p + 1)}
            >
              Siguiente
            </button>
          </div>
        </>
      )}

      {modal && modal.tipo === "eliminar" && (
        <section aria-label="Eliminar usuario">
          <DeleteConfirmModal
            usuario={modal.usuario}
            onEliminado={cerrarModalYRefrescar}
            onCancelar={() => setModal(null)}
          />
        </section>
      )}

      {modal && modal.tipo !== "eliminar" && (
        <section aria-label={modal.tipo === "editar" ? "Editar usuario" : "Nuevo usuario"}>
          <h2>{modal.tipo === "editar" ? "Editar usuario" : "Nuevo usuario"}</h2>
          <UserForm
            modo={modal.tipo === "crear" ? "crear" : "editar"}
            usuario={modal.tipo === "editar" ? modal.usuario : undefined}
            onGuardado={cerrarModalYRefrescar}
          />
          <button type="button" onClick={() => setModal(null)}>
            Cancelar
          </button>
        </section>
      )}
    </section>
  );
}
