using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Api.Tests.Support;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Api.Tests.EliminarUsuario;

/// <summary>
/// Tests de aceptación de US-004 · Eliminar cuenta de usuario, derivados de
/// features/US-004.feature.
///
/// Contrato del endpoint: 204 éxito, 404 no existe/ya eliminado, 409 último Admin
/// o autoeliminación, 403 si el actor no es Admin.
///
/// Desde It10, el Admin de arranque (ver <see cref="AuthTestHelpers"/>) es el
/// actor por defecto. "No autoeliminación" y "solo Admin elimina" ya no están
/// deferidos. Nota: bloquear la eliminación del último Admin y bloquear la
/// autoeliminación colapsan en el mismo caso cuando solo existe un Admin (es
/// el único que podría autorizar su propia eliminación), así que ambas reglas
/// se verifican con 409 sin distinguir cuál disparó primero.
///
/// Deferido: la advertencia del modal para Admin es frontend (It 16).
/// </summary>
public class EliminarUsuarioTests : IAsyncLifetime
{
    private readonly InMemoryApiFactory _factory = new();
    private HttpClient _client = null!;
    private Guid _bootstrapAdminId;

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        _bootstrapAdminId = await _client.BootstrapAdminAsync();
        _client.AuthenticateAs(_bootstrapAdminId);
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }

    private sealed record UserDto(Guid Id, string? Nombre, string? Email, string? Rol, string? Estado);

    private async Task<UserDto> CrearAsync(
        string nombre, string email, string rol = "Editor", string estado = "Activo")
    {
        var response = await _client.PostAsJsonAsync("/api/users", new
        {
            nombre,
            email,
            password = "Abcdef1x",
            confirmPassword = "Abcdef1x",
            rol,
            estado
        });
        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        Assert.NotNull(dto);
        return dto!;
    }

    private HttpClient ClienteComo(Guid actorId)
    {
        var client = _factory.CreateClient();
        client.AuthenticateAs(actorId);
        return client;
    }

    private Task<HttpResponseMessage> EliminarAsync(Guid id) =>
        _client.DeleteAsync($"/api/users/{id}");

    // Scenario: Eliminar una cuenta con confirmación explícita
    // (la parte backend: la cuenta se elimina y desaparece de la tabla)
    [Fact]
    public async Task Eliminar_una_cuenta_la_marca_eliminada_y_desaparece_de_la_tabla()
    {
        var objetivo = await CrearAsync("Ana Gomez", "ana@cidenet.com", "Editor", "Activo");

        var response = await EliminarAsync(objetivo.Id);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var lista = await _client.GetFromJsonAsync<JsonElement>("/api/users");
        var items = lista.GetProperty("items");
        Assert.DoesNotContain(items.EnumerateArray(),
            u => u.GetProperty("id").GetGuid() == objetivo.Id);
    }

    // Scenario: No se puede eliminar el último Admin (R1) — con un único Admin,
    // solo él podría autorizar su propia eliminación, así que coincide con autoeliminación.
    [Fact]
    public async Task No_se_puede_eliminar_el_ultimo_admin()
    {
        var response = await EliminarAsync(_bootstrapAdminId);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // Scenario: Un Admin no puede eliminar su propia cuenta, aunque no sea el último
    [Fact]
    public async Task Un_admin_no_puede_eliminar_su_propia_cuenta()
    {
        await CrearAsync("Segundo Admin", "segundo@cidenet.com", "Admin"); // ahora hay 2 admins

        var response = await EliminarAsync(_bootstrapAdminId);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // Scenario: Eliminar una cuenta que ya fue eliminada en paralelo
    [Fact]
    public async Task Eliminar_una_cuenta_ya_eliminada_informa_que_no_existe_sin_fallar()
    {
        var objetivo = await CrearAsync("Ana Gomez", "ana@cidenet.com", "Editor", "Activo");
        await EliminarAsync(objetivo.Id); // otro Admin ya la eliminó

        var response = await EliminarAsync(objetivo.Id);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Eliminar un usuario inexistente devuelve 404
    [Fact]
    public async Task Eliminar_usuario_inexistente_devuelve_404()
    {
        var response = await EliminarAsync(Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Scenario US-004-AUD: La eliminación es lógica y conserva el registro para auditoría
    [Fact]
    public async Task Eliminacion_es_logica_y_conserva_el_registro_fisico()
    {
        var objetivo = await CrearAsync("Ana Gomez", "ana@cidenet.com", "Editor", "Activo");

        await EliminarAsync(objetivo.Id);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var registro = await db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == objetivo.Id);

        Assert.NotNull(registro); // el registro sigue físicamente en la base
    }

    // Scenario (US-004-SEC): Solo Admin puede eliminar cuentas
    [Fact]
    public async Task Editor_no_puede_eliminar_cuentas()
    {
        var editor = await CrearAsync("Eddie Editor", "eddie@cidenet.com", "Editor");
        var objetivo = await CrearAsync("Objetivo", "objetivo@cidenet.com");
        var editorClient = ClienteComo(editor.Id);

        var response = await editorClient.DeleteAsync($"/api/users/{objetivo.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
