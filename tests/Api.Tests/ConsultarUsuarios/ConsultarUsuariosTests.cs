using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Api.Tests.Support;
using Xunit;

namespace Api.Tests.ConsultarUsuarios;

/// <summary>
/// Tests de aceptación de US-002 · Consultar usuarios (tabla), derivados de
/// features/US-002.feature. Generados con /test ANTES de la implementación:
/// deben fallar en rojo (aún no existe GET /api/users).
///
/// Cada test crea su propia factory (BD InMemory aislada) para que los conteos
/// de paginación y totales no se contaminen entre escenarios.
///
/// Deferidos (dependen de otras iteraciones):
///  - "Editor solo lectura / Viewer sin acceso" (US-002-SEC): requiere auth (It 9–10).
/// </summary>
public class ConsultarUsuariosTests : IDisposable
{
    private readonly InMemoryApiFactory _factory = new();
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    public ConsultarUsuariosTests() => _client = _factory.CreateClient();

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private sealed record UserRow(Guid Id, string? Nombre, string? Email, string? Rol, string? Estado);

    private sealed record PagedUsers(List<UserRow> Items, int Total, int Page, int PageSize);

    private async Task SeedUserAsync(
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
    }

    private async Task<PagedUsers> GetUsersAsync(string query)
    {
        var response = await _client.GetAsync("/api/users" + query);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedUsers>(JsonOptions);
        Assert.NotNull(page);
        return page!;
    }

    // Scenario: Ver la tabla paginada con sus columnas
    [Fact]
    public async Task Ver_tabla_muestra_columnas_nombre_email_rol_estado()
    {
        await SeedUserAsync("Ana Gomez", "ana@cidenet.com", "Editor", "Activo");

        var page = await GetUsersAsync("");

        var row = Assert.Single(page.Items);
        Assert.False(string.IsNullOrWhiteSpace(row.Nombre));
        Assert.False(string.IsNullOrWhiteSpace(row.Email));
        Assert.False(string.IsNullOrWhiteSpace(row.Rol));
        Assert.False(string.IsNullOrWhiteSpace(row.Estado));
    }

    // Scenario: paginación de 10 por página + total
    [Fact]
    public async Task Paginacion_por_defecto_10_por_pagina_con_total()
    {
        for (var i = 0; i < 12; i++)
        {
            await SeedUserAsync($"Usuario {i}", $"user{i}@cidenet.com");
        }

        var page1 = await GetUsersAsync("?page=1");
        Assert.Equal(12, page1.Total);
        Assert.Equal(10, page1.Items.Count);

        var page2 = await GetUsersAsync("?page=2");
        Assert.Equal(12, page2.Total);
        Assert.Equal(2, page2.Items.Count);
    }

    // Scenario Outline: Buscar por nombre o email de forma parcial y case-insensitive
    [Theory]
    [InlineData("ana", 1)]        // coincide por nombre "Ana Gomez"
    [InlineData("ANA", 1)]        // case-insensitive
    [InlineData("@cidenet", 2)]   // coincide por email en dos cuentas
    public async Task Buscar_parcial_case_insensitive(string termino, int esperados)
    {
        await SeedUserAsync("Ana Gomez", "ana@cidenet.com");
        await SeedUserAsync("Bruno Diaz", "bruno@otrodominio.com");
        await SeedUserAsync("Carla Ruiz", "carla@cidenet.com");

        var page = await GetUsersAsync($"?search={Uri.EscapeDataString(termino)}");

        Assert.Equal(esperados, page.Total);
    }

    // Scenario: Combinar filtros de rol y estado
    [Fact]
    public async Task Combinar_filtros_rol_y_estado()
    {
        await SeedUserAsync("Editor Activo 1", "ea1@cidenet.com", "Editor", "Activo");
        await SeedUserAsync("Editor Activo 2", "ea2@cidenet.com", "Editor", "Activo");
        await SeedUserAsync("Editor Inactivo", "ei@cidenet.com", "Editor", "Inactivo");
        await SeedUserAsync("Admin Activo", "aa@cidenet.com", "Admin", "Activo");

        var page = await GetUsersAsync("?rol=Editor&estado=Activo");

        Assert.Equal(2, page.Total);
    }

    // Scenario: Búsqueda o filtros sin resultados
    [Fact]
    public async Task Busqueda_sin_resultados_devuelve_vacio()
    {
        await SeedUserAsync("Ana Gomez", "ana@cidenet.com");

        var page = await GetUsersAsync("?search=zzz-inexistente");

        Assert.Equal(0, page.Total);
        Assert.Empty(page.Items);
    }

    // Scenario: Recortar espacios del término de búsqueda
    [Fact]
    public async Task Recorta_espacios_del_termino_de_busqueda()
    {
        await SeedUserAsync("Ana Gomez", "ana@cidenet.com");
        await SeedUserAsync("Bruno Diaz", "bruno@otrodominio.com");

        var page = await GetUsersAsync($"?search={Uri.EscapeDataString("  ana  ")}");

        Assert.Equal(1, page.Total);
    }

    // Scenario: Solicitar una página fuera de rango
    [Fact]
    public async Task Pagina_fuera_de_rango_no_falla()
    {
        await SeedUserAsync("Ana Gomez", "ana@cidenet.com");

        var page = await GetUsersAsync("?page=99");

        Assert.NotNull(page.Items); // 200 OK, sin fallar; sin resultados en esa página
        Assert.Empty(page.Items);
    }

    // Scenario (US-002-AUD): La tabla no muestra cuentas eliminadas lógicamente
    [Fact]
    public async Task La_tabla_no_muestra_cuentas_eliminadas_logicamente()
    {
        await SeedUserAsync("Admin Uno", "admin1@cidenet.com", "Admin", "Activo");
        var response = await _client.PostAsJsonAsync("/api/users", new
        {
            nombre = "Ana Gomez",
            email = "ana@cidenet.com",
            password = "Abcdef1x",
            confirmPassword = "Abcdef1x",
            rol = "Editor",
            estado = "Activo"
        });
        response.EnsureSuccessStatusCode();
        var creada = await response.Content.ReadFromJsonAsync<UserRow>(JsonOptions);
        Assert.NotNull(creada);

        await _client.DeleteAsync($"/api/users/{creada!.Id}");

        var page = await GetUsersAsync("");

        Assert.DoesNotContain(page.Items, u => u.Id == creada.Id);
    }

    // Scenario (US-002-SEC): La respuesta de la tabla nunca incluye la contraseña
    [Fact]
    public async Task La_respuesta_no_incluye_la_contrasena()
    {
        await SeedUserAsync("Ana Gomez", "ana@cidenet.com");

        var response = await _client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("Abcdef1x", body);
        Assert.DoesNotContain("password", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("passwordHash", body, StringComparison.OrdinalIgnoreCase);
    }
}
