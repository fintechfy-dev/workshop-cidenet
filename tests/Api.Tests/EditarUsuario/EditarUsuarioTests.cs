using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Api.Tests.Support;
using Xunit;

namespace Api.Tests.EditarUsuario;

/// <summary>
/// Tests de aceptación de US-003 · Editar cuenta de usuario, derivados de
/// features/US-003.feature. Generados con /test ANTES de implementar PUT /api/users/{id}.
///
/// Cada test usa su propia BD InMemory (la regla del "último Admin" cuenta admins
/// en toda la base, así que el aislamiento es necesario).
///
/// Deferidos (dependen de otras iteraciones):
///  - R2 "no puede modificar su propio rol" y US-003-SEC "solo Admin edita a otros":
///    requieren la identidad del actor autenticado (It 9–10).
///  - "botón Guardar deshabilitado": frontend (It 14).
/// </summary>
public class EditarUsuarioTests : IDisposable
{
    private readonly InMemoryApiFactory _factory = new();
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    public EditarUsuarioTests() => _client = _factory.CreateClient();

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

    private Task<HttpResponseMessage> EditarAsync(
        Guid id, string nombre, string email, string rol, string estado) =>
        _client.PutAsJsonAsync($"/api/users/{id}", new { nombre, email, rol, estado });

    // Scenario: Editar los datos de un usuario con éxito
    [Fact]
    public async Task Editar_actualiza_los_datos_del_usuario()
    {
        var user = await CrearAsync("Ana Gomez", "ana@cidenet.com", "Editor", "Activo");

        var response = await EditarAsync(user.Id, "Ana Maria Gomez", "ana@cidenet.com", "Editor", "Activo");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        Assert.NotNull(updated);
        Assert.Equal("Ana Maria Gomez", updated!.Nombre);
    }

    // Scenario: Rechazar cambio de email a uno ya usado por otra cuenta (R4)
    [Fact]
    public async Task Rechazar_cambio_de_email_a_uno_ya_usado_por_otra_cuenta()
    {
        await CrearAsync("Ana Gomez", "ana@cidenet.com");
        var bruno = await CrearAsync("Bruno Diaz", "bruno@cidenet.com");

        var response = await EditarAsync(bruno.Id, "Bruno Diaz", "ana@cidenet.com", "Editor", "Activo");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // Scenario: No se puede quitar el rol Admin al último Admin (derivada de R1)
    [Fact]
    public async Task No_se_puede_quitar_el_rol_admin_al_ultimo_admin()
    {
        var admin = await CrearAsync("Admin Unico", "admin@cidenet.com", "Admin", "Activo");

        var response = await EditarAsync(admin.Id, "Admin Unico", "admin@cidenet.com", "Editor", "Activo");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // Scenario: Cambiar el email a una variante que solo difiere en mayúsculas es el mismo email
    [Fact]
    public async Task Cambiar_email_a_variante_en_mayusculas_del_propio_no_genera_conflicto()
    {
        var user = await CrearAsync("Ana Gomez", "ana@cidenet.com");

        var response = await EditarAsync(user.Id, "Ana Gomez", "ANA@cidenet.com", "Editor", "Activo");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Editar un usuario inexistente devuelve 404
    [Fact]
    public async Task Editar_usuario_inexistente_devuelve_404()
    {
        var response = await EditarAsync(Guid.NewGuid(), "Alguien", "alguien@cidenet.com", "Editor", "Activo");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
