using Application.Monitoring;
using Domain.Common;
using Domain.Monitoring;
using Domain.Users;

namespace Application.Users;

/// <summary>
/// Orquesta la edición de una cuenta (US-003): valida entrada, aplica unicidad de
/// email (excluyendo la propia), protege al último Admin y persiste los cambios.
/// Las invariantes de datos viven en el dominio (Email, User).
///
/// MON-2: tras cualquier edición exitosa, si quedan pocos Admin activos (umbral
/// configurable), se emite una alerta de riesgo de gobernanza.
/// </summary>
public sealed class EditUserService
{
    private readonly IUserRepository _users;
    private readonly AlertService _alerts;
    private readonly MonitoringOptions _options;

    public EditUserService(IUserRepository users, AlertService alerts, MonitoringOptions options)
    {
        _users = users;
        _alerts = alerts;
        _options = options;
    }

    public async Task<EditUserResult> EditAsync(Guid id, EditUserCommand command, CancellationToken ct = default)
    {
        try
        {
            var user = await _users.GetByIdAsync(id, ct);
            if (user is null)
            {
                return EditUserResult.NotFound();
            }

            var email = Email.Create(command.Email);
            var role = UserRoleParser.Parse(command.Rol);
            var status = UserStatusParser.Parse(command.Estado);

            if (await _users.ExistsByEmailExceptAsync(email.Value, id, ct))
            {
                return EditUserResult.Conflict("El email ya está registrado por otra cuenta.");
            }

            // Protección del último Admin (extensión de R1): no dejar al sistema sin administradores.
            if (user.Role == UserRole.Admin && role != UserRole.Admin)
            {
                var admins = await _users.CountByRoleAsync(UserRole.Admin, ct);
                if (admins <= 1)
                {
                    return EditUserResult.Conflict(
                        "No se puede quitar el rol Admin al último administrador del sistema.");
                }
            }

            user.Rename(command.Nombre);
            user.ChangeEmail(email);
            user.ChangeRole(role);
            user.ChangeStatus(status);

            await _users.SaveChangesAsync(ct);
            await AlertarSiQuedanPocosAdminActivosAsync(ct);

            return EditUserResult.Updated(
                new UserDto(user.Id, user.Name, user.Email, user.Role.ToString(), user.Status.ToString()));
        }
        catch (DomainValidationException ex)
        {
            return EditUserResult.Validation(ex.Message);
        }
    }

    private async Task AlertarSiQuedanPocosAdminActivosAsync(CancellationToken ct)
    {
        var activos = await _users.CountActiveByRoleAsync(UserRole.Admin, ct);
        if (activos <= _options.LowActiveAdminThreshold)
        {
            await _alerts.RaiseAsync(
                AlertType.LowActiveAdmins,
                $"Quedan solo {activos} Admin(es) activo(s) en el sistema.",
                ct);
        }
    }
}
