using Domain.Audit;

namespace Application.Audit;

/// <summary>
/// Puerto de persistencia del registro de auditoría. Deliberadamente solo
/// declara "agregar" y "leer": no hay Update ni Delete (append-only, AUD-2).
/// </summary>
public interface IAuditLogRepository
{
    Task AppendAsync(AuditLogEntry entry, CancellationToken ct = default);

    Task<IReadOnlyList<AuditLogEntry>> GetAllAsync(CancellationToken ct = default);
}
