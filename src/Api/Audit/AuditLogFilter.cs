using Application.Audit;
using Domain.Audit;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Audit;

/// <summary>
/// Registra en el audit trail (US-001-AUD) cada request que pasa por el grupo
/// `/api`: actor (X-User-Id), acción (nombre del endpoint, ya declarado con
/// .WithName en cada ruta), entidad (segmento de la URL) y marca de tiempo
/// (CreatedAt de la entidad). Se aplica una sola vez a nivel de grupo en vez
/// de instrumentar cada servicio de aplicación por separado.
/// </summary>
public sealed class AuditLogFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var result = await next(context);

        var httpContext = context.HttpContext;
        var auditLog = httpContext.RequestServices.GetRequiredService<IAuditLogRepository>();

        Guid? actorId = null;
        if (httpContext.Request.Headers.TryGetValue("X-User-Id", out var raw) && Guid.TryParse(raw, out var parsed))
        {
            actorId = parsed;
        }

        var action = httpContext.GetEndpoint()?.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName ?? "Unknown";
        var entityType = InferEntityType(httpContext.Request.Path);
        var entityId = httpContext.Request.RouteValues.TryGetValue("id", out var idValue)
            ? idValue?.ToString()
            : httpContext.Request.Path.Value?.EndsWith("/me", StringComparison.OrdinalIgnoreCase) == true
                ? actorId?.ToString()
                : null;

        await auditLog.AppendAsync(AuditLogEntry.Create(actorId, action, entityType, entityId));

        return result;
    }

    private static string InferEntityType(PathString path)
    {
        var segments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? [];
        return segments.Length > 1 ? segments[1] : "unknown";
    }
}
