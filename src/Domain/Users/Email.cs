using System.Text.RegularExpressions;
using Domain.Common;

namespace Domain.Users;

/// <summary>
/// Value object de email. Normaliza (trim + minúsculas) y valida formato y longitud.
/// La normalización es la base de la unicidad case-insensitive (US-001-EDGE1).
/// </summary>
public sealed class Email
{
    public const int MaxLength = 254; // RFC 5321

    private static readonly Regex Pattern = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string? raw)
    {
        var normalized = (raw ?? string.Empty).Trim().ToLowerInvariant();

        if (normalized.Length == 0)
        {
            throw new DomainValidationException("El email es obligatorio.");
        }

        if (normalized.Length > MaxLength)
        {
            throw new DomainValidationException(
                $"El email no puede superar {MaxLength} caracteres.");
        }

        if (!Pattern.IsMatch(normalized))
        {
            throw new DomainValidationException("El email no tiene un formato válido.");
        }

        return new Email(normalized);
    }

    public override string ToString() => Value;
}
