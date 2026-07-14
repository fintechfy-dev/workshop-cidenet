using Api.Audit;
using Api.Auth;
using Api.Monitoring;
using Api.Permissions;
using Api.Users;
using Application.Audit;
using Application.Monitoring;
using Application.Permissions;
using Application.Users;
using Domain.Permissions;
using Domain.Users;
using Infrastructure.Persistence;
using Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

const string FrontendCorsPolicy = "frontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins(
                builder.Configuration["FRONTEND_URL"] ?? "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? "Host=localhost;Port=5432;Database=workshop_cidenet;Username=postgres;Password=postgres";

// En pruebas, el proveedor de base de datos lo registra el harness (EF InMemory);
// fuera de pruebas usamos Postgres.
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
}

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<IFailedLoginTracker, FailedLoginTracker>();
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddSingleton(
    builder.Configuration.GetSection("Monitoring").Get<MonitoringOptions>() ?? new MonitoringOptions());
builder.Services.AddSingleton(
    builder.Configuration.GetSection("Seed").Get<SeedOptions>() ?? new SeedOptions());
builder.Services.AddScoped<AlertService>();
builder.Services.AddScoped<CreateUserService>();
builder.Services.AddScoped<ListUsersService>();
builder.Services.AddScoped<EditUserService>();
builder.Services.AddScoped<DeleteUserService>();
builder.Services.AddScoped<EditMyProfileService>();
builder.Services.AddScoped<PermissionsService>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<AdminSeedService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Api v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseCors(FrontendCorsPolicy);

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithName("HealthCheck");

// Todo lo bajo /api pasa por: BackendErrorAlertFilter (MON-3, envuelve todo:
// observa y alerta ante una excepción no controlada) y AuditLogFilter
// (US-001-AUD: registra actor, acción, entidad y marca de tiempo de cada
// request) — sin tener que instrumentar cada servicio uno por uno.
var api = app.MapGroup("/api")
    .AddEndpointFilter<BackendErrorAlertFilter>()
    .AddEndpointFilter<AuditLogFilter>();

api.MapGet("/users", async (
    HttpRequest httpRequest, IUserRepository users, IPermissionRepository permissions,
    string? search, string? rol, string? estado, int? page, int? pageSize,
    ListUsersService service) =>
{
    var (_, denied) = await AuthorizePermissionAsync(httpRequest, users, permissions, PermissionResource.Users, PermissionAction.Read);
    if (denied is not null)
    {
        return denied;
    }

    var result = await service.ListAsync(new ListUsersQuery(
        search, rol, estado, page ?? 1, pageSize ?? 10));

    return Results.Ok(result);
})
.WithName("ListUsers");

api.MapPost("/users", async (
    HttpRequest httpRequest, IUserRepository users, IPermissionRepository permissions,
    CreateUserRequest request, CreateUserService service) =>
{
    var (_, denied) = await AuthorizePermissionAsync(httpRequest, users, permissions, PermissionResource.Users, PermissionAction.Create);
    if (denied is not null)
    {
        return denied;
    }

    var result = await service.CreateAsync(new CreateUserCommand(
        request.Nombre,
        request.Email,
        request.Password,
        request.ConfirmPassword,
        request.Rol,
        request.Estado));

    return result.Outcome switch
    {
        CreateUserOutcome.Created =>
            Results.Created($"/api/users/{result.User!.Id}", result.User),
        CreateUserOutcome.EmailAlreadyExists =>
            Results.Conflict(new { error = result.Error }),
        _ => Results.BadRequest(new { error = result.Error })
    };
})
.WithName("CreateUser");

api.MapPut("/users/{id:guid}", async (
    HttpRequest httpRequest, IUserRepository users, IPermissionRepository permissions,
    Guid id, EditUserRequest request, EditUserService service) =>
{
    var (actor, denied) = await AuthorizePermissionAsync(httpRequest, users, permissions, PermissionResource.Users, PermissionAction.Update);
    if (denied is not null)
    {
        return denied;
    }

    // R2: un usuario no puede cambiar su propio rol (aquí ya sabemos quién es el actor).
    if (actor!.Id == id
        && !string.IsNullOrWhiteSpace(request.Rol)
        && !string.Equals(request.Rol.Trim(), actor.Role.ToString(), StringComparison.OrdinalIgnoreCase))
    {
        return Results.Conflict(new { error = "No puedes cambiar tu propio rol (R2)." });
    }

    var result = await service.EditAsync(id, new EditUserCommand(
        request.Nombre, request.Email, request.Rol, request.Estado));

    return result.Outcome switch
    {
        EditUserOutcome.Updated => Results.Ok(result.User),
        EditUserOutcome.NotFound => Results.NotFound(),
        EditUserOutcome.Conflict => Results.Conflict(new { error = result.Error }),
        _ => Results.BadRequest(new { error = result.Error })
    };
})
.WithName("EditUser");

