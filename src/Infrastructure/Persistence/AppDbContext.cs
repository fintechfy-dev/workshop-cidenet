using Domain.Audit;
using Domain.Monitoring;
using Domain.Permissions;
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
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<FailedLoginAttempt> FailedLoginAttempts => Set<FailedLoginAttempt>();

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
            entity.Property(u => u.IsDeleted).IsRequired();

            // Borrado lógico (US-004-AUD): las operaciones normales no ven cuentas
            // eliminadas; el registro físico se conserva y solo se accede a él
            // explícitamente vía IgnoreQueryFilters (auditoría, reactivación de email).
            entity.HasQueryFilter(u => !u.IsDeleted);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Role).HasConversion<string>().IsRequired();
            entity.Property(p => p.Resource).HasConversion<string>().IsRequired();
            entity.Property(p => p.Action).HasConversion<string>().IsRequired();
            entity.HasIndex(p => new { p.Role, p.Resource, p.Action }).IsUnique();
        });

        modelBuilder.Entity<AuditLogEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired();
            entity.Property(e => e.EntityType).IsRequired();
            // Sin FK hacia User: así una entrada sobrevive aunque la cuenta se
            // elimine lógicamente (o si el actor mismo fue eliminado) — US-001-AUD.
        });

        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Type).HasConversion<string>().IsRequired();
            entity.Property(a => a.Message).IsRequired();
        });

        modelBuilder.Entity<FailedLoginAttempt>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.NormalizedEmail).IsRequired();
        });
    }
}
