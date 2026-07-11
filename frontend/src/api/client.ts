import { getSession } from "../auth/session";

const API_BASE_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5000";

export async function checkHealth(): Promise<{ status: string }> {
  const response = await fetch(`${API_BASE_URL}/health`);
  if (!response.ok) {
    throw new Error(`Health check failed: ${response.status}`);
  }
  return response.json();
}

export class ApiError extends Error {
  constructor(
    message: string,
    public readonly status: number,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

function authHeaders(): HeadersInit {
  const session = getSession();
  return session ? { "X-User-Id": session.id } : {};
}

export interface UserRow {
  id: string;
  nombre: string;
  email: string;
  rol: string;
  estado: string;
}

export interface PagedUsers {
  items: UserRow[];
  total: number;
  page: number;
  pageSize: number;
}

export interface ListUsersParams {
  search?: string;
  rol?: string;
  estado?: string;
  page?: number;
}

export async function listUsers(params: ListUsersParams = {}): Promise<PagedUsers> {
  const query = new URLSearchParams();
  if (params.search) query.set("search", params.search);
  if (params.rol) query.set("rol", params.rol);
  if (params.estado) query.set("estado", params.estado);
  if (params.page) query.set("page", String(params.page));

  const response = await fetch(`${API_BASE_URL}/api/users?${query.toString()}`, {
    headers: authHeaders(),
  });

  if (!response.ok) {
    throw new ApiError("No se pudo cargar la lista de usuarios.", response.status);
  }

  return response.json();
}
