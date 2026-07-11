namespace Api.Permissions;

/// <summary>Un cambio individual de celda en el cuerpo JSON del PUT /api/permissions.</summary>
public sealed record PermissionChangeRequest(string? Rol, string? Recurso, string? Accion, bool Permitido);

/// <summary>Cuerpo JSON del PUT /api/permissions (US-006): lote de cambios a aplicar.</summary>
public sealed record EditPermissionsRequest(List<PermissionChangeRequest>? Cambios);
