namespace Application.Users;

/// <summary>Datos de entrada para editar una cuenta (US-003). La contraseña no se edita aquí.</summary>
public sealed record EditUserCommand(
    string? Nombre,
    string? Email,
    string? Rol,
    string? Estado);

public enum EditUserOutcome
{
    Updated,
    ValidationFailed,
    Conflict,
    NotFound
}

public sealed record EditUserResult(EditUserOutcome Outcome, UserDto? User, string? Error)
{
    public static EditUserResult Updated(UserDto user) =>
        new(EditUserOutcome.Updated, user, null);

    public static EditUserResult Validation(string error) =>
        new(EditUserOutcome.ValidationFailed, null, error);

    public static EditUserResult Conflict(string error) =>
        new(EditUserOutcome.Conflict, null, error);

    public static EditUserResult NotFound() =>
        new(EditUserOutcome.NotFound, null, null);
}
