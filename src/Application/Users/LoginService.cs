using Domain.Users;

namespace Application.Users;

/// <summary>
/// Orquesta la autenticación (US-007): valida credenciales contra el hash
/// (nunca en texto plano, US-007-SEC), bloquea inactivos (R6) y responde con
/// un mensaje genérico ante credenciales inválidas para evitar enumeración de
/// usuarios (email inexistente vs. contraseña incorrecta no se distinguen).
/// </summary>
public sealed class LoginService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;

    public LoginService(IUserRepository users, IPasswordHasher passwordHasher)
    {
        _users = users;
        _passwordHasher = passwordHasher;
    }

    public async Task<LoginResult> LoginAsync(LoginCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.Email) || string.IsNullOrWhiteSpace(command.Password))
        {
            return LoginResult.Validation("El email y la contraseña son obligatorios.");
        }

        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        var user = await _users.FindByEmailAsync(normalizedEmail, ct);

        if (user is null || !_passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            return LoginResult.InvalidCredentials();
        }

        if (user.Status != UserStatus.Activo)
        {
            return LoginResult.Inactive();
        }

        return LoginResult.Success(
            new UserDto(user.Id, user.Name, user.Email, user.Role.ToString(), user.Status.ToString()));
    }
}
