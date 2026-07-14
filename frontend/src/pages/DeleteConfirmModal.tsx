import { useState } from "react";
import { ApiError, deleteUser, type UserRow } from "../api/client";

export interface DeleteConfirmModalProps {
  usuario: UserRow;
  onEliminado: (id?: string) => void;
  onCancelar: () => void;
}

export default function DeleteConfirmModal({ usuario, onEliminado, onCancelar }: DeleteConfirmModalProps) {
  const [eliminando, setEliminando] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [infoMessage, setInfoMessage] = useState<string | null>(null);

  async function handleConfirmar() {
    setEliminando(true);
    setErrorMessage(null);
    setInfoMessage(null);

    try {
      await deleteUser(usuario.id);
      onEliminado(usuario.id);
    } catch (err) {
      if (err instanceof ApiError && err.status === 404) {
        setInfoMessage("Este usuario ya no existe: probablemente otro Admin ya lo eliminó.");
      } else if (err instanceof ApiError) {
        setErrorMessage(err.message);
      } else {
        setErrorMessage("Ocurrió un error inesperado.");
      }
    } finally {
      setEliminando(false);
    }
  }

  return (
    <div>
      <h2>Eliminar usuario</h2>
      <p>{usuario.nombre}</p>
      <p>{usuario.email}</p>
      <p>{usuario.rol}</p>

      {usuario.rol === "Admin" && (
        <p role="alert">Advertencia: estás a punto de eliminar una cuenta con rol Admin.</p>
      )}

      {infoMessage && <p>{infoMessage}</p>}
      {errorMessage && <p role="alert">{errorMessage}</p>}

      <button type="button" onClick={handleConfirmar} disabled={eliminando}>
        Confirmar eliminación
      </button>
      <button type="button" onClick={onCancelar}>
        Cancelar
      </button>
    </div>
  );
}
