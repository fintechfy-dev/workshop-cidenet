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

// El AppDbContext queda registrado y listo. Cuando definas tu dominio y tu
// primera migración, aplícala al arrancar (ej.):
//   if (!app.Environment.IsEnvironment("Testing"))
//   {
//       using var scope = app.Services.CreateScope();
//       await scope.ServiceProvider.GetRequiredService<AppDbContext>()
//           .Database.MigrateAsync();
//   }

app.Run();

public partial class Program
{
}
