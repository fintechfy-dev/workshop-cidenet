namespace Application.Users;

/// <summary>
/// Puerto de hashing de contraseñas (implementado en Infrastructure).
/// El texto plano nunca se persiste; solo su hash de un solo sentido con salt (SEC-2).
/// </summary>
public interface IPasswordHasher
{
    string Hash(string plainTextPassword);

    /// <summary>¿El texto plano corresponde a este hash? (verificación de contraseña actual, US-005-V4).</summary>
    bool Verify(string plainTextPassword, string hash);
}
