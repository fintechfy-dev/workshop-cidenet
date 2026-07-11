using System.Linq;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests.Support;

/// <summary>
/// Factory de pruebas genérica: reemplaza el AppDbContext por EF InMemory,
/// con un nombre de base único por instancia. Instánciala por test para
/// aislar el estado (conteos de paginación, totales) entre escenarios.
/// </summary>
public sealed class InMemoryApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "api-tests-" + System.Guid.NewGuid();

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
