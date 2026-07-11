namespace Application.Users;

/// <summary>Datos de entrada para crear una cuenta (en lenguaje del caso).</summary>
public sealed record CreateUserCommand(
    string? Nombre,
    string? Email,
    string? Password,
    string? ConfirmPassword,
    string? Rol,
    string? Estado);

public enum CreateUserOutcome
{
    Created,
    ValidationFailed,
    EmailAlreadyExists
}

/// <summary>Vista de una cuenta hacia afuera. Nunca incluye la contraseña (SEC-1).</summary>
public sealed record UserDto(Guid Id, string Nombre, string Email, string Rol, string Estado);

public sealed record CreateUserResult(CreateUserOutcome Outcome, UserDto? User, string? Error)
{
    public static CreateUserResult Created(UserDto user) =>
        new(CreateUserOutcome.Created, user, null);

    public static CreateUserResult Validation(string error) =>
        new(CreateUserOutcome.ValidationFailed, null, error);

    public static CreateUserResult Conflict(string error) =>
        new(CreateUserOutcome.EmailAlreadyExists, null, error);
}
