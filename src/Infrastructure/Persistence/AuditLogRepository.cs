using Application.Audit;
using Domain.Audit;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _db;

    public AuditLogRepository(AppDbContext db) => _db = db;

    public async Task AppendAsync(AuditLogEntry entry, CancellationToken ct = default)
    {
        await _db.AuditLogEntries.AddAsync(entry, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<AuditLogEntry>> GetAllAsync(CancellationToken ct = default) =>
        await _db.AuditLogEntries.AsNoTracking().OrderByDescending(e => e.CreatedAt).ToListAsync(ct);
}
