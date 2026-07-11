using Domain.Permissions;
using Domain.Users;

namespace Application.Permissions;

/// <summary>Puerto de persistencia de la matriz de permisos (implementado en Infrastructure).</summary>
public interface IPermissionRepository
{
    Task<bool> AnyAsync(CancellationToken ct = default);

    Task SeedAsync(IEnumerable<Permission> permissions, CancellationToken ct = default);

    Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken ct = default);

    Task<Permission?> FindAsync(
        UserRole role, PermissionResource resource, PermissionAction action, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
