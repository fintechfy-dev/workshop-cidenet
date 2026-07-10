import { useEffect, useState } from "react";
import { BrowserRouter, Link, Route, Routes } from "react-router-dom";
import { checkHealth } from "./api/client";
import { UsersTable } from "./pages/UsersTable";
import { UserForm } from "./pages/UserForm";
import { PermissionsMatrix } from "./pages/PermissionsMatrix";
import { DeleteConfirmModal } from "./pages/DeleteConfirmModal";
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

export default function App() {
  return (
    <BrowserRouter>
      <header>
        <h1>Módulo de Usuarios, Roles y Permisos</h1>
        <HealthBadge />
        <nav>
          <Link to="/">Usuarios</Link> | <Link to="/nuevo">Nuevo Usuario</Link> |{" "}
          <Link to="/permisos">Permisos</Link>
        </nav>
      </header>
      <main>
        <Routes>
          <Route path="/" element={<UsersTable />} />
          <Route path="/nuevo" element={<UserForm />} />
          <Route path="/permisos" element={<PermissionsMatrix />} />
          <Route path="/eliminar" element={<DeleteConfirmModal />} />
        </Routes>
      </main>
    </BrowserRouter>
  );
}
