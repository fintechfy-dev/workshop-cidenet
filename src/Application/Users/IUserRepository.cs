using Domain.Users;

namespace Application.Users;

/// <summary>Puerto de persistencia de usuarios (implementado en Infrastructure).</summary>
public interface IUserRepository
{
    /// <summary>¿Existe ya una cuenta con este email normalizado? (unicidad, R4).</summary>
    Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken ct = default);

    Task AddAsync(User user, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Lista usuarios aplicando búsqueda parcial (nombre/email, case-insensitive),
    /// filtros opcionales por rol y estado, y paginación. Devuelve la página y el total.
    /// </summary>
    Task<(IReadOnlyList<User> Items, int Total)> ListAsync(
        string? search,
        UserRole? role,
        UserStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
