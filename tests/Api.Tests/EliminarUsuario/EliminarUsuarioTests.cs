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
/// features/US-004.feature. Generados con /test ANTES de implementar
/// DELETE /api/users/{id} (deben fallar en rojo).
///
/// Contrato asumido para el endpoint (a implementar en /iterate):
///  - 204 No Content al eliminar con éxito (soft-delete).
///  - 404 Not Found si el usuario no existe o ya fue eliminado (idempotencia,
///    "informa que ya no existe, sin fallar").
///  - 409 Conflict si es el último Admin (R1), con { error }.
///
/// Deferidos (dependen de otras iteraciones):
///  - US-004 escenario "Un Admin no puede eliminar su propia cuenta": requiere
///    la identidad del actor autenticado (It 9–10), igual que R2 en Editar (It5).
///  - US-004-SEC "Solo Admin puede eliminar cuentas": requiere autenticación (It 9–10).
///  - "El modal advierte cuando el usuario a eliminar es Admin": frontend (It 16).
/// </summary>
public class EliminarUsuarioTests : IDisposable
{
    private readonly InMemoryApiFactory _factory = new();
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    public EliminarUsuarioTests() => _client = _factory.CreateClient();

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
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

    private Task<HttpResponseMessage> EliminarAsync(Guid id) =>
        _client.DeleteAsync($"/api/users/{id}");

    // Scenario: Eliminar una cuenta con confirmación explícita
    // (la parte backend: la cuenta se elimina y desaparece de la tabla)
    [Fact]
    public async Task Eliminar_una_cuenta_la_marca_eliminada_y_desaparece_de_la_tabla()
    {
        await CrearAsync("Admin Uno", "admin1@cidenet.com", "Admin", "Activo");
        var objetivo = await CrearAsync("Ana Gomez", "ana@cidenet.com", "Editor", "Activo");

        var response = await EliminarAsync(objetivo.Id);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var lista = await _client.GetFromJsonAsync<JsonElement>("/api/users");
        var items = lista.GetProperty("items");
        Assert.DoesNotContain(items.EnumerateArray(),
            u => u.GetProperty("id").GetGuid() == objetivo.Id);
    }

    // Scenario: No se puede eliminar el último Admin (R1)
    [Fact]
    public async Task No_se_puede_eliminar_el_ultimo_admin()
    {
        var admin = await CrearAsync("Admin Unico", "admin@cidenet.com", "Admin", "Activo");

        var response = await EliminarAsync(admin.Id);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("último administrador", body.GetProperty("error").GetString());
    }

    // Scenario: Eliminar una cuenta que ya fue eliminada en paralelo
    [Fact]
    public async Task Eliminar_una_cuenta_ya_eliminada_informa_que_no_existe_sin_fallar()
    {
        await CrearAsync("Admin Uno", "admin1@cidenet.com", "Admin", "Activo");
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
        await CrearAsync("Admin Uno", "admin1@cidenet.com", "Admin", "Activo");
        var objetivo = await CrearAsync("Ana Gomez", "ana@cidenet.com", "Editor", "Activo");

        await EliminarAsync(objetivo.Id);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var registro = await db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == objetivo.Id);

        Assert.NotNull(registro); // el registro sigue físicamente en la base
    }
}
