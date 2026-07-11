using Domain.Common;
using Domain.Permissions;
using Domain.Users;

namespace Application.Permissions;

/// <summary>
/// Orquesta la matriz de permisos (US-006): la siembra con los valores por
/// defecto del caso la primera vez que se consulta o edita, expone su estado
/// y aplica cambios individuales.
///
/// R3 + anti-lockout (US-006-V2 y US-006-SEC-ERR1) se resuelven con una sola
/// regla: los permisos del rol Admin son inmutables desde este endpoint. Como
/// solo un Admin puede llegar aquí (US-006-SEC, exigido en It 9–10), "el rol
/// que él mismo posee" es siempre Admin — bloquear cualquier cambio a esa fila
/// evita a la vez la auto-asignación de privilegios y el auto-bloqueo del
/// sistema, sin necesitar todavía la identidad puntual del actor.
///
/// Nota: "un cambio se refleja en la autorización efectiva" (que otros
/// endpoints consulten esta matriz) es transversal y se cierra en It 10.
/// </summary>
public sealed class PermissionsService
{
    private readonly IPermissionRepository _permissions;

    public PermissionsService(IPermissionRepository permissions) => _permissions = permissions;

    public async Task<IReadOnlyList<PermissionCellDto>> GetMatrixAsync(CancellationToken ct = default)
    {
        await EnsureSeededAsync(ct);
        return ToDto(await _permissions.GetAllAsync(ct));
    }

    public async Task<EditPermissionsResult> EditMatrixAsync(
        IReadOnlyList<PermissionChangeCommand> changes, CancellationToken ct = default)
    {
        await EnsureSeededAsync(ct);

        var parsed = new List<(UserRole Role, PermissionResource Resource, PermissionAction Action, bool Allowed)>();
        try
        {
            foreach (var change in changes)
            {
                parsed.Add((
                    UserRoleParser.Parse(change.Rol),
                    PermissionResourceParser.Parse(change.Recurso),
                    PermissionActionParser.Parse(change.Accion),
                    change.Permitido));
            }
        }
        catch (DomainValidationException ex)
        {
            return EditPermissionsResult.Validation(ex.Message);
        }

        if (parsed.Any(c => c.Role == UserRole.Admin))
        {
            return EditPermissionsResult.Conflict("No se pueden modificar los permisos del rol Admin.");
        }

        foreach (var (role, resource, action, allowed) in parsed)
        {
            var permission = await _permissions.FindAsync(role, resource, action, ct);
            permission!.SetAllowed(allowed);
        }

        await _permissions.SaveChangesAsync(ct);

        return EditPermissionsResult.Updated(ToDto(await _permissions.GetAllAsync(ct)));
    }

    private async Task EnsureSeededAsync(CancellationToken ct)
    {
        if (!await _permissions.AnyAsync(ct))
        {
            await _permissions.SeedAsync(PermissionMatrixDefaults.Build(), ct);
        }
    }

    private static IReadOnlyList<PermissionCellDto> ToDto(IReadOnlyList<Permission> permissions) =>
        permissions
            .OrderBy(p => p.Role).ThenBy(p => p.Resource).ThenBy(p => p.Action)
            .Select(p => new PermissionCellDto(p.Role.ToString(), p.Resource.ToString(), p.Action.ToString(), p.Allowed))
            .ToList();
}
