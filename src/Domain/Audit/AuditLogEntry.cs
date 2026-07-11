using Domain.Common;

namespace Domain.Audit;

/// <summary>
/// Una entrada del registro de auditoría (US-001-AUD): quién hizo qué, sobre
/// qué entidad y cuándo (CreatedAt de <see cref="Entity"/> es la marca de
/// tiempo). Append-only por diseño: no expone ningún método para modificarse
/// una vez creada, y <see cref="Application.Audit.IAuditLogRepository"/> no
/// declara operaciones de edición ni borrado.
/// </summary>
public class AuditLogEntry : Entity
{
    public Guid? ActorId { get; private set; }
    public string Action { get; private set; } = null!;
    public string EntityType { get; private set; } = null!;
    public string? EntityId { get; private set; }

    private AuditLogEntry()
    {
        // Requerido por EF Core para materializar.
    }

    private AuditLogEntry(Guid? actorId, string action, string entityType, string? entityId)
    {
        ActorId = actorId;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
    }

    public static AuditLogEntry Create(Guid? actorId, string action, string entityType, string? entityId) =>
        new(actorId, action, entityType, entityId);
}
