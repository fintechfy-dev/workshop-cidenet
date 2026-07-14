using System;
using System.Linq;
using System.Threading.Tasks;
using Api.Tests.Support;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Api.Tests.CrearUsuario;

/// <summary>
/// Factory de pruebas para US-001 (Crear usuario). Reemplaza el AppDbContext
/// cableado a Postgres por un proveedor EF InMemory, aislado por instancia,
/// para que los tests corran sin una base real (decisión de harness: EF InMemory).
///
/// El host siembra su propio Admin al arrancar (MNT-1); esta factory solo
/// inicia sesión con él una vez para toda la clase (la BD es compartida vía
/// IClassFixture) y reusa su id en los tests (ver <see cref="AdminId"/>).
///
/// Nota: EF InMemory no valida el índice único case-insensitive ni la concurrencia
/// como Postgres; esas reglas se verifican a nivel de lógica de aplicación.
/// </summary>
public class CrearUsuarioFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _dbName = "crear-usuario-" + System.Guid.NewGuid();

    public Guid AdminId { get; private set; }

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

    async Task IAsyncLifetime.InitializeAsync()
    {
        using var client = CreateClient();
        AdminId = await client.BootstrapAdminAsync();
    }

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;
}
