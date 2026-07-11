using System.Linq;
using Domain.Common;

namespace Domain.Users;

/// <summary>
/// Política de contraseña del caso: mínimo 8 caracteres, con al menos una
/// mayúscula, una minúscula y un número. Valida el texto plano ANTES de hashear.
/// </summary>
public static class PasswordPolicy
{
    public const int MinLength = 8;

    public static void Ensure(string? password)
    {
        if (string.IsNullOrEmpty(password)
            || password.Length < MinLength
            || !password.Any(char.IsUpper)
            || !password.Any(char.IsLower)
            || !password.Any(char.IsDigit))
        {
            throw new DomainValidationException(
                "La contraseña debe tener al menos 8 caracteres, con una mayúscula, una minúscula y un número.");
        }
    }
}
