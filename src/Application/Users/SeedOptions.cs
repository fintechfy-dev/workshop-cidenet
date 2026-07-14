namespace Application.Users;

/// <summary>
/// Credenciales del Admin sembrado al arranque en una base vacía (MNT-1),
/// configurables vía la sección "Seed" (mismo patrón que MonitoringOptions).
/// </summary>
public sealed class SeedOptions
{
    public string AdminName { get; init; } = "Admin";

    // Dominio deliberadamente distinto de "@cidenet.com": ese es el dominio
    // que usan los datos de prueba en toda la suite (tests/Api.Tests), y una
    // colisión aquí infla conteos de búsqueda por dominio o choca con el
    // email exacto de algún usuario de prueba (ver auditoría de cierre).
    public string AdminEmail { get; init; } = "admin@sistema.local";
    public string AdminPassword { get; init; } = "ChangeMe123!";
}
