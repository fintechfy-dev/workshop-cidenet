using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>
/// Contexto de EF Core. Mapea el agregado User a partir de las invariantes del dominio.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Name).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(Email.MaxLength);
            entity.HasIndex(u => u.Email).IsUnique(); // unicidad a nivel de BD (R4)
            entity.Property(u => u.Role).HasConversion<string>().IsRequired();
            entity.Property(u => u.Status).HasConversion<string>().IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
        });
    }
}
