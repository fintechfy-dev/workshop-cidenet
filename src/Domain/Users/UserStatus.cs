using Domain.Common;

namespace Domain.Users;

/// <summary>
/// Estado de una cuenta. Al crear, el valor por defecto es Activo.
/// Un usuario Inactivo no puede autenticarse (R6, se aplica en US-007).
/// </summary>
public enum UserStatus
{
    Activo,
    Inactivo
}

public static class UserStatusParser
{
    /// <summary>Parsea el estado; si viene vacío, aplica el default Activo.</summary>
    public static UserStatus Parse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return UserStatus.Activo;
        }

        if (Enum.TryParse<UserStatus>(raw.Trim(), ignoreCase: true, out var status)
            && Enum.IsDefined(status))
        {
            return status;
        }

        throw new DomainValidationException("El estado debe ser Activo o Inactivo.");
    }
}
