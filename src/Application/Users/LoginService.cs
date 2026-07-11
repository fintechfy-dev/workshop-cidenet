using Application.Monitoring;
using Domain.Monitoring;
using Domain.Users;

namespace Application.Users;

/// <summary>
/// Orquesta la autenticación (US-007): valida credenciales contra el hash
/// (nunca en texto plano, US-007-SEC), bloquea inactivos (R6) y responde con
/// un mensaje genérico ante credenciales inválidas para evitar enumeración de
/// usuarios (email inexistente vs. contraseña incorrecta no se distinguen).
///
/// MON-1: cada credencial inválida cuenta como intento fallido; si se supera
/// el umbral dentro de la ventana configurada, se emite una alerta (posible
/// fuerza bruta).
/// </summary>
public sealed class LoginService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IFailedLoginTracker _failedLogins;
    private readonly AlertService _alerts;
    private readonly MonitoringOptions _options;

    public LoginService(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IFailedLoginTracker failedLogins,
        AlertService alerts,
        MonitoringOptions options)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _failedLogins = failedLogins;
        _alerts = alerts;
        _options = options;
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
            await RegistrarFalloYAlertarSiCorrespondeAsync(normalizedEmail, ct);
            return LoginResult.InvalidCredentials();
        }

        if (user.Status != UserStatus.Activo)
        {
            return LoginResult.Inactive();
        }

        return LoginResult.Success(
            new UserDto(user.Id, user.Name, user.Email, user.Role.ToString(), user.Status.ToString()));
    }

    private async Task RegistrarFalloYAlertarSiCorrespondeAsync(string normalizedEmail, CancellationToken ct)
    {
        await _failedLogins.RecordAsync(normalizedEmail, ct);

        var recientes = await _failedLogins.CountRecentAsync(
            normalizedEmail, TimeSpan.FromMinutes(_options.FailedLoginWindowMinutes), ct);

        if (recientes >= _options.FailedLoginThreshold)
        {
            await _alerts.RaiseAsync(
                AlertType.LoginBruteForce,
                $"Posible fuerza bruta: {recientes} intentos fallidos para \"{normalizedEmail}\" " +
                $"en los últimos {_options.FailedLoginWindowMinutes} minuto(s).",
                ct);
        }
    }
}
