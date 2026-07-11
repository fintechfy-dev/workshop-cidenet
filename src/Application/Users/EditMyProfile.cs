namespace Application.Users;

/// <summary>
/// Datos de entrada para editar el propio perfil (US-005). Rol y estado NO forman
/// parte del contrato: son de solo lectura (R2/R3) y no se pueden cambiar desde aquí.
/// </summary>
public sealed record EditMyProfileCommand(
    Guid ActorId,
    string? Nombre,
    string? Email,
    string? CurrentPassword,
    string? NewPassword,
    string? ConfirmNewPassword);

public enum EditMyProfileOutcome
{
    Updated,
    ValidationFailed,
    Conflict,
    Unauthorized
}

public sealed record EditMyProfileResult(EditMyProfileOutcome Outcome, UserDto? User, string? Error)
{
    public static EditMyProfileResult Updated(UserDto user) =>
        new(EditMyProfileOutcome.Updated, user, null);

    public static EditMyProfileResult Validation(string error) =>
        new(EditMyProfileOutcome.ValidationFailed, null, error);

    public static EditMyProfileResult Conflict(string error) =>
        new(EditMyProfileOutcome.Conflict, null, error);

    public static EditMyProfileResult Unauthorized() =>
        new(EditMyProfileOutcome.Unauthorized, null, null);
}
