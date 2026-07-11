using Domain.Common;

namespace Domain.Monitoring;

/// <summary>Un intento de login fallido, para detectar picos de fuerza bruta (MON-1).</summary>
public class FailedLoginAttempt : Entity
{
    public string NormalizedEmail { get; private set; } = null!;

    private FailedLoginAttempt()
    {
        // Requerido por EF Core para materializar.
    }

    private FailedLoginAttempt(string normalizedEmail) => NormalizedEmail = normalizedEmail;

    public static FailedLoginAttempt Create(string normalizedEmail) => new(normalizedEmail);
}
