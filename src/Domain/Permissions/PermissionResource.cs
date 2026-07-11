using Domain.Common;

namespace Domain.Permissions;

/// <summary>Recursos del sistema sobre los que se aplican permisos (caso, sección 3).</summary>
public enum PermissionResource
{
    Users,
    Roles,
    Permissions,
    Reports
}

public static class PermissionResourceParser
{
    public static PermissionResource Parse(string? raw)
    {
        if (!string.IsNullOrWhiteSpace(raw)
            && Enum.TryParse<PermissionResource>(raw.Trim(), ignoreCase: true, out var resource)
            && Enum.IsDefined(resource))
        {
            return resource;
        }

        throw new DomainValidationException(
            "El recurso debe ser uno de los válidos: users, roles, permissions o reports.");
    }
}
