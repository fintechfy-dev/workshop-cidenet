using Api.Users;
using Application.Users;
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
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<CreateUserService>();
builder.Services.AddScoped<ListUsersService>();
builder.Services.AddScoped<EditUserService>();
builder.Services.AddScoped<DeleteUserService>();
builder.Services.AddScoped<EditMyProfileService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(FrontendCorsPolicy);

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithName("HealthCheck");

app.MapGet("/api/users", async (
    string? search, string? rol, string? estado, int? page, int? pageSize,
    ListUsersService service) =>
{
    var result = await service.ListAsync(new ListUsersQuery(
        search, rol, estado, page ?? 1, pageSize ?? 10));

    return Results.Ok(result);
})
.WithName("ListUsers");

app.MapPost("/api/users", async (CreateUserRequest request, CreateUserService service) =>
{
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

app.MapPut("/api/users/{id:guid}", async (Guid id, EditUserRequest request, EditUserService service) =>
{
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

app.MapDelete("/api/users/{id:guid}", async (Guid id, DeleteUserService service) =>
{
    var result = await service.DeleteAsync(id);

    return result.Outcome switch
    {
        DeleteUserOutcome.Deleted => Results.NoContent(),
        DeleteUserOutcome.NotFound => Results.NotFound(),
        _ => Results.Conflict(new { error = result.Error })
    };
})
.WithName("DeleteUser");

app.MapGet("/api/users/me", async (HttpRequest request, IUserRepository users) =>
{
    if (!TryGetActorId(request, out var actorId))
    {
        return Results.Unauthorized();
    }

    var actor = await users.GetByIdAsync(actorId);
    if (actor is null)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new UserDto(actor.Id, actor.Name, actor.Email, actor.Role.ToString(), actor.Status.ToString()));
})
.WithName("GetMyProfile");

app.MapPut("/api/users/me", async (HttpRequest request, EditMyProfileRequest body, EditMyProfileService service) =>
{
    if (!TryGetActorId(request, out var actorId))
    {
        return Results.Unauthorized();
    }

    var result = await service.EditAsync(new EditMyProfileCommand(
        actorId, body.Nombre, body.Email, body.CurrentPassword, body.NewPassword, body.ConfirmNewPassword));

    return result.Outcome switch
    {
        EditMyProfileOutcome.Updated => Results.Ok(result.User),
        EditMyProfileOutcome.Conflict => Results.Conflict(new { error = result.Error }),
        EditMyProfileOutcome.Unauthorized => Results.Unauthorized(),
        _ => Results.BadRequest(new { error = result.Error })
    };
})
.WithName("EditMyProfile");

// El AppDbContext queda registrado y listo. Cuando definas tu dominio y tu
// primera migración, aplícala al arrancar (ej.):
//   if (!app.Environment.IsEnvironment("Testing"))
//   {
//       using var scope = app.Services.CreateScope();
//       await scope.ServiceProvider.GetRequiredService<AppDbContext>()
//           .Database.MigrateAsync();
//   }

app.Run();

// Marcador provisional de identidad (X-User-Id) hasta que la autenticación real
// (It 9–10) resuelva el actor desde la sesión en vez de un header explícito.
static bool TryGetActorId(HttpRequest request, out Guid actorId)
{
    actorId = Guid.Empty;
    return request.Headers.TryGetValue("X-User-Id", out var raw)
        && Guid.TryParse(raw, out actorId);
}

public partial class Program
{
}
