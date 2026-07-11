import { useEffect, useState } from "react";
import { checkHealth } from "./api/client";
import UsersTablePage from "./pages/UsersTable";
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
    <>
      <header>
        <h1>Workshop AI-First</h1>
        <HealthBadge />
      </header>
      <main>
        <UsersTablePage />
      </main>
    </>
  );
}
