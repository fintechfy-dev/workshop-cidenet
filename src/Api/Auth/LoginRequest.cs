namespace Api.Auth;

/// <summary>Cuerpo JSON del POST /api/auth/login (US-007).</summary>
public sealed record LoginRequest(string? Email, string? Password);
