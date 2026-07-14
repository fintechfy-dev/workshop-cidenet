namespace Application.Users;

/// <summary>
/// Siembra el Admin inicial (MNT-1) cuando el sistema arranca con una base
/// vacía — sin él, nadie podría crear la primera cuenta (solo Admin puede).
/// Reutiliza CreateUserService para no duplicar las invariantes de creación.
/// </summary>
public sealed class AdminSeedService
{
    private readonly IUserRepository _users;
    private readonly CreateUserService _createUser;
    private readonly SeedOptions _options;

    public AdminSeedService(IUserRepository users, CreateUserService createUser, SeedOptions options)
    {
        _users = users;
        _createUser = createUser;
        _options = options;
    }

    public async Task SeedIfNeededAsync(CancellationToken ct = default)
    {
        if (await _users.AnyAsync(ct))
        {
            return;
        }

        await _createUser.CreateAsync(new CreateUserCommand(
            _options.AdminName,
            _options.AdminEmail,
            _options.AdminPassword,
            _options.AdminPassword,
            "Admin",
            "Activo"), ct);
    }
}
