using Application.Permissions;
using Domain.Permissions;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class PermissionRepository : IPermissionRepository
{
    private readonly AppDbContext _db;

    public PermissionRepository(AppDbContext db) => _db = db;

    public Task<bool> AnyAsync(CancellationToken ct = default) =>
        _db.Permissions.AnyAsync(ct);

    public async Task SeedAsync(IEnumerable<Permission> permissions, CancellationToken ct = default)
    {
        await _db.Permissions.AddRangeAsync(permissions, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Permissions.AsNoTracking().ToListAsync(ct);

    public Task<Permission?> FindAsync(
        UserRole role, PermissionResource resource, PermissionAction action, CancellationToken ct = default) =>
        _db.Permissions.FirstOrDefaultAsync(
            p => p.Role == role && p.Resource == resource && p.Action == action, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
