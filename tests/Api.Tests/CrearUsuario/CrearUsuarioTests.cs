using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Api.Tests.CrearUsuario;

/// <summary>
/// Tests de aceptación de US-001 · Crear cuenta de usuario, derivados de
/// features/US-001.feature (un test por Scenario). Generados con /test ANTES
/// de la implementación: deben fallar en rojo contra el código actual
/// (aún no existe POST /api/users).
///
/// Fuera de este archivo (deferidos por dependencia de otras iteraciones):
///  - Scenario "Un rol sin permiso no puede crear usuarios" (US-001-SEC): requiere
///    autenticación/autorización (US-007 / Iteración 10) → va en la suite de SEC.
///  - Scenario UX "botón Guardar deshabilitado": es de frontend (Iteración 14).
/// </summary>
public class CrearUsuarioTests : IClassFixture<CrearUsuarioFactory>
{
    private readonly CrearUsuarioFactory _factory;

    public CrearUsuarioTests(CrearUsuarioFactory factory) => _factory = factory;

    private static string UniqueEmail() => $"u{Guid.NewGuid():N}@cidenet.com";

    /// <summary>Cliente autenticado como el Admin de arranque (ver <see cref="CrearUsuarioFactory"/>).</summary>
    private HttpClient CreateAuthenticatedClient()
    {
        var httpClient = _factory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-User-Id", _factory.AdminId.ToString());
        return httpClient;
    }

    private static object CrearRequest(
        string? nombre = "Ana Gomez",
        string? email = null,
        string password = "Abcdef1x",
        string? confirmPassword = null,
        string rol = "Editor",
        string estado = "Activo")
        => new
        {
            nombre,
            email = email ?? "ana@cidenet.com",
            password,
            confirmPassword = confirmPassword ?? password,
            rol,
            estado
        };

    // Scenario: Crear una cuenta con datos válidos
    [Fact]
    public async Task Crear_cuenta_con_datos_validos_queda_activa_y_sin_exponer_password()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/users", CrearRequest(email: UniqueEmail()));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Activo", body);                                      // estado por defecto
        Assert.DoesNotContain("Abcdef1x", body);                              // la contraseña nunca sale
        Assert.DoesNotContain("password", body, StringComparison.OrdinalIgnoreCase);
    }

    // Scenario: Rechazar creación con un campo obligatorio vacío
    [Fact]
    public async Task Rechazar_cuando_un_campo_obligatorio_esta_vacio()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync(
            "/api/users", CrearRequest(nombre: "", email: UniqueEmail()));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // Scenario: Rechazar email con formato inválido
    [Fact]
    public async Task Rechazar_email_con_formato_invalido()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync(
            "/api/users", CrearRequest(email: "ana(arroba)cidenet"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // Scenario: Rechazar email duplicado (R4)
    [Fact]
    public async Task Rechazar_email_duplicado()
    {
        var client = CreateAuthenticatedClient();
        var email = UniqueEmail();

        await client.PostAsJsonAsync("/api/users", CrearRequest(email: email));
        var response = await client.PostAsJsonAsync("/api/users", CrearRequest(email: email));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // Scenario Outline: Validar la política de contraseña
    [Theory]
    [InlineData("Abcdef1x", true)]   // válida
    [InlineData("abcdef1x", false)]  // falta mayúscula
    [InlineData("ABCDEF1X", false)]  // falta minúscula
    [InlineData("Abcdefgh", false)]  // falta número
    [InlineData("Ab1x", false)]      // menos de 8 caracteres
    public async Task Validar_politica_de_password(string password, bool aceptada)
    {
        var client = CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync(
            "/api/users", CrearRequest(email: UniqueEmail(), password: password));

        if (aceptada)
        {
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    // Scenario: Rechazar cuando la contraseña y su confirmación no coinciden
    [Fact]
    public async Task Rechazar_cuando_password_y_confirmacion_no_coinciden()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync(
            "/api/users",
            CrearRequest(email: UniqueEmail(), password: "Abcdef1x", confirmPassword: "Distinta9Z"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // Scenario: Rechazar cuando no se selecciona un rol válido
    [Fact]
    public async Task Rechazar_cuando_el_rol_no_es_valido()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync(
            "/api/users", CrearRequest(email: UniqueEmail(), rol: "SuperUser"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // Scenario: El email es case-insensitive para la unicidad
    [Fact]
    public async Task Email_es_case_insensitive_para_la_unicidad()
    {
        var client = CreateAuthenticatedClient();
        var localPart = $"u{Guid.NewGuid():N}";

        await client.PostAsJsonAsync("/api/users", CrearRequest(email: $"{localPart}@cidenet.com"));
        var response = await client.PostAsJsonAsync(
            "/api/users", CrearRequest(email: $"{localPart.ToUpperInvariant()}@CIDENET.COM"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // Scenario: Se recortan los espacios del nombre y del email
    [Fact]
    public async Task Se_recortan_los_espacios_del_nombre_y_del_email()
    {
        var client = CreateAuthenticatedClient();
        var localPart = $"u{Guid.NewGuid():N}";

        var response = await client.PostAsJsonAsync(
            "/api/users",
            CrearRequest(nombre: "  Ana Gomez  ", email: $"  {localPart}@cidenet.com  "));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains($"{localPart}@cidenet.com", body);
        Assert.DoesNotContain($"  {localPart}", body);   // sin espacios de guarda
    }

    // Scenario Outline: Validar el límite de longitud del nombre
    [Theory]
    [InlineData(1, false)]     // menos del mínimo (2)
    [InlineData(50, true)]     // dentro del rango
    [InlineData(101, false)]   // más del máximo (100)
    public async Task Validar_limite_de_longitud_del_nombre(int longitud, bool aceptada)
    {
        var client = CreateAuthenticatedClient();
        var nombre = new string('a', longitud);

        var response = await client.PostAsJsonAsync(
            "/api/users", CrearRequest(nombre: nombre, email: UniqueEmail()));

        if (aceptada)
        {
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    // Scenario (US-001-EDGE5): Recrear el email de una cuenta eliminada reactiva la cuenta
    [Fact]
    public async Task Recrear_email_de_cuenta_eliminada_reactiva_la_cuenta_en_vez_de_duplicarla()
    {
        var client = CreateAuthenticatedClient();
        var email = UniqueEmail();

        var creada = await client.PostAsJsonAsync("/api/users", CrearRequest(email: email));
        creada.EnsureSuccessStatusCode();
        var creadaId = (await creada.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        await client.DeleteAsync($"/api/users/{creadaId}");

        var reactivada = await client.PostAsJsonAsync(
            "/api/users", CrearRequest(nombre: "Ana Reactivada", email: email));

        Assert.Equal(HttpStatusCode.Created, reactivada.StatusCode);
        var body = await reactivada.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(creadaId, body.GetProperty("id").GetGuid()); // misma cuenta, no duplicada
        Assert.Equal("Ana Reactivada", body.GetProperty("nombre").GetString());
    }
}
