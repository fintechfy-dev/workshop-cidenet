import { useEffect, useState } from "react";
import { checkHealth } from "./api/client";
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
        <p>
          Tu app va aquí. La construyes durante el taller, a partir de la spec
          que salga de tu discovery.
        </p>
      </main>
    </>
  );
}
