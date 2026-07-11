using Domain.Common;
using Domain.Users;

namespace Domain.Permissions;

/// <summary>
/// Una celda de la matriz de permisos: si el rol dado puede ejecutar la acción
/// dada sobre el recurso dado. Roles y recursos son fijos (V1); solo el estado
/// (Allowed) cambia.
/// </summary>
public class Permission : Entity
{
    public UserRole Role { get; private set; }
    public PermissionResource Resource { get; private set; }
    public PermissionAction Action { get; private set; }
    public bool Allowed { get; private set; }

    private Permission()
    {
        // Requerido por EF Core para materializar.
    }

    private Permission(UserRole role, PermissionResource resource, PermissionAction action, bool allowed)
    {
        Role = role;
        Resource = resource;
        Action = action;
        Allowed = allowed;
    }

    public static Permission Create(UserRole role, PermissionResource resource, PermissionAction action, bool allowed) =>
        new(role, resource, action, allowed);

    public void SetAllowed(bool allowed)
    {
        Allowed = allowed;
        UpdatedAt = DateTime.UtcNow;
    }
}
