import { useEffect, useState } from "react";
import { checkHealth } from "./api/client";
import { clearSession, getSession } from "./auth/session";
import UsersTablePage from "./pages/UsersTable";
import PermissionsMatrixPage from "./pages/PermissionsMatrix";
import Login from "./pages/Login";
import "./App.css";

function HealthBadge() {
  const [status, setStatus] = useState<"checking" | "ok" | "error">("checking");

  useEffect(() => {
    checkHealth()
      .then(() => setStatus("ok"))
      .catch(() => setStatus("error"));
  }, []);

  return <span data-testid="health-badge">API: {status}</span>;
}

type Vista = "usuarios" | "permisos";

export default function App() {
  const [session, setSessionState] = useState(getSession());
  const [vista, setVista] = useState<Vista>("usuarios");

  if (!session) {
    return (
      <>
        <header>
          <h1>Workshop AI-First</h1>
          <HealthBadge />
        </header>
        <main>
          <Login onLogin={() => setSessionState(getSession())} />
        </main>
      </>
    );
  }

  function cerrarSesion() {
    clearSession();
    setSessionState(null);
    setVista("usuarios");
  }

  const esAdmin = session.rol === "Admin";

  return (
    <>
      <header>
        <h1>Workshop AI-First</h1>
        <HealthBadge />
      </header>
      <nav>
        <button type="button" onClick={() => setVista("usuarios")} disabled={vista === "usuarios"}>
          Usuarios
        </button>
        {esAdmin && (
          <button type="button" onClick={() => setVista("permisos")} disabled={vista === "permisos"}>
            Permisos
          </button>
        )}
        <button type="button" onClick={cerrarSesion}>
          Cerrar sesión
        </button>
      </nav>
      <main>{vista === "permisos" && esAdmin ? <PermissionsMatrixPage /> : <UsersTablePage />}</main>
    </>
  );
}
