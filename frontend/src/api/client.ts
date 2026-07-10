const API_BASE_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5000";

export async function checkHealth(): Promise<{ status: string }> {
  const response = await fetch(`${API_BASE_URL}/health`);
  if (!response.ok) {
    throw new Error(`Health check failed: ${response.status}`);
  }
  return response.json();
}

// TODO (ejercicio del taller): agrega aqui los metodos del cliente que
// necesite tu caso, una vez que /discovery y /plan definan tus endpoints.
