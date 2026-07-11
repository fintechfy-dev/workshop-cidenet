namespace Api.Users;

/// <summary>Cuerpo JSON del PUT /api/users/{id} (US-003). La contraseña no se edita aquí.</summary>
public sealed record EditUserRequest(
    string? Nombre,
    string? Email,
    string? Rol,
    string? Estado);
