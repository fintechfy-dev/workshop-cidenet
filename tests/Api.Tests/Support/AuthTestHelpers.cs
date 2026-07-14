using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Users;

namespace Api.Tests.Support;

/// <summary>
/// Helpers de autenticación para tests. Desde el cierre de auditoría (MNT-1),
/// el sistema siembra un Admin al arrancar (<see cref="SeedOptions"/>); los
/// tests se autentican como ESE Admin vía login real. Ya no existe una vía de
/// "primer POST /api/users sin autenticar gana" — quedó cerrada a propósito.
/// </summary>
public static class AuthTestHelpers
{
    public static async Task<Guid> BootstrapAdminAsync(this HttpClient client)
    {
        var seed = new SeedOptions();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = seed.AdminEmail,
            password = seed.AdminPassword
        });
        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<JsonElement>();
        return dto.GetProperty("id").GetGuid();
    }

    /// <summary>Fija el actor por defecto de este cliente (reemplaza cualquier valor previo).</summary>
    public static void AuthenticateAs(this HttpClient client, Guid actorId)
    {
        client.DefaultRequestHeaders.Remove("X-User-Id");
        client.DefaultRequestHeaders.Add("X-User-Id", actorId.ToString());
    }
}
