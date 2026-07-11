using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Api.Tests.Support;
using Xunit;

namespace Api.Tests.Autenticacion;

/// <summary>
/// Tests de aceptación de US-007 · Autenticación y bloqueo de inactivos,
/// derivados de features/US-007.feature.
///
/// El login devuelve el mismo UserDto que el resto de la API (sin contraseña);
/// su Id es lo que el cliente usa después como header X-User-Id para
/// /api/users/me, etc. Crear cuentas de prueba pasa por un Admin autenticado
/// (<see cref="AuthTestHelpers"/>, It10); las llamadas a /login y /me en sí no
/// exigen rol específico, así que se envían con un cliente plano.
///
/// US-007-USR (desactivar corta el acceso de inmediato) se verifica sobre
/// GET /api/users/me, que revalida Estado=Activo en cada request.
/// </summary>
public class AutenticacionTests : IAsyncLifetime
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

    private Task<HttpResponseMessage> LoginAsync(string? email, string? password) =>
        _client.PostAsJsonAsync("/api/auth/login", new { email, password });

    private Task<HttpResponseMessage> GetMeAsync(Guid actorId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/me");
        request.Headers.Add("X-User-Id", actorId.ToString());
        return _client.SendAsync(request);
    }

    // Scenario: Autenticar a un usuario activo con credenciales correctas
    [Fact]
    public async Task Autentica_a_un_usuario_activo_con_credenciales_correctas()
    {
        await CrearAsync("Ana Gomez", "ana@cidenet.com", "Abcdef1x");

        var response = await LoginAsync("ana@cidenet.com", "Abcdef1x");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Abcdef1x", body);
        Assert.DoesNotContain("password", body, StringComparison.OrdinalIgnoreCase);
    }

    // Scenario: Bloquear a un usuario inactivo aunque sus credenciales sean correctas (R6)
    [Fact]
    public async Task Bloquea_a_un_usuario_inactivo_con_credenciales_correctas()
    {
        await CrearAsync("Ana Gomez", "ana@cidenet.com", "Abcdef1x", estado: "Inactivo");

        var response = await LoginAsync("ana@cidenet.com", "Abcdef1x");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("inactiva", body, StringComparison.OrdinalIgnoreCase);
    }

    // Scenario: Credenciales inválidas devuelven un mensaje genérico (anti-enumeración)
    [Fact]
    public async Task Credenciales_invalidas_devuelven_el_mismo_mensaje_generico()
    {
        await CrearAsync("Ana Gomez", "ana@cidenet.com", "Abcdef1x");

        var emailInexistente = await LoginAsync("no-existe@cidenet.com", "Abcdef1x");
        var passwordIncorrecta = await LoginAsync("ana@cidenet.com", "Incorrecta1");

        Assert.Equal(HttpStatusCode.Unauthorized, emailInexistente.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, passwordIncorrecta.StatusCode);

        var mensaje1 = await emailInexistente.Content.ReadAsStringAsync();
        var mensaje2 = await passwordIncorrecta.Content.ReadAsStringAsync();
        Assert.Equal(mensaje1, mensaje2); // mismo mensaje, no revela cuál era el problema
    }

    // Scenario: Email y contraseña son obligatorios
    [Theory]
    [InlineData(null, "Abcdef1x")]
    [InlineData("ana@cidenet.com", null)]
    [InlineData("", "")]
    public async Task Email_y_password_son_obligatorios(string? email, string? password)
    {
        var response = await LoginAsync(email, password);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // Scenario: El email se trata case-insensitive al autenticar
    [Fact]
    public async Task El_email_es_case_insensitive_al_autenticar()
    {
        await CrearAsync("Ana Gomez", "ana@cidenet.com", "Abcdef1x");

        var response = await LoginAsync("ANA@CIDENET.com", "Abcdef1x");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Scenario (US-007-USR): Desactivar a un usuario corta su sesión activa de inmediato
    [Fact]
    public async Task Desactivar_a_un_usuario_corta_su_acceso_al_perfil_de_inmediato()
    {
        var yo = await CrearAsync("Ana Gomez", "ana@cidenet.com", "Abcdef1x", estado: "Activo");

        Assert.Equal(HttpStatusCode.OK, (await GetMeAsync(yo.Id)).StatusCode);

        await _adminClient.PutAsJsonAsync($"/api/users/{yo.Id}", new
        {
            nombre = yo.Nombre,
            email = yo.Email,
            rol = yo.Rol,
            estado = "Inactivo"
        });

        var response = await GetMeAsync(yo.Id);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
