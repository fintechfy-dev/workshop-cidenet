using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Api.Tests.Support;
using Xunit;

namespace Api.Tests.MatrizPermisos;

/// <summary>
/// Tests de aceptación de US-006 · Matriz de permisos, derivados de
/// features/US-006.feature. Generados con /test ANTES de implementar
/// GET/PUT /api/permissions (deben fallar en rojo).
///
/// R3 ("un Admin no puede asignarse permisos a sí mismo") y el anti-lockout
/// (US-006-SEC-ERR1, "no dejar a Admin sin sus permisos esenciales") se
/// resuelven con UNA sola regla: los permisos del rol Admin son inmutables
/// desde este endpoint (solo un Admin puede llegar aquí, así que "el rol que
/// él mismo posee" es siempre Admin). Cualquier intento de tocar esa fila,
/// en cualquier dirección, se rechaza — cubre ambos escenarios a la vez.
///
/// Deferidos (dependen de otras iteraciones):
///  - "Un cambio se refleja en la autorización efectiva" (que otros endpoints
///    respeten la matriz): transversal, se cierra en It 10.
///  - "Solo Admin accede a la matriz" (US-006-SEC): requiere autenticación (It 9–10).
/// </summary>
public class MatrizPermisosTests : IDisposable
{
    private readonly InMemoryApiFactory _factory = new();
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    public MatrizPermisosTests() => _client = _factory.CreateClient();

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private sealed record PermissionCell(string Rol, string Recurso, string Accion, bool Permitido);

    private async Task<List<PermissionCell>> GetMatrixAsync()
    {
        var response = await _client.GetAsync("/api/permissions");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var matrix = await response.Content.ReadFromJsonAsync<List<PermissionCell>>(JsonOptions);
        Assert.NotNull(matrix);
        return matrix!;
    }

    private Task<HttpResponseMessage> PutMatrixAsync(params object[] cambios) =>
        _client.PutAsJsonAsync("/api/permissions", new { cambios });

    // Scenario: Ver la matriz de permisos con su estado actual
    [Fact]
    public async Task Ver_matriz_muestra_3_roles_x_4_recursos_x_4_acciones_con_defaults_del_caso()
    {
        var matrix = await GetMatrixAsync();

        Assert.Equal(48, matrix.Count); // 3 roles x 4 recursos x 4 acciones

        // Admin: acceso total.
        Assert.All(matrix.Where(c => c.Rol == "Admin"), c => Assert.True(c.Permitido));

        // Editor: solo lectura sobre users/roles/permissions; CRUD sobre reports.
        Assert.True(matrix.Single(c => c.Rol == "Editor" && c.Recurso == "Users" && c.Accion == "Read").Permitido);
        Assert.False(matrix.Single(c => c.Rol == "Editor" && c.Recurso == "Users" && c.Accion == "Create").Permitido);
        Assert.True(matrix.Single(c => c.Rol == "Editor" && c.Recurso == "Reports" && c.Accion == "Delete").Permitido);

        // Viewer: nada salvo lectura de reports.
        Assert.False(matrix.Single(c => c.Rol == "Viewer" && c.Recurso == "Users" && c.Accion == "Read").Permitido);
        Assert.True(matrix.Single(c => c.Rol == "Viewer" && c.Recurso == "Reports" && c.Accion == "Read").Permitido);
    }

    // Scenario: Activar o desactivar un permiso individual
    [Fact]
    public async Task Desactivar_un_permiso_individual_se_persiste()
    {
        var response = await PutMatrixAsync(new { rol = "Editor", recurso = "Reports", accion = "Create", permitido = false });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var matrix = await GetMatrixAsync();
        Assert.False(matrix.Single(c => c.Rol == "Editor" && c.Recurso == "Reports" && c.Accion == "Create").Permitido);
    }

    // Scenario: Los roles son predefinidos (rechaza un rol inválido)
    [Fact]
    public async Task Rechaza_un_rol_que_no_es_uno_de_los_predefinidos()
    {
        var response = await PutMatrixAsync(new { rol = "SuperAdmin", recurso = "Reports", accion = "Read", permitido = true });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // Scenario: Un Admin no puede asignarse permisos a sí mismo (R3)
    [Fact]
    public async Task No_se_pueden_modificar_los_permisos_del_rol_admin()
    {
        var response = await PutMatrixAsync(new { rol = "Admin", recurso = "Users", accion = "Delete", permitido = false });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var matrix = await GetMatrixAsync();
        Assert.True(matrix.Single(c => c.Rol == "Admin" && c.Recurso == "Users" && c.Accion == "Delete").Permitido);
    }

    // Scenario (US-006-SEC): No se puede dejar a Admin sin permisos esenciales (anti-lockout)
    // Cubierto por la misma regla que R3: la fila de Admin es inmutable.
    [Fact]
    public async Task No_se_puede_desactivar_un_permiso_esencial_de_gestion_de_admin()
    {
        var response = await PutMatrixAsync(new { rol = "Admin", recurso = "Permissions", accion = "Update", permitido = false });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
