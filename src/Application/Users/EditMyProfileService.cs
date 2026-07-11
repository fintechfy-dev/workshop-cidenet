using Domain.Common;
using Domain.Users;

namespace Application.Users;

/// <summary>
/// Orquesta la edición del propio perfil (US-005): alcance self (el actor solo
/// puede editar su propia cuenta, R2/R3 al no exponer rol/estado en el contrato),
/// unicidad de email y cambio de contraseña con verificación de la actual.
///
/// Nota: la identidad del actor llega hoy vía el header X-User-Id (marcador
/// provisional); se reemplaza por la identidad real de sesión en It 9–10.
/// </summary>
public sealed class EditMyProfileService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;

    public EditMyProfileService(IUserRepository users, IPasswordHasher passwordHasher)
    {
        _users = users;
        _passwordHasher = passwordHasher;
    }

    public async Task<EditMyProfileResult> EditAsync(EditMyProfileCommand command, CancellationToken ct = default)
    {
        var actor = await _users.GetByIdAsync(command.ActorId, ct);
        if (actor is null || actor.Status != UserStatus.Activo)
        {
            // US-007-USR: revalida el estado en cada request; si lo desactivaron,
            // pierde acceso de inmediato sin esperar a un mecanismo de sesión aparte.
            return EditMyProfileResult.Unauthorized();
        }

        try
        {
            var email = Email.Create(command.Email);

            if (await _users.ExistsByEmailExceptAsync(email.Value, actor.Id, ct))
            {
                return EditMyProfileResult.Conflict("El email ya está registrado por otra cuenta.");
            }

            if (!string.IsNullOrEmpty(command.NewPassword) || !string.IsNullOrEmpty(command.ConfirmNewPassword))
            {
                if (string.IsNullOrEmpty(command.CurrentPassword)
                    || !_passwordHasher.Verify(command.CurrentPassword, actor.PasswordHash))
                {
                    return EditMyProfileResult.Validation("La contraseña actual no es correcta.");
                }

                if (command.NewPassword != command.ConfirmNewPassword)
                {
                    return EditMyProfileResult.Validation("La nueva contraseña y su confirmación no coinciden.");
                }

                PasswordPolicy.Ensure(command.NewPassword);
                actor.ChangePassword(_passwordHasher.Hash(command.NewPassword!));
            }

            actor.Rename(command.Nombre);
            actor.ChangeEmail(email);

            await _users.SaveChangesAsync(ct);

            return EditMyProfileResult.Updated(
                new UserDto(actor.Id, actor.Name, actor.Email, actor.Role.ToString(), actor.Status.ToString()));
        }
        catch (DomainValidationException ex)
        {
            return EditMyProfileResult.Validation(ex.Message);
        }
    }
}
