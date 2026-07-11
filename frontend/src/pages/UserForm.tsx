import { useMemo, useState, type FormEvent } from "react";
import { ApiError, createUser, updateUser, type UserRow } from "../api/client";
import { getSession } from "../auth/session";

export interface UserFormProps {
  modo: "crear" | "editar";
  usuario?: UserRow;
  onGuardado: (usuario: UserRow) => void;
}

function validarNombre(nombre: string): string | null {
  const trimmed = nombre.trim();
  if (!trimmed) return "El nombre es obligatorio.";
  if (trimmed.length < 2 || trimmed.length > 100) return "El nombre debe tener entre 2 y 100 caracteres.";
  return null;
}

function validarEmail(email: string): string | null {
  const trimmed = email.trim();
  if (!trimmed) return "El email es obligatorio.";
  if (!/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(trimmed)) return "El formato del email no es válido.";
  return null;
}

function validarPassword(password: string): string | null {
  if (!password || password.length < 8) return "La contraseña debe tener al menos 8 caracteres.";
  if (!/[A-Z]/.test(password)) return "La contraseña debe incluir una mayúscula.";
  if (!/[a-z]/.test(password)) return "La contraseña debe incluir una minúscula.";
  if (!/[0-9]/.test(password)) return "La contraseña debe incluir un número.";
  return null;
}

export default function UserForm({ modo, usuario, onGuardado }: UserFormProps) {
  const session = getSession();
  const esPropiaCuenta = modo === "editar" && usuario?.id === session?.id;

  const [nombre, setNombre] = useState(usuario?.nombre ?? "");
  const [email, setEmail] = useState(usuario?.email ?? "");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [rol, setRol] = useState(usuario?.rol ?? "");
  const [estado, setEstado] = useState(usuario?.estado ?? "Activo");
  const [serverError, setServerError] = useState<string | null>(null);
  const [emailServerError, setEmailServerError] = useState<string | null>(null);
  const [guardando, setGuardando] = useState(false);

  const errores = useMemo(() => {
    const e: Record<string, string> = {};

    const nombreError = validarNombre(nombre);
    if (nombreError) e.nombre = nombreError;

    const emailError = validarEmail(email);
    if (emailError) e.email = emailError;

    if (!rol) e.rol = "Debes seleccionar un rol (Admin, Editor o Viewer).";

    if (modo === "crear") {
      const passwordError = validarPassword(password);
      if (passwordError) e.password = passwordError;
      if (password !== confirmPassword) e.confirmPassword = "La contraseña y su confirmación no coinciden.";
    }

    return e;
  }, [nombre, email, rol, password, confirmPassword, modo]);

  const huboCambios =
    modo === "crear" ||
    nombre.trim() !== usuario?.nombre ||
    email.trim().toLowerCase() !== usuario?.email.toLowerCase() ||
    rol !== usuario?.rol ||
    estado !== usuario?.estado;

  const esValido = Object.keys(errores).length === 0;
  const puedeGuardar = esValido && huboCambios && !guardando;

  async function handleSubmit(ev: FormEvent) {
    ev.preventDefault();
    if (!puedeGuardar) return;

    setGuardando(true);
    setServerError(null);
    setEmailServerError(null);

    try {
      const resultado =
        modo === "crear"
          ? await createUser({
              nombre: nombre.trim(),
              email: email.trim(),
              password,
              confirmPassword,
              rol,
              estado,
            })
          : await updateUser(usuario!.id, { nombre: nombre.trim(), email: email.trim(), rol, estado });

      onGuardado(resultado);
    } catch (err) {
      if (err instanceof ApiError && err.status === 409) {
        setEmailServerError(err.message);
      } else {
        setServerError(err instanceof Error ? err.message : "Ocurrió un error inesperado.");
      }
    } finally {
      setGuardando(false);
    }
  }

  const mensajesPendientes = Object.values(errores);

  return (
    <form onSubmit={handleSubmit} noValidate>
      <label>
        Nombre
        <input aria-label="Nombre" value={nombre} onChange={(e) => setNombre(e.target.value)} />
      </label>
      {errores.nombre && <p role="alert">{errores.nombre}</p>}

      <label>
        Email
        <input aria-label="Email" value={email} onChange={(e) => setEmail(e.target.value)} />
      </label>
      {(errores.email || emailServerError) && <p role="alert">{errores.email ?? emailServerError}</p>}

      {modo === "crear" && (
        <>
          <label>
            Contraseña
            <input
              aria-label="Contraseña"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
          </label>
          {errores.password && <p role="alert">{errores.password}</p>}

          <label>
            Confirmar contraseña
            <input
              aria-label="Confirmar contraseña"
              type="password"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
            />
          </label>
          {errores.confirmPassword && <p role="alert">{errores.confirmPassword}</p>}
        </>
      )}

      <label>
        Rol
        <select aria-label="Rol" value={rol} disabled={esPropiaCuenta} onChange={(e) => setRol(e.target.value)}>
          <option value="">Selecciona un rol</option>
          <option value="Admin">Admin</option>
          <option value="Editor">Editor</option>
          <option value="Viewer">Viewer</option>
        </select>
      </label>
      {errores.rol && <p role="alert">{errores.rol}</p>}

      <label>
        Estado
        <select aria-label="Estado" value={estado} onChange={(e) => setEstado(e.target.value)}>
          <option value="Activo">Activo</option>
          <option value="Inactivo">Inactivo</option>
        </select>
      </label>

      {mensajesPendientes.length > 0 && (
        <ul aria-label="Resumen de errores">
          {mensajesPendientes.map((m) => (
            <li key={m}>{m}</li>
          ))}
        </ul>
      )}
      {serverError && <p role="alert">{serverError}</p>}

      <button type="submit" disabled={!puedeGuardar}>
        Guardar
      </button>
    </form>
  );
}
