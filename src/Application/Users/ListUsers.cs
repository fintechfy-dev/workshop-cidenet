namespace Application.Users;

/// <summary>Consulta de usuarios con búsqueda, filtros y paginación (US-002).</summary>
public sealed record ListUsersQuery(
    string? Search,
    string? Rol,
    string? Estado,
    int Page,
    int PageSize);

/// <summary>Página de resultados. Los items nunca incluyen la contraseña (SEC-1).</summary>
public sealed record PagedUsersResult(
    IReadOnlyList<UserDto> Items,
    int Total,
    int Page,
    int PageSize);
