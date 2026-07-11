using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Api.Tests.Support;
using Xunit;

namespace Api.Tests.EditarPerfil;

/// <summary>
/// Tests de aceptación de US-005 · Editar el propio perfil, derivados de
/// features/US-005.feature.
///
/// Contrato: la identidad del actor viaja en el header "X-User-Id" (respaldado
/// por login real desde It9). El cuerpo de PUT /api/users/me no incluye rol ni
/// estado — son de solo lectura (R2/R3). Desde It10, crear cuentas de prueba
/// requiere un Admin autenticado (<see cref="AuthTestHelpers"/>); las llamadas a
/// /me no exigen rol específico (cualquier actor activo), así que se envían con
/// un cliente plano y el header explícito por request.
///
/// Deferido: "No puedo editar el perfil de otro usuario" vía PUT /api/users/{id}
/// directo ya está cubierto en EditarUsuarioTests (US-003-SEC).
/// </summary>
public class EditarPerfilTests : IAsyncLifetime
{
    private readonly InMemoryApiFactory _factory = new();
    private HttpClient _client = null!;
    private HttpClient _adminClient = null!;

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    public async Task InitializeAsync()
    {
        _adminClient = _factory.CreateClient();
        var adminId = await _adminClient.BootstrapAdminAsync();
        _adminClient.AuthenticateAs(adminId);

        _client = _factory.CreateClient();
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _adminClient.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }

    private sealed record UserDto(Guid Id, string? Nombre, string? Email, string? Rol, string? Estado);

    private async Task<UserDto> CrearAsync(
        string nombre, string email, string password = "Abcdef1x", string rol = "Editor", string estado = "Activo")
    {
        var response = await _adminClient.PostAsJsonAsync("/api/users", new
        {
            nombre,
            email,
            password,
            confirmPassword = password,
            rol,
            estado
        });
        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        Assert.NotNull(dto);
        return dto!;
    }

    private Task<HttpResponseMessage> GetMeAsync(Guid actorId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/me");
        request.Headers.Add("X-User-Id", actorId.ToString());
        return _client.SendAsync(request);
    }

    private Task<HttpResponseMessage> PutMeAsync(Guid actorId, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, "/api/users/me")
        {
            Content = JsonContent.Create(body)
        };
        request.Headers.Add("X-User-Id", actorId.ToString());
        return _client.SendAsync(request);
    }

    // Scenario: Actualizar mis datos personales
    [Fact]
    public async Task Actualizar_mis_datos_personales_actualiza_solo_mi_cuenta()
    {
        var yo = await CrearAsync("Ana Gomez", "ana@cidenet.com");

        var response = await PutMeAsync(yo.Id, new { nombre = "Ana Maria Gomez", email = "ana@cidenet.com" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var actualizado = await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        Assert.Equal("Ana Maria Gomez", actualizado!.Nombre);
    }

    // Scenario: No puedo cambiar mi propio rol ni mis permisos (R2, R3)
    [Fact]
    public async Task El_contrato_no_permite_cambiar_rol_ni_estado_desde_el_perfil_propio()
    {
        var yo = await CrearAsync("Ana Gomez", "ana@cidenet.com", rol: "Editor", estado: "Activo");

        // El body incluye "rol"/"estado" para simular un intento directo; el DTO del
        // endpoint no los declara, así que nunca llegan a aplicarse.
        var response = await PutMeAsync(yo.Id, new
        {
            nombre = "Ana Gomez",
            email = "ana@cidenet.com",
            rol = "Admin",
            estado = "Inactivo"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var actualizado = await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        Assert.Equal("Editor", actualizado!.Rol);
        Assert.Equal("Activo", actualizado.Estado);
    }

    // Scenario: Cambiar mi contraseña exige confirmarla, cumplir la política y la actual
    [Fact]
    public async Task Cambiar_password_exige_actual_correcta_politica_y_confirmacion()
    {
        var yo = await CrearAsync("Ana Gomez", "ana@cidenet.com", password: "Abcdef1x");

        var incorrecta = await PutMeAsync(yo.Id, new
        {
            nombre = "Ana Gomez",
            email = "ana@cidenet.com",
            currentPassword = "Otra1234",
            newPassword = "Nuevapass1",
            confirmNewPassword = "Nuevapass1"
        });
        Assert.Equal(HttpStatusCode.BadRequest, incorrecta.StatusCode);

        var sinCoincidir = await PutMeAsync(yo.Id, new
        {
            nombre = "Ana Gomez",
            email = "ana@cidenet.com",
            currentPassword = "Abcdef1x",
            newPassword = "Nuevapass1",
            confirmNewPassword = "Distinta9"
        });
        Assert.Equal(HttpStatusCode.BadRequest, sinCoincidir.StatusCode);

        var debil = await PutMeAsync(yo.Id, new
        {
            nombre = "Ana Gomez",
            email = "ana@cidenet.com",
            currentPassword = "Abcdef1x",
            newPassword = "debil",
            confirmNewPassword = "debil"
        });
        Assert.Equal(HttpStatusCode.BadRequest, debil.StatusCode);

        var exitosa = await PutMeAsync(yo.Id, new
        {
            nombre = "Ana Gomez",
            email = "ana@cidenet.com",
            currentPassword = "Abcdef1x",
            newPassword = "Nuevapass1",
            confirmNewPassword = "Nuevapass1"
        });
        Assert.Equal(HttpStatusCode.OK, exitosa.StatusCode);
    }

    // Scenario: No puedo cambiar mi email a uno ya usado por otra cuenta
    [Fact]
    public async Task No_puedo_cambiar_mi_email_a_uno_ya_usado_por_otra_cuenta()
    {
        await CrearAsync("Bruno Diaz", "bruno@cidenet.com");
        var yo = await CrearAsync("Ana Gomez", "ana@cidenet.com");

        var response = await PutMeAsync(yo.Id, new { nombre = "Ana Gomez", email = "bruno@cidenet.com" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // Scenario (US-005-SEC): La respuesta del perfil nunca incluye la contraseña
    [Fact]
    public async Task La_respuesta_del_perfil_no_incluye_la_contrasena()
    {
        var yo = await CrearAsync("Ana Gomez", "ana@cidenet.com", password: "Abcdef1x");

        var response = await GetMeAsync(yo.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Abcdef1x", body);
        Assert.DoesNotContain("password", body, StringComparison.OrdinalIgnoreCase);
    }

    // Sin identidad (sin X-User-Id) no se puede acceder al propio perfil
    [Fact]
    public async Task Sin_identidad_no_se_puede_acceder_al_perfil()
    {
        var response = await _client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
