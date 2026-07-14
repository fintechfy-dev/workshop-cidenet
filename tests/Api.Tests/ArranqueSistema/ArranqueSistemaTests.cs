using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Api.Tests.Support;
using Application.Users;
using Xunit;

namespace Api.Tests.ArranqueSistema;

/// <summary>
/// Tests de aceptación de US-001-MNT (MNT-1), derivados de
/// features/US-001-MNT.feature — arranque con Admin sembrado.
/// </summary>
public class ArranqueSistemaTests : IAsyncLifetime
{
    private readonly InMemoryApiFactory _factory = new();
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }

    // Scenario: El sistema arranca con un Admin inicial sembrado
    [Fact]
    public async Task Una_base_recien_desplegada_tiene_un_admin_sembrado_capaz_de_crear_los_demas_usuarios()
    {
        var seed = new SeedOptions();

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = seed.AdminEmail,
            password = seed.AdminPassword
        });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var admin = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Admin", admin.GetProperty("rol").GetString());
        _client.AuthenticateAs(admin.GetProperty("id").GetGuid());

        var createResponse = await _client.PostAsJsonAsync("/api/users", new
        {
            nombre = "Eddie Editor",
            email = "eddie-arranque@cidenet.com",
            password = "Abcdef1x",
            confirmPassword = "Abcdef1x",
            rol = "Editor",
            estado = "Activo"
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
    }

    // Hallazgo de /audit: sin seed, el primer POST /api/users sin autenticar
    // ganaba y se volvía Admin. Con el Admin sembrado al arranque, esa vía
    // queda cerrada: toda creación exige autorización real.
    [Fact]
    public async Task Ya_no_se_puede_crear_el_primer_usuario_sin_autenticar()
    {
        var response = await _client.PostAsJsonAsync("/api/users", new
        {
            nombre = "Atacante",
            email = "attacker@evil.com",
            password = "Abcdef1x",
            confirmPassword = "Abcdef1x",
            rol = "Admin",
            estado = "Activo"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
