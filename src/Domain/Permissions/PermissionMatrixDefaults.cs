using Domain.Users;

namespace Domain.Permissions;

/// <summary>
/// Matriz de permisos por defecto del sistema (caso, sección 4), usada para
/// sembrar la tabla la primera vez que se consulta o edita.
/// </summary>
public static class PermissionMatrixDefaults
{
    private static readonly PermissionResource[] Resources =
    [
        PermissionResource.Users, PermissionResource.Roles,
        PermissionResource.Permissions, PermissionResource.Reports
    ];

    private static readonly PermissionAction[] Actions =
    [
        PermissionAction.Create, PermissionAction.Read,
        PermissionAction.Update, PermissionAction.Delete
    ];

    public static IReadOnlyList<Permission> Build()
    {
        var permissions = new List<Permission>();

        foreach (var resource in Resources)
        {
            foreach (var action in Actions)
            {
                permissions.Add(Permission.Create(UserRole.Admin, resource, action, allowed: true));
                permissions.Add(Permission.Create(UserRole.Editor, resource, action, allowed: IsEditorAllowed(resource, action)));
                permissions.Add(Permission.Create(UserRole.Viewer, resource, action, allowed: IsViewerAllowed(resource, action)));
            }
        }

        return permissions;
    }

    private static bool IsEditorAllowed(PermissionResource resource, PermissionAction action) =>
        resource == PermissionResource.Reports || action == PermissionAction.Read;

    private static bool IsViewerAllowed(PermissionResource resource, PermissionAction action) =>
        resource == PermissionResource.Reports && action == PermissionAction.Read;
}
