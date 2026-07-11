using Application.Monitoring;
using Domain.Monitoring;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class AlertRepository : IAlertRepository
{
    private readonly AppDbContext _db;

    public AlertRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(Alert alert, CancellationToken ct = default)
    {
        await _db.Alerts.AddAsync(alert, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Alert>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Alerts.AsNoTracking().OrderByDescending(a => a.CreatedAt).ToListAsync(ct);
}
