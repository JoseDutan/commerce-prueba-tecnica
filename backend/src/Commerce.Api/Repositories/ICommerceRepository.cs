using Commerce.Api.Dtos;
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

    /// <summary>
    /// Valida los comercios de la fecha mediante sp_process_commerce.
    /// Devuelve los registros movidos a commerce_quarantine.
    /// </summary>
    Task<int> ProcessAsync(DateOnly processDate, CancellationToken cancellationToken);

    /// <summary>
    /// Lista los comercios en cuarentena mediante sp_get_commerce_quarantine.
    /// </summary>
    Task<IReadOnlyList<QuarantineRecordDto>> GetQuarantineAsync(CancellationToken cancellationToken);
}
