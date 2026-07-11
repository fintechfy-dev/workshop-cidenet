using Domain.Common;

namespace Domain.Users;

/// <summary>
/// Cuenta de usuario. Entidad rica: sus invariantes viven aquí, no en servicios.
/// - Nombre entre 2 y 100 caracteres (se recorta).
/// - Exactamente un rol (R5, modelo single-role).
/// - Guarda únicamente el hash de la contraseña, nunca el texto plano (SEC-2).
/// </summary>
public class User : Entity
{
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public UserRole Role { get; private set; }
    public UserStatus Status { get; private set; }
    public string PasswordHash { get; private set; } = null!;
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private User()
    {
        // Requerido por EF Core para materializar.
    }

    private User(string name, string email, UserRole role, UserStatus status, string passwordHash)
    {
        Name = name;
        Email = email;
        Role = role;
        Status = status;
        PasswordHash = passwordHash;
    }

    /// <summary>
    /// Crea una cuenta aplicando sus invariantes. Recibe el email ya normalizado
    /// (value object <see cref="Domain.Users.Email"/>) y el hash ya calculado.
    /// </summary>
    public static User Create(
        string? name, Email email, UserRole role, UserStatus status, string passwordHash)
    {
        var trimmedName = (name ?? string.Empty).Trim();

        if (trimmedName.Length < 2 || trimmedName.Length > 100)
        {
            throw new DomainValidationException(
                "El nombre debe tener entre 2 y 100 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainValidationException("La contraseña es obligatoria.");
        }

        return new User(trimmedName, email.Value, role, status, passwordHash);
    }

    public void Rename(string? name)
    {
        var trimmedName = (name ?? string.Empty).Trim();

        if (trimmedName.Length < 2 || trimmedName.Length > 100)
        {
            throw new DomainValidationException(
                "El nombre debe tener entre 2 y 100 caracteres.");
        }

        Name = trimmedName;
        Touch();
    }

    public void ChangeEmail(Email email)
    {
        Email = email.Value;
        Touch();
    }

    public void ChangeRole(UserRole role)
    {
        Role = role;
        Touch();
    }

    public void ChangeStatus(UserStatus status)
    {
        Status = status;
        Touch();
    }

    /// <summary>Cambia solo la contraseña (US-005), tras verificar la actual en el servicio de aplicación.</summary>
    public void ChangePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        Touch();
    }

    /// <summary>Borrado lógico (US-004-AUD): el registro nunca se borra físicamente.</summary>
    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        Touch();
    }

    /// <summary>
    /// Reactiva una cuenta previamente eliminada al recrear su email (US-001-EDGE5),
    /// en vez de duplicarla.
    /// </summary>
    public void Reactivate(string? name, Email email, UserRole role, UserStatus status, string passwordHash)
    {
        IsDeleted = false;
        DeletedAt = null;
        Rename(name);
        ChangeEmail(email);
        ChangeRole(role);
        ChangeStatus(status);
        PasswordHash = passwordHash;
        Touch();
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;
}
