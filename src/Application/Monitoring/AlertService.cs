using Domain.Monitoring;
using Microsoft.Extensions.Logging;

namespace Application.Monitoring;

/// <summary>
/// Punto único para emitir un evento observable (US-001-MON): lo persiste
/// (queda consultable vía GET /api/monitoring/alerts) y lo loguea de forma
/// estructurada (logging estructurado, listo para engancharse a un backend de
/// alertas real).
/// </summary>
public sealed class AlertService
{
    private readonly IAlertRepository _alerts;
    private readonly ILogger<AlertService> _logger;

    public AlertService(IAlertRepository alerts, ILogger<AlertService> logger)
    {
        _alerts = alerts;
        _logger = logger;
    }

    public async Task RaiseAsync(AlertType type, string message, CancellationToken ct = default)
    {
        _logger.LogWarning("ALERTA [{AlertType}]: {Message}", type, message);
        await _alerts.AddAsync(Alert.Create(type, message), ct);
    }
}
