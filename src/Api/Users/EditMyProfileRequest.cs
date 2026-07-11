namespace Api.Users;

/// <summary>
/// Cuerpo JSON del PUT /api/users/me (US-005). Sin rol ni estado: son de solo
/// lectura desde el perfil propio (R2/R3). La contraseña es opcional.
/// </summary>
public sealed record EditMyProfileRequest(
    string? Nombre,
    string? Email,
    string? CurrentPassword,
    string? NewPassword,
    string? ConfirmNewPassword);
