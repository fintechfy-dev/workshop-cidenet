using Domain.Common;

namespace Domain.Users;

/// <summary>
/// Roles predefinidos del sistema. No se crean ni se eliminan roles (regla del caso):
/// el conjunto es fijo y un usuario tiene exactamente uno.
/// </summary>
public enum UserRole
{
    Admin,
    Editor,
    Viewer
}

public static class UserRoleParser
{
    public static UserRole Parse(string? raw)
    {
        if (!string.IsNullOrWhiteSpace(raw)
            && Enum.TryParse<UserRole>(raw.Trim(), ignoreCase: true, out var role)
            && Enum.IsDefined(role))
        {
            return role;
        }

        throw new DomainValidationException(
            "El rol debe ser uno de los válidos: Admin, Editor o Viewer.");
    }
}
