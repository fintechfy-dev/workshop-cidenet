using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>
/// Datos fijos de arranque (roles, permisos base y 5 usuarios de prueba) para que el
/// modulo tenga algo con que trabajar desde el primer /discovery. No implementa ninguna
/// regla de negocio — eso es el ejercicio del taller.
/// </summary>
public static class DbSeeder
{
    private static readonly string[] Resources = ["users", "roles", "permissions", "reports"];
    private static readonly string[] Actions = ["create", "read", "update", "delete"];

    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync())
        {
            return;
        }

        var permissions = new Dictionary<(string Resource, string Action), Permission>();
        foreach (var resource in Resources)
        {
            foreach (var action in Actions)
            {
                var permission = new Permission { Resource = resource, Action = action };
                permissions[(resource, action)] = permission;
            }
        }

        var admin = new Role { Name = "admin", Description = "Administrador con acceso total al sistema" };
        var editor = new Role { Name = "editor", Description = "Colaborador con permisos de lectura y escritura en reportes" };
        var viewer = new Role { Name = "viewer", Description = "Usuario de solo lectura" };

        // Matriz base de permisos (specs/BRIEF.md, seccion 4) — punto de partida, no la solucion final.
        admin.Permissions = permissions.Values.ToList();

        editor.Permissions =
        [
            permissions[("users", "read")],
            permissions[("roles", "read")],
            permissions[("permissions", "read")],
            permissions[("reports", "create")],
            permissions[("reports", "read")],
            permissions[("reports", "update")],
            permissions[("reports", "delete")],
        ];

        viewer.Permissions = [permissions[("reports", "read")]];

        // Placeholder — no es un hash de produccion. Construir el manejo real de credenciales
        // (fuerza de password, hashing) es parte del ejercicio.
        const string placeholderHash = "seed-placeholder-hash";

        var users = new List<User>
        {
            new() { Name = "Ana García", Email = "ana@example.com", PasswordHash = placeholderHash, Status = UserStatus.Active, Roles = [admin] },
            new() { Name = "Carlos López", Email = "carlos@example.com", PasswordHash = placeholderHash, Status = UserStatus.Active, Roles = [admin] },
            new() { Name = "María Torres", Email = "maria@example.com", PasswordHash = placeholderHash, Status = UserStatus.Active, Roles = [editor] },
            new() { Name = "Juan Ramírez", Email = "juan@example.com", PasswordHash = placeholderHash, Status = UserStatus.Inactive, Roles = [editor] },
            new() { Name = "Laura Fernández", Email = "laura@example.com", PasswordHash = placeholderHash, Status = UserStatus.Active, Roles = [viewer] },
        };

        db.Users.AddRange(users);
        await db.SaveChangesAsync();
    }
}
