using Domain.Common;
using Domain.Users;

namespace Application.Users;

/// <summary>
/// Orquesta la creación de una cuenta (US-001): valida entrada, aplica unicidad,
/// hashea la contraseña y persiste. Las invariantes de datos viven en el dominio
/// (Email, PasswordPolicy, User); aquí se coordina el flujo y se mapea el resultado.
/// </summary>
public sealed class CreateUserService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserService(IUserRepository users, IPasswordHasher passwordHasher)
    {
        _users = users;
        _passwordHasher = passwordHasher;
    }

    public async Task<CreateUserResult> CreateAsync(CreateUserCommand command, CancellationToken ct = default)
    {
        try
        {
            if (command.Password != command.ConfirmPassword)
            {
                return CreateUserResult.Validation("La contraseña y su confirmación no coinciden.");
            }

            var email = Email.Create(command.Email);
            PasswordPolicy.Ensure(command.Password);
            var role = UserRoleParser.Parse(command.Rol);
            var status = UserStatusParser.Parse(command.Estado);

            if (await _users.ExistsByEmailAsync(email.Value, ct))
            {
                return CreateUserResult.Conflict("El email ya está registrado.");
            }

            var passwordHash = _passwordHasher.Hash(command.Password!);
            var user = User.Create(command.Nombre, email, role, status, passwordHash);

            await _users.AddAsync(user, ct);
            await _users.SaveChangesAsync(ct);

            return CreateUserResult.Created(
                new UserDto(user.Id, user.Name, user.Email, user.Role.ToString(), user.Status.ToString()));
        }
        catch (DomainValidationException ex)
        {
            return CreateUserResult.Validation(ex.Message);
        }
    }
}
