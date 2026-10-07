using Commerce.Api.Models;

namespace Commerce.Api.Services;

/// <summary>
/// Convierte el contenido de un archivo commerce_DDMMYYYY.csv en registros tipados.
/// </summary>
public interface ICsvCommerceParser
{
    /// <exception cref="Exceptions.ValidationException">
    /// El archivo está vacío, no es UTF-8 o tiene filas con formato inválido.
    /// </exception>
    Task<IReadOnlyList<CommerceRecord>> ParseAsync(Stream content, CancellationToken cancellationToken);
}
