using Application.Users;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db) => _db = db;

    public Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken ct = default) =>
        _db.Users.AnyAsync(u => u.Email == normalizedEmail, ct);

    public async Task AddAsync(User user, CancellationToken ct = default) =>
        await _db.Users.AddAsync(user, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

    public async Task<(IReadOnlyList<User> Items, int Total)> ListAsync(
        string? search,
        UserRole? role,
        UserStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        IQueryable<User> query = _db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(u =>
                u.Name.ToLower().Contains(term) || u.Email.ToLower().Contains(term));
        }

        if (role.HasValue)
        {
            query = query.Where(u => u.Role == role.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(u => u.Status == status.Value);
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(u => u.Name)
            .ThenBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<bool> ExistsByEmailExceptAsync(string normalizedEmail, Guid excludeId, CancellationToken ct = default) =>
        _db.Users.AnyAsync(u => u.Email == normalizedEmail && u.Id != excludeId, ct);

    public Task<int> CountByRoleAsync(UserRole role, CancellationToken ct = default) =>
        _db.Users.CountAsync(u => u.Role == role, ct);
}
