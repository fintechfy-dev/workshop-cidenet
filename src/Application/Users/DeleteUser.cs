namespace Application.Users;

public enum DeleteUserOutcome
{
    Deleted,
    NotFound,
    Conflict
}

public sealed record DeleteUserResult(DeleteUserOutcome Outcome, string? Error)
{
    public static DeleteUserResult Deleted() =>
        new(DeleteUserOutcome.Deleted, null);

    public static DeleteUserResult NotFound() =>
        new(DeleteUserOutcome.NotFound, null);

    public static DeleteUserResult Conflict(string error) =>
        new(DeleteUserOutcome.Conflict, error);
}
