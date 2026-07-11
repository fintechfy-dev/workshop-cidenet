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

    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Busca por email normalizado, excluyendo cuentas eliminadas (para autenticación, US-007).</summary>
    Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken ct = default);

    /// <summary>¿Otro usuario (distinto de excludeId) ya usa este email normalizado?</summary>
    Task<bool> ExistsByEmailExceptAsync(string normalizedEmail, Guid excludeId, CancellationToken ct = default);

    /// <summary>Cuántos usuarios tienen el rol dado (p. ej. para proteger al último Admin).</summary>
    Task<int> CountByRoleAsync(UserRole role, CancellationToken ct = default);

    /// <summary>
    /// Busca por email normalizado incluyendo cuentas eliminadas lógicamente
    /// (para detectar si hay que reactivar en vez de crear una duplicada, US-001-EDGE5).
    /// </summary>
    Task<User?> GetByEmailIncludingDeletedAsync(string normalizedEmail, CancellationToken ct = default);
}
