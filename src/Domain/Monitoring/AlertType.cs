namespace Domain.Monitoring;

/// <summary>Los tres eventos observables del caso (US-001-MON): MON-1, MON-2, MON-3.</summary>
public enum AlertType
{
    LoginBruteForce,
    LowActiveAdmins,
    BackendError
}
