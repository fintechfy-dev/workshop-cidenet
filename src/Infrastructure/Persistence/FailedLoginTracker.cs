using Application.Monitoring;
using Domain.Monitoring;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class FailedLoginTracker : IFailedLoginTracker
{
    private readonly AppDbContext _db;

    public FailedLoginTracker(AppDbContext db) => _db = db;

    public async Task RecordAsync(string normalizedEmail, CancellationToken ct = default)
    {
        await _db.FailedLoginAttempts.AddAsync(FailedLoginAttempt.Create(normalizedEmail), ct);
        await _db.SaveChangesAsync(ct);
    }

    public Task<int> CountRecentAsync(string normalizedEmail, TimeSpan window, CancellationToken ct = default)
    {
        var since = DateTime.UtcNow - window;
        return _db.FailedLoginAttempts.CountAsync(
            a => a.NormalizedEmail == normalizedEmail && a.CreatedAt >= since, ct);
    }
}
