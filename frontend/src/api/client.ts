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

async function readErrorMessage(response: Response, fallback: string): Promise<string> {
  try {
    const body = (await response.json()) as { error?: string };
    return body?.error ?? fallback;
  } catch {
    return fallback;
  }
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

export interface CreateUserPayload {
  nombre: string;
  email: string;
  password: string;
  confirmPassword: string;
  rol: string;
  estado: string;
}

export interface EditUserPayload {
  nombre: string;
  email: string;
  rol: string;
  estado: string;
}

export async function createUser(payload: CreateUserPayload): Promise<UserRow> {
  const response = await fetch(`${API_BASE_URL}/api/users`, {
    method: "POST",
    headers: { "Content-Type": "application/json", ...authHeaders() },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    throw new ApiError(await readErrorMessage(response, "No se pudo crear el usuario."), response.status);
  }

  return response.json();
}

export async function updateUser(id: string, payload: EditUserPayload): Promise<UserRow> {
  const response = await fetch(`${API_BASE_URL}/api/users/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json", ...authHeaders() },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    throw new ApiError(await readErrorMessage(response, "No se pudo actualizar el usuario."), response.status);
  }

  return response.json();
}

export async function deleteUser(id: string): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/api/users/${id}`, {
    method: "DELETE",
    headers: authHeaders(),
  });

  if (response.status === 204) {
    return;
  }

  throw new ApiError(await readErrorMessage(response, "No se pudo eliminar el usuario."), response.status);
}

export async function login(email: string, password: string): Promise<UserRow> {
  const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password }),
  });

  if (!response.ok) {
    throw new ApiError(await readErrorMessage(response, "No se pudo iniciar sesión."), response.status);
  }

  return response.json();
}

export interface PermissionCell {
  rol: string;
  recurso: string;
  accion: string;
  permitido: boolean;
}

export interface PermissionChange {
  rol: string;
  recurso: string;
  accion: string;
  permitido: boolean;
}

export async function getPermissionMatrix(): Promise<PermissionCell[]> {
  const response = await fetch(`${API_BASE_URL}/api/permissions`, {
    headers: authHeaders(),
  });

  if (!response.ok) {
    throw new ApiError(await readErrorMessage(response, "No se pudo cargar la matriz de permisos."), response.status);
  }

  return response.json();
}

export async function updatePermissions(cambios: PermissionChange[]): Promise<PermissionCell[]> {
  const response = await fetch(`${API_BASE_URL}/api/permissions`, {
    method: "PUT",
    headers: { "Content-Type": "application/json", ...authHeaders() },
    body: JSON.stringify({ cambios }),
  });

  if (!response.ok) {
    throw new ApiError(await readErrorMessage(response, "No se pudo actualizar el permiso."), response.status);
  }

  return response.json();
}
