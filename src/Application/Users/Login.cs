namespace Application.Users;

public sealed record LoginCommand(string? Email, string? Password);

public enum LoginOutcome
{
    Success,
    ValidationFailed,
    InvalidCredentials,
    InactiveAccount
}

public sealed record LoginResult(LoginOutcome Outcome, UserDto? User, string? Error)
{
    public static LoginResult Success(UserDto user) =>
        new(LoginOutcome.Success, user, null);

    public static LoginResult Validation(string error) =>
        new(LoginOutcome.ValidationFailed, null, error);

    public static LoginResult InvalidCredentials() =>
        new(LoginOutcome.InvalidCredentials, null, "El email o la contraseña son incorrectos.");

    public static LoginResult Inactive() =>
        new(LoginOutcome.InactiveAccount, null, "La cuenta está inactiva. Contacte al administrador.");
}
