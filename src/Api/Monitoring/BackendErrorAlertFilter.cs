using Application.Monitoring;
using Domain.Monitoring;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Monitoring;

/// <summary>
/// MON-3: si una operación del grupo /api lanza una excepción no controlada
/// (fallo de backend real, no un rechazo de negocio — esos ya se devuelven
/// como 400/409/403 sin lanzar), la observa (log + alerta persistida) y
/// responde 500 sin filtrar detalles internos. Envuelve a <c>AuditLogFilter</c>
/// para poder capturar también sus fallos.
/// </summary>
public sealed class BackendErrorAlertFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try
        {
            return await next(context);
        }
        catch (Exception ex)
        {
            var httpContext = context.HttpContext;
            var alerts = httpContext.RequestServices.GetRequiredService<AlertService>();
            var action = httpContext.GetEndpoint()?.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName ?? "Unknown";

            await alerts.RaiseAsync(AlertType.BackendError, $"Error de backend en \"{action}\": {ex.Message}");

            return Results.Json(
                new { error = "Ocurrió un error interno. Intente de nuevo." },
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
