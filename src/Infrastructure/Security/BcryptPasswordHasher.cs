using Application.Users;

namespace Infrastructure.Security;

/// <summary>
/// Hashing de contraseñas con bcrypt (hash de un solo sentido con salt, SEC-2).
/// bcrypt genera y embebe un salt único por hash automáticamente.
/// </summary>
public sealed class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string plainTextPassword) =>
        BCrypt.Net.BCrypt.HashPassword(plainTextPassword);

    public bool Verify(string plainTextPassword, string hash) =>
        BCrypt.Net.BCrypt.Verify(plainTextPassword, hash);
}
