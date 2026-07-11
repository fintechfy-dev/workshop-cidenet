using Api.Auth;
using Api.Permissions;
using Api.Users;
using Application.Permissions;
using Application.Users;
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
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<CreateUserService>();
builder.Services.AddScoped<ListUsersService>();
builder.Services.AddScoped<EditUserService>();
builder.Services.AddScoped<DeleteUserService>();
builder.Services.AddScoped<EditMyProfileService>();
builder.Services.AddScoped<PermissionsService>();
builder.Services.AddScoped<LoginService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(FrontendCorsPolicy);

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithName("HealthCheck");

app.MapGet("/api/users", async (
    HttpRequest httpRequest, IUserRepository users,
    string? search, string? rol, string? estado, int? page, int? pageSize,
    ListUsersService service) =>
{
    // Admin gestiona; Editor ve en solo lectura (mismo GET, sin acciones de escritura); Viewer sin acceso.
    var (_, denied) = await AuthorizeAsync(httpRequest, users, UserRole.Admin, UserRole.Editor);
    if (denied is not null)
    {
        return denied;
    }

    var result = await service.ListAsync(new ListUsersQuery(
        search, rol, estado, page ?? 1, pageSize ?? 10));

    return Results.Ok(result);
})
.WithName("ListUsers");

app.MapPost("/api/users", async (HttpRequest httpRequest, IUserRepository users, CreateUserRequest request, CreateUserService service) =>
{
    // Arranque: la primera cuenta del sistema (el Admin semilla) se crea sin
    // exigir autorización porque todavía no existe ningún Admin que la otorgue.
    if (await users.AnyAsync())
    {
        var (_, denied) = await AuthorizeAsync(httpRequest, users, UserRole.Admin);
        if (denied is not null)
        {
            return denied;
        }
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

app.MapPut("/api/users/{id:guid}", async (
    HttpRequest httpRequest, IUserRepository users, Guid id, EditUserRequest request, EditUserService service) =>
{
    var (actor, denied) = await AuthorizeAsync(httpRequest, users, UserRole.Admin);
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

app.MapDelete("/api/users/{id:guid}", async (HttpRequest httpRequest, IUserRepository users, Guid id, DeleteUserService service) =>
{
    var (actor, denied) = await AuthorizeAsync(httpRequest, users, UserRole.Admin);
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

app.MapGet("/api/users/me", async (HttpRequest httpRequest, IUserRepository users) =>
{
    var (actor, denied) = await AuthorizeAsync(httpRequest, users);
    if (denied is not null)
    {
        return denied;
    }

    return Results.Ok(new UserDto(actor!.Id, actor.Name, actor.Email, actor.Role.ToString(), actor.Status.ToString()));
})
.WithName("GetMyProfile");

app.MapPut("/api/users/me", async (HttpRequest httpRequest, IUserRepository users, EditMyProfileRequest body, EditMyProfileService service) =>
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

app.MapGet("/api/permissions", async (HttpRequest httpRequest, IUserRepository users, PermissionsService service) =>
{
    var (_, denied) = await AuthorizeAsync(httpRequest, users, UserRole.Admin);
    if (denied is not null)
    {
        return denied;
    }

    return Results.Ok(await service.GetMatrixAsync());
})
.WithName("GetPermissionMatrix");

app.MapPut("/api/permissions", async (
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

app.MapPost("/api/auth/login", async (LoginRequest request, LoginService service) =>
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

// El AppDbContext queda registrado y listo. Cuando definas tu dominio y tu
// primera migración, aplícala al arrancar (ej.):
//   if (!app.Environment.IsEnvironment("Testing"))
//   {
//       using var scope = app.Services.CreateScope();
//       await scope.ServiceProvider.GetRequiredService<AppDbContext>()
//           .Database.MigrateAsync();
//   }

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

public partial class Program
{
}
