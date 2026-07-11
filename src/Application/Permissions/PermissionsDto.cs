namespace Application.Permissions;

/// <summary>Una celda de la matriz hacia afuera: rol × recurso × acción y si está permitida.</summary>
public sealed record PermissionCellDto(string Rol, string Recurso, string Accion, bool Permitido);

/// <summary>Un cambio solicitado sobre una celda de la matriz.</summary>
public sealed record PermissionChangeCommand(string? Rol, string? Recurso, string? Accion, bool Permitido);

public enum EditPermissionsOutcome
{
    Updated,
    ValidationFailed,
    Conflict
}

public sealed record EditPermissionsResult(
    EditPermissionsOutcome Outcome, IReadOnlyList<PermissionCellDto>? Matrix, string? Error)
{
    public static EditPermissionsResult Updated(IReadOnlyList<PermissionCellDto> matrix) =>
        new(EditPermissionsOutcome.Updated, matrix, null);

    public static EditPermissionsResult Validation(string error) =>
        new(EditPermissionsOutcome.ValidationFailed, null, error);

    public static EditPermissionsResult Conflict(string error) =>
        new(EditPermissionsOutcome.Conflict, null, error);
}