api.MapDelete("/users/{id:guid}", async (
    HttpRequest httpRequest, IUserRepository users, IPermissionRepository permissions,
    Guid id, DeleteUserService service) =>
{
    var (actor, denied) = await AuthorizePermissionAsync(httpRequest, users, permissions, PermissionResource.Users, PermissionAction.Delete);
    if (denied is not null)
    {
        return denied;
    }

    // Un Admin no puede eliminar su propia cuenta; debe hacerlo otro Admin.
    if (actor!.Id == id)
    {
        return Results.Conflict(new { error = "No puedes eliminar tu propia cuenta; debe hacerlo otro Admin." });
    }

    var result = await service.DeleteAsync(id);

    return result.Outcome switch
    {
        DeleteUserOutcome.Deleted => Results.NoContent(),
        DeleteUserOutcome.NotFound => Results.NotFound(),
        _ => Results.Conflict(new { error = result.Error })
    };
})
.WithName("DeleteUser");

api.MapGet("/users/me", async (HttpRequest httpRequest, IUserRepository users) =>
{
    var (actor, denied) = await AuthorizeAsync(httpRequest, users);
    if (denied is not null)
    {
        return denied;
    }

    return Results.Ok(new UserDto(actor!.Id, actor.Name, actor.Email, actor.Role.ToString(), actor.Status.ToString()));
})
.WithName("GetMyProfile");

api.MapPut("/users/me", async (HttpRequest httpRequest, IUserRepository users, EditMyProfileRequest body, EditMyProfileService service) =>
{
    var (actor, denied) = await AuthorizeAsync(httpRequest, users);
    if (denied is not null)
    {
        return denied;
    }

    var result = await service.EditAsync(new EditMyProfileCommand(
        actor!.Id, body.Nombre, body.Email, body.CurrentPassword, body.NewPassword, body.ConfirmNewPassword));

    return result.Outcome switch
    {
        EditMyProfileOutcome.Updated => Results.Ok(result.User),
        EditMyProfileOutcome.Conflict => Results.Conflict(new { error = result.Error }),
        EditMyProfileOutcome.Unauthorized => Results.Unauthorized(),
        _ => Results.BadRequest(new { error = result.Error })
    };
})
.WithName("EditMyProfile");

api.MapGet("/permissions", async (HttpRequest httpRequest, IUserRepository users, PermissionsService service) =>
{
    var (_, denied) = await AuthorizeAsync(httpRequest, users, UserRole.Admin);
    if (denied is not null)
    {
        return denied;
    }

    return Results.Ok(await service.GetMatrixAsync());
})
.WithName("GetPermissionMatrix");

api.MapPut("/permissions", async (
    HttpRequest httpRequest, IUserRepository users, EditPermissionsRequest request, PermissionsService service) =>
{
    var (_, denied) = await AuthorizeAsync(httpRequest, users, UserRole.Admin);
    if (denied is not null)
    {
        return denied;
    }

    var changes = (request.Cambios ?? [])
        .Select(c => new PermissionChangeCommand(c.Rol, c.Recurso, c.Accion, c.Permitido))
        .ToList();

    var result = await service.EditMatrixAsync(changes);

    return result.Outcome switch
    {
        EditPermissionsOutcome.Updated => Results.Ok(result.Matrix),
        EditPermissionsOutcome.Conflict => Results.Conflict(new { error = result.Error }),
        _ => Results.BadRequest(new { error = result.Error })
    };
})
.WithName("EditPermissionMatrix");

api.MapPost("/auth/login", async (LoginRequest request, LoginService service) =>
{
    var result = await service.LoginAsync(new LoginCommand(request.Email, request.Password));

    return result.Outcome switch
    {
        LoginOutcome.Success => Results.Ok(result.User),
        LoginOutcome.InactiveAccount => Results.Json(new { error = result.Error }, statusCode: StatusCodes.Status403Forbidden),
        LoginOutcome.ValidationFailed => Results.BadRequest(new { error = result.Error }),
        _ => Results.Json(new { error = result.Error }, statusCode: StatusCodes.Status401Unauthorized)
    };
})
.WithName("Login");

