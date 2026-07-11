namespace Application.Users;

/// <summary>
/// Puerto de hashing de contraseñas (implementado en Infrastructure).
/// El texto plano nunca se persiste; solo su hash de un solo sentido con salt (SEC-2).
/// </summary>
public interface IPasswordHasher
{
    string Hash(string plainTextPassword);
}
