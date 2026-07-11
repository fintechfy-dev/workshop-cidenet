using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Api.Tests.Support;
using Application.Users;
using Domain.Users;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Api.Tests.Observabilidad;

/// <summary>
/// Tests de aceptación de US-001-MON · Observabilidad y alertas, derivados de
/// features/US-001-MON.feature.
///
/// MON-1 (fuerza bruta) y MON-2 (pocos Admin activos) usan los umbrales por
/// defecto de <c>MonitoringOptions</c> (5 intentos / 15 min; 1 Admin). MON-3
/// (error de backend) se simula sustituyendo IUserRepository por un decorador
/// que falla al guardar, solo para esa llamada.
/// </summary>
public class ObservabilidadTests : IAsyncLifetime
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

    private sealed record AlertaDto(Guid Id, string Tipo, string Mensaje, DateTime Fecha);

    private async Task<UserDto> CrearAsync(string nombre, string email, string password = "Abcdef1x", string rol = "Editor")
    {
        var response = await _client.PostAsJsonAsync("/api/users", new
        {
            nombre,
            email,
            password,
            confirmPassword = password,
            rol,
            estado = "Activo"
        });
        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        Assert.NotNull(dto);
        return dto!;
    }

    private async Task<List<AlertaDto>> GetAlertasAsync()
    {
        var response = await _client.GetAsync("/api/monitoring/alerts");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var alertas = await response.Content.ReadFromJsonAsync<List<AlertaDto>>(JsonOptions);
        Assert.NotNull(alertas);
        return alertas!;
    }

    // Sustituye IUserRepository por uno que falla al guardar, para simular un error de backend real.
    private sealed class ThrowingOnAddUserRepository(IUserRepository inner) : IUserRepository
    {
        public Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken ct = default) =>
            inner.ExistsByEmailAsync(normalizedEmail, ct);
        public Task<bool> AnyAsync(CancellationToken ct = default) => inner.AnyAsync(ct);
        public Task AddAsync(User user, CancellationToken ct = default) =>
            throw new InvalidOperationException("Fallo simulado de base de datos.");
        public Task SaveChangesAsync(CancellationToken ct = default) => inner.SaveChangesAsync(ct);
        public Task<(IReadOnlyList<User> Items, int Total)> ListAsync(
            string? search, UserRole? role, UserStatus? status, int page, int pageSize, CancellationToken ct = default) =>
            inner.ListAsync(search, role, status, page, pageSize, ct);
        public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) => inner.GetByIdAsync(id, ct);
        public Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken ct = default) =>
            inner.FindByEmailAsync(normalizedEmail, ct);
        public Task<bool> ExistsByEmailExceptAsync(string normalizedEmail, Guid excludeId, CancellationToken ct = default) =>
            inner.ExistsByEmailExceptAsync(normalizedEmail, excludeId, ct);
        public Task<int> CountByRoleAsync(UserRole role, CancellationToken ct = default) => inner.CountByRoleAsync(role, ct);
        public Task<int> CountActiveByRoleAsync(UserRole role, CancellationToken ct = default) =>
            inner.CountActiveByRoleAsync(role, ct);
        public Task<User?> GetByEmailIncludingDeletedAsync(string normalizedEmail, CancellationToken ct = default) =>
            inner.GetByEmailIncludingDeletedAsync(normalizedEmail, ct);
    }

    // Scenario (MON-1): Alertar ante un pico de inicios de sesión fallidos
    [Fact]
    public async Task Alerta_ante_un_pico_de_logins_fallidos()
    {
        await CrearAsync("Ana Gomez", "ana@cidenet.com", "Abcdef1x");

        for (var i = 0; i < 5; i++) // umbral por defecto: 5 intentos
        {
            await _client.PostAsJsonAsync(
                "/api/auth/login", new { email = "ana@cidenet.com", password = "Incorrecta1" });
        }

        var alertas = await GetAlertasAsync();
        Assert.Contains(alertas, a => a.Tipo == "LoginBruteForce");
    }

    // Scenario (MON-2): Alertar cuando quedan muy pocos Admin activos
    [Fact]
    public async Task Alerta_cuando_quedan_pocos_admin_activos()
    {
        var segundoAdmin = await CrearAsync("Segundo Admin", "segundo@cidenet.com", rol: "Admin");

        // Desactivarlo deja un solo Admin activo (el de arranque) — umbral por defecto: 1.
        await _client.PutAsJsonAsync($"/api/users/{segundoAdmin.Id}", new
        {
            nombre = segundoAdmin.Nombre,
            email = segundoAdmin.Email,
            rol = "Admin",
            estado = "Inactivo"
        });

        var alertas = await GetAlertasAsync();
        Assert.Contains(alertas, a => a.Tipo == "LowActiveAdmins");
    }

    // Scenario (MON-3): Observar y alertar los errores de backend en operaciones
    [Fact]
    public async Task Observa_y_alerta_un_error_de_backend_al_crear_una_cuenta()
    {
        var throwingFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped<IUserRepository>(sp =>
                    new ThrowingOnAddUserRepository(new UserRepository(sp.GetRequiredService<AppDbContext>())));
            });
        });

        using var throwingClient = throwingFactory.CreateClient();
        throwingClient.AuthenticateAs(_adminId);

        var response = await throwingClient.PostAsJsonAsync("/api/users", new
        {
            nombre = "Falla Simulada",
            email = "falla@cidenet.com",
            password = "Abcdef1x",
            confirmPassword = "Abcdef1x",
            rol = "Editor",
            estado = "Activo"
        });

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        var alertas = await GetAlertasAsync();
        Assert.Contains(alertas, a => a.Tipo == "BackendError");
    }
}
