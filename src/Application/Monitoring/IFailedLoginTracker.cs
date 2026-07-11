namespace Application.Monitoring;

/// <summary>Rastrea intentos de login fallidos por email, para detectar fuerza bruta (MON-1).</summary>
public interface IFailedLoginTracker
{
    Task RecordAsync(string normalizedEmail, CancellationToken ct = default);

    Task<int> CountRecentAsync(string normalizedEmail, TimeSpan window, CancellationToken ct = default);
}
