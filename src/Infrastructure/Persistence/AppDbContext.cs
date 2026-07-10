using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>
/// Contexto de EF Core, ya cableado a Postgres y registrado en la API.
/// Está vacío a propósito: agrega aquí tus DbSet a medida que descubras y
/// modeles tu dominio, y genera tus migraciones desde ahí.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Ejemplo (bórralo y pon los tuyos):
    // public DbSet<MiEntidad> MiEntidades => Set<MiEntidad>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Configura aquí tus relaciones y constraints cuando tengas entidades.
    }
}
