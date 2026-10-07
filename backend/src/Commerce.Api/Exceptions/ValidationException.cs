namespace Commerce.Api.Exceptions;

/// <summary>
/// Error de validación de la petición. El middleware global lo traduce a 400 Bad Request.
/// </summary>
public sealed class ValidationException : Exception
{
    public ValidationException(string message, IReadOnlyList<string>? errors = null)
        : base(message)
    {
        Errors = errors ?? [];
    }

    /// <summary>Detalle opcional, por ejemplo los errores por línea del CSV.</summary>
    public IReadOnlyList<string> Errors { get; }
}
