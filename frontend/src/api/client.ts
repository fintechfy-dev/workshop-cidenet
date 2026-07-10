const API_BASE_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5000";

export async function checkHealth(): Promise<{ status: string }> {
  const response = await fetch(`${API_BASE_URL}/health`);
  if (!response.ok) {
    throw new Error(`Health check failed: ${response.status}`);
  }
  return response.json();
}

// TODO (ejercicio del taller): agregar aqui los metodos reales del cliente
// (listUsers, createUser, updateUser, deleteUser, listRoles, updatePermissions...)
// una vez que /discovery y /plan definan los endpoints del modulo.
