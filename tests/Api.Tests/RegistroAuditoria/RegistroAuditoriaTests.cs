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

namespace Api.Tests.RegistroAuditoria;

/// <summary>
/// Tests de aceptación de US-001-AUD · Registro de auditoría, derivados de
/// features/US-001-AUD.feature.
///
/// El registro se llena solo: un <c>IEndpointFilter</c> (AuditLogFilter) aplicado
/// a todo el grupo /api anota actor (X-User-Id), acción (nombre del endpoint) y
/// entidad (segmento de la URL) en cada request — no hay que instrumentar cada
/// servicio de aplicación por separado.
/// </summary>
public class RegistroAuditoriaTests : IAsyncLifetime
{
    private readonly InMemoryApiFactory _factory = new();
    private HttpClient _client = null!;
    private Guid _adminId;

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        _adminId = await _client.BootstrapAdminAsync();
        _client.AuthenticateAs(_adminId);
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }

    private sealed record UserDto(Guid Id, string? Nombre, string? Email, string? Rol, string? Estado);

    private sealed record AuditEntry(Guid Id, Guid? ActorId, string Accion, string Entidad, string? EntityId, DateTime Fecha);

    private async Task<UserDto> CrearAsync(string nombre, string email, string rol = "Editor")
    {
        var response = await _client.PostAsJsonAsync("/api/users", new
        {
            nombre,
            email,
            password = "Abcdef1x",
            confirmPassword = "Abcdef1x",
            rol,
            estado = "Activo"
        });
        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        Assert.NotNull(dto);
        return dto!;
    }

    private async Task<List<AuditEntry>> GetAuditLogAsync()
    {
        var response = await _client.GetAsync("/api/audit-log");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var entries = await response.Content.ReadFromJsonAsync<List<AuditEntry>>(JsonOptions);
        Assert.NotNull(entries);
        return entries!;
    }

    // Scenario Outline: Registrar cada acción con actor, acción, entidad y fecha
    [Fact]
    public async Task Registra_actor_accion_entidad_y_fecha_de_cada_tipo_de_accion()
    {
        var objetivo = await CrearAsync("Ana Gomez", "ana@cidenet.com"); // crear usuario
        await _client.GetAsync("/api/users"); // consultar la tabla de usuarios
        await _client.PutAsJsonAsync($"/api/users/{objetivo.Id}", // editar usuario / cambiar rol o estado
            new { nombre = "Ana Gomez", email = "ana@cidenet.com", rol = "Editor", estado = "Activo" });
        await _client.PutAsJsonAsync("/api/permissions", // configurar la matriz de permisos
            new { cambios = new[] { new { rol = "Editor", recurso = "Reports", accion = "Create", permitido = true } } });
        await _client.PostAsJsonAsync("/api/auth/login", // iniciar sesión
            new { email = "ana@cidenet.com", password = "Abcdef1x" });
        await _client.DeleteAsync($"/api/users/{objetivo.Id}"); // eliminar usuario

        var entradas = await GetAuditLogAsync();

        void AsertarRegistrada(string accion, string entidad)
        {
            var entrada = entradas.FirstOrDefault(e => e.Accion == accion && e.Entidad == entidad);
            Assert.True(entrada is not null, $"No se encontró una entrada para {accion}/{entidad}.");
            Assert.True(entrada!.Fecha > DateTime.MinValue);
        }

        // "CreateUser" ya tiene una entrada previa del Admin de arranque (sin actor,
        // creado antes del login); acá se verifica la de Ana, hecha por el Admin.
        AsertarRegistrada("CreateUser", "users");
        AsertarRegistrada("ListUsers", "users");
        AsertarRegistrada("EditUser", "users");
        AsertarRegistrada("EditPermissionMatrix", "permissions");
        AsertarRegistrada("Login", "auth");
        AsertarRegistrada("DeleteUser", "users");

        // El actor queda registrado en las acciones hechas por el Admin autenticado.
        var creacion = entradas.Single(e => e.Accion == "CreateUser" && e.Entidad == "users" && e.ActorId == _adminId);
        Assert.Equal(_adminId, creacion.ActorId);
    }

    // Scenario: El registro de auditoría es inmutable (append-only)
    [Fact]
    public async Task El_registro_de_auditoria_no_se_puede_editar_ni_borrar()
    {
        await CrearAsync("Ana Gomez", "ana@cidenet.com");
        var entradas = await GetAuditLogAsync();
        var idExistente = entradas.First().Id;

        var putResponse = await _client.PutAsJsonAsync($"/api/audit-log/{idExistente}", new { });
        var deleteResponse = await _client.DeleteAsync($"/api/audit-log/{idExistente}");

        // No existe ninguna ruta de edición/borrado: el sistema simplemente no la ofrece.
        Assert.Equal(HttpStatusCode.NotFound, putResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }

    // Scenario: El registro de auditoría sobrevive al soft-delete de la cuenta
    [Fact]
    public async Task El_registro_sobrevive_al_soft_delete_de_la_cuenta_involucrada()
    {
        var objetivo = await CrearAsync("Ana Gomez", "ana@cidenet.com");
        await _client.PutAsJsonAsync($"/api/users/{objetivo.Id}",
            new { nombre = "Ana Gomez", email = "ana@cidenet.com", rol = "Editor", estado = "Activo" });

        await _client.DeleteAsync($"/api/users/{objetivo.Id}"); // soft-delete

        var entradas = await GetAuditLogAsync();
        Assert.Contains(entradas, e => e.Accion == "EditUser" && e.EntityId == objetivo.Id.ToString());
    }
}
