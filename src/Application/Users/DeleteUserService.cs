using Domain.Users;

namespace Application.Users;

/// <summary>
/// Orquesta la eliminación lógica de una cuenta (US-004): protege al último Admin
/// (R1) y aplica el borrado lógico, que vive como invariante en <see cref="User"/>.
///
/// Nota: "no autoeliminación" (US-004-V3) y "solo Admin elimina" (US-004-SEC)
/// requieren la identidad del actor autenticado; se incorporan en It 9–10.
/// </summary>
public sealed class DeleteUserService
{
    private readonly IUserRepository _users;

    public DeleteUserService(IUserRepository users) => _users = users;

    public async Task<DeleteUserResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(id, ct);
        if (user is null)
        {
            return DeleteUserResult.NotFound();
        }

        if (user.Role == UserRole.Admin)
        {
            var admins = await _users.CountByRoleAsync(UserRole.Admin, ct);
            if (admins <= 1)
            {
                return DeleteUserResult.Conflict(
                    "No se puede eliminar el último administrador del sistema.");
            }
        }

        user.SoftDelete();
        await _users.SaveChangesAsync(ct);

        return DeleteUserResult.Deleted();
    }
}