api.MapGet("/audit-log", async (HttpRequest httpRequest, IUserRepository users, IAuditLogRepository auditLog) =>
{
    var (_, denied) = await AuthorizeAsync(httpRequest, users, UserRole.Admin);
    if (denied is not null)
    {
        return denied;
    }

    var entries = await auditLog.GetAllAsync();

    return Results.Ok(entries.Select(e => new
    {
        e.Id,
        actorId = e.ActorId,
        accion = e.Action,
        entidad = e.EntityType,
        entityId = e.EntityId,
        fecha = e.CreatedAt
    }));
})
.WithName("GetAuditLog");

api.MapGet("/monitoring/alerts", async (HttpRequest httpRequest, IUserRepository users, IAlertRepository alerts) =>
{
    var (_, denied) = await AuthorizeAsync(httpRequest, users, UserRole.Admin);
    if (denied is not null)
    {
        return denied;
    }

    var entries = await alerts.GetAllAsync();

    return Results.Ok(entries.Select(a => new { a.Id, tipo = a.Type.ToString(), mensaje = a.Message, fecha = a.CreatedAt }));
})
.WithName("GetAlerts");

// Arranque (MNT-1): aplica la migración inicial (fuera de Testing, donde el
// harness ya sustituye el AppDbContext por EF InMemory) y siembra el Admin
// inicial + la matriz de permisos por defecto si la base está vacía. La
// siembra corre en todo entorno (incluida Testing) porque ahí también hace
// falta un actor conocido con el que autenticar los tests, y porque
// AuthorizeAsync necesita que la matriz ya exista para poder consultarla.
using (var scope = app.Services.CreateScope())
{
    if (!app.Environment.IsEnvironment("Testing"))
    {
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    }

    await scope.ServiceProvider.GetRequiredService<AdminSeedService>().SeedIfNeededAsync();
    await scope.ServiceProvider.GetRequiredService<PermissionsService>().GetMatrixAsync();
}

app.Run();

// Marcador provisional de identidad (X-User-Id, respaldado por credenciales
// reales desde el login de It9) hasta que haya un mecanismo de sesión real.
static bool TryGetActorId(HttpRequest request, out Guid actorId)
{
    actorId = Guid.Empty;
    return request.Headers.TryGetValue("X-User-Id", out var raw)
        && Guid.TryParse(raw, out actorId);
}

/// <summary>
/// Resuelve al actor autenticado y, si se dan roles permitidos, verifica que
/// el suyo esté entre ellos (matriz de autorización, US-001-SEC/It10). Un
/// actor inactivo se trata como no autenticado (US-007-USR: revalida en cada
/// request, sin sesión cacheada que revocar).
/// </summary>
static async Task<(User? Actor, IResult? Denied)> AuthorizeAsync(
    HttpRequest request, IUserRepository users, params UserRole[] allowedRoles)
{
    if (!TryGetActorId(request, out var actorId))
    {
        return (null, Results.Unauthorized());
    }

    var actor = await users.GetByIdAsync(actorId);
    if (actor is null || actor.Status != UserStatus.Activo)
    {
        return (null, Results.Unauthorized());
    }

    if (allowedRoles.Length > 0 && Array.IndexOf(allowedRoles, actor.Role) < 0)
    {
        return (null, Results.Json(
            new { error = "No tiene permisos para realizar esta acción." },
            statusCode: StatusCodes.Status403Forbidden));
    }

    return (actor, null);
}

/// <summary>
/// Variante de AuthorizeAsync que consulta la matriz de permisos (US-006-EDGE2:
/// un cambio en la matriz debe regir la autorización efectiva) en vez de una
/// lista de roles fija en el propio endpoint. El rol Admin siempre pasa: su
/// fila en la matriz es inmutable y queda fija en "permitido" (R3/anti-lockout,
/// ver PermissionsService.EditMatrixAsync).
/// </summary>
static async Task<(User? Actor, IResult? Denied)> AuthorizePermissionAsync(
    HttpRequest request, IUserRepository users, IPermissionRepository permissions,
    PermissionResource resource, PermissionAction action)
{
    if (!TryGetActorId(request, out var actorId))
    {
        return (null, Results.Unauthorized());
    }

    var actor = await users.GetByIdAsync(actorId);
    if (actor is null || actor.Status != UserStatus.Activo)
    {
        return (null, Results.Unauthorized());
    }

    if (actor.Role != UserRole.Admin)
    {
        var permission = await permissions.FindAsync(actor.Role, resource, action);
        if (permission is null || !permission.Allowed)
        {
            return (null, Results.Json(
                new { error = "No tiene permisos para realizar esta acción." },
                statusCode: StatusCodes.Status403Forbidden));
        }
    }

    return (actor, null);
}

public partial class Program
{
}
