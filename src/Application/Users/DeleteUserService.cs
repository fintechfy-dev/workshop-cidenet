using Application.Monitoring;
using Domain.Monitoring;
using Domain.Users;

namespace Application.Users;

/// <summary>
/// Orquesta la eliminación lógica de una cuenta (US-004): protege al último Admin
/// (R1) y aplica el borrado lógico, que vive como invariante en <see cref="User"/>.
///
/// MON-2: tras una eliminación exitosa, si quedan pocos Admin activos (umbral
/// configurable), se emite una alerta de riesgo de gobernanza.
/// </summary>
public sealed class DeleteUserService
{
    private readonly IUserRepository _users;
    private readonly AlertService _alerts;
    private readonly MonitoringOptions _options;

    public DeleteUserService(IUserRepository users, AlertService alerts, MonitoringOptions options)
    {
        _users = users;
        _alerts = alerts;
        _options = options;
    }

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

        var activos = await _users.CountActiveByRoleAsync(UserRole.Admin, ct);
        if (activos <= _options.LowActiveAdminThreshold)
        {
            await _alerts.RaiseAsync(
                AlertType.LowActiveAdmins,
                $"Quedan solo {activos} Admin(es) activo(s) en el sistema.",
                ct);
        }

        return DeleteUserResult.Deleted();
    }
}
