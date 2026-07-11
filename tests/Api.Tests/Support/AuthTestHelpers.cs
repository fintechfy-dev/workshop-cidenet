using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Tests.Support;

/// <summary>
/// Helpers de autenticación para tests (It10, autorización transversal).
/// La primera cuenta del sistema (el Admin de arranque) se crea sin exigir
/// autorización porque todavía no hay ningún Admin que la otorgue; de ahí en
/// adelante, cada llamada debe indicar el actor mediante X-User-Id.
/// </summary>
public static class AuthTestHelpers
{
    public static async Task<Guid> BootstrapAdminAsync(this HttpClient client, string email = "bootstrap-admin@system.local")
    {
        var response = await client.PostAsJsonAsync("/api/users", new
        {
            nombre = "Admin Bootstrap",
            email,
            password = "Abcdef1x",
            confirmPassword = "Abcdef1x",
            rol = "Admin",
            estado = "Activo"
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
