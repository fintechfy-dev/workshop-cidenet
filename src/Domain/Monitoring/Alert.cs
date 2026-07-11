using Domain.Common;

namespace Domain.Monitoring;

/// <summary>
/// Un evento observable emitido hacia operación (US-001-MON). CreatedAt (de
/// <see cref="Entity"/>) es su marca de tiempo.
/// </summary>
public class Alert : Entity
{
    public AlertType Type { get; private set; }
    public string Message { get; private set; } = null!;

    private Alert()
    {
        // Requerido por EF Core para materializar.
    }

    private Alert(AlertType type, string message)
    {
        Type = type;
        Message = message;
    }

    public static Alert Create(AlertType type, string message) => new(type, message);
}
