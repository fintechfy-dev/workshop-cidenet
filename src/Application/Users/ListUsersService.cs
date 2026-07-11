using System.Linq;
using Domain.Users;

namespace Application.Users;

/// <summary>
/// Orquesta la consulta paginada de usuarios (US-002): normaliza paginación,
/// interpreta los filtros opcionales y mapea a DTOs (sin exponer la contraseña).
/// </summary>
public sealed class ListUsersService
{
    private const int DefaultPageSize = 10;

    private readonly IUserRepository _users;

    public ListUsersService(IUserRepository users) => _users = users;

    public async Task<PagedUsersResult> ListAsync(ListUsersQuery query, CancellationToken ct = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? DefaultPageSize : query.PageSize;

        var role = TryParseRole(query.Rol);
        var status = TryParseStatus(query.Estado);

        var (items, total) = await _users.ListAsync(query.Search, role, status, page, pageSize, ct);

        var dtos = items
            .Select(u => new UserDto(u.Id, u.Name, u.Email, u.Role.ToString(), u.Status.ToString()))
            .ToList();

        return new PagedUsersResult(dtos, total, page, pageSize);
    }

    // Un valor de filtro inválido o vacío se interpreta como "sin filtrar" por ese campo.
    private static UserRole? TryParseRole(string? raw) =>
        !string.IsNullOrWhiteSpace(raw) && Enum.TryParse<UserRole>(raw.Trim(), true, out var r) && Enum.IsDefined(r)
            ? r
            : null;

    private static UserStatus? TryParseStatus(string? raw) =>
        !string.IsNullOrWhiteSpace(raw) && Enum.TryParse<UserStatus>(raw.Trim(), true, out var s) && Enum.IsDefined(s)
            ? s
            : null;
}
