using System.Linq;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests.CrearUsuario;

/// <summary>
/// Factory de pruebas para US-001 (Crear usuario). Reemplaza el AppDbContext
/// cableado a Postgres por un proveedor EF InMemory, aislado por instancia,
/// para que los tests corran sin una base real (decisión de harness: EF InMemory).
///
/// Nota: EF InMemory no valida el índice único case-insensitive ni la concurrencia
/// como Postgres; esas reglas se verifican a nivel de lógica de aplicación.
/// </summary>
public class CrearUsuarioFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "crear-usuario-" + System.Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });
    }
}
