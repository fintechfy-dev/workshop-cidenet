namespace Application.Monitoring;

/// <summary>
/// Umbrales configurables de observabilidad (US-001-MON). Se registra como
/// singleton en Api/Program.cs, enlazado desde la sección "Monitoring" de la
/// configuración si existe; si no, aplican estos valores por defecto.
/// </summary>
public sealed class MonitoringOptions
{
    /// <summary>MON-1: cuántos intentos fallidos, dentro de la ventana, disparan la alerta.</summary>
    public int FailedLoginThreshold { get; set; } = 5;

    /// <summary>MON-1: ventana de tiempo (minutos) en la que se cuentan los intentos fallidos.</summary>
    public int FailedLoginWindowMinutes { get; set; } = 15;

    /// <summary>MON-2: alerta cuando los Admin activos caen a este número o menos.</summary>
    public int LowActiveAdminThreshold { get; set; } = 1;
}
