namespace Domain.Common;

/// <summary>
/// Se lanza cuando se viola una invariante o regla de negocio del dominio.
/// La capa de API la traduce a un 400 Bad Request con el mensaje.
/// </summary>
public class DomainValidationException : Exception
{
    public DomainValidationException(string message) : base(message)
    {
    }
}
