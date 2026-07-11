using Domain.Users;

namespace Application.Users;

/// <summary>Puerto de persistencia de usuarios (implementado en Infrastructure).</summary>
public interface IUserRepository
{
    /// <summary>¿Existe ya una cuenta con este email normalizado? (unicidad, R4).</summary>
    Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken ct = default);

    Task AddAsync(User user, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
