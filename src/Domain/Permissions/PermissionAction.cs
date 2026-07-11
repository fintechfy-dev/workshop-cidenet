using Domain.Common;

namespace Domain.Permissions;

/// <summary>Acciones CRUD que se pueden permitir o no sobre un recurso (caso, sección 3).</summary>
public enum PermissionAction
{
    Create,
    Read,
    Update,
    Delete
}

public static class PermissionActionParser
{
    public static PermissionAction Parse(string? raw)
    {
        if (!string.IsNullOrWhiteSpace(raw)
            && Enum.TryParse<PermissionAction>(raw.Trim(), ignoreCase: true, out var action)
            && Enum.IsDefined(action))
        {
            return action;
        }

        throw new DomainValidationException(
            "La acción debe ser una de las válidas: Create, Read, Update o Delete.");
    }
}
