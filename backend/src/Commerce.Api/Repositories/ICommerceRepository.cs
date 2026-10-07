using Commerce.Api.Models;

namespace Commerce.Api.Repositories;

/// <summary>
/// Acceso a datos de comercios. Toda operación pasa por un stored procedure.
/// </summary>
public interface ICommerceRepository
{
    /// <summary>
    /// Inserta los registros en commerce mediante sp_create_commerce. Devuelve las filas insertadas.
    /// </summary>
    Task<int> CreateAsync(IReadOnlyCollection<CommerceRecord> records, CancellationToken cancellationToken);
}
