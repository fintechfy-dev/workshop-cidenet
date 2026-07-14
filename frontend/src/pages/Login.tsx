import { useState, type FormEvent } from "react";
import { ApiError, login } from "../api/client";
import { setSession } from "../auth/session";

export interface LoginProps {
  onLogin: () => void;
}

export default function Login({ onLogin }: LoginProps) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [enviando, setEnviando] = useState(false);

  const puedeEnviar = email.trim() !== "" && password !== "" && !enviando;

  async function handleSubmit(ev: FormEvent) {
    ev.preventDefault();
    if (!puedeEnviar) return;

    setEnviando(true);
    setError(null);

    try {
      const usuario = await login(email.trim(), password);
      setSession({ id: usuario.id, rol: usuario.rol });
      onLogin();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Ocurrió un error inesperado.");
    } finally {
      setEnviando(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} noValidate>
      <h1>Iniciar sesión</h1>

      <label>
        Email
        <input aria-label="Email" value={email} onChange={(e) => setEmail(e.target.value)} />
      </label>

      <label>
        Contraseña
        <input
          aria-label="Contraseña"
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />
      </label>

      {error && <p role="alert">{error}</p>}

      <button type="submit" disabled={!puedeEnviar}>
        Iniciar sesión
      </button>
    </form>
  );
}
