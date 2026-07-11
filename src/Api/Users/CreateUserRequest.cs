namespace Api.Users;

/// <summary>Cuerpo JSON del POST /api/users (US-001).</summary>
public sealed record CreateUserRequest(
    string? Nombre,
    string? Email,
    string? Password,
    string? ConfirmPassword,
    string? Rol,
    string? Estado);
