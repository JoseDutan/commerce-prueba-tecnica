using Commerce.Api.Dtos;

namespace Commerce.Api.Services;

/// <summary>
/// Casos de uso de comercios: reglas de aplicación entre el controlador y el repositorio.
/// </summary>
public interface ICommerceService
{
    /// <summary>
    /// Valida el nombre y el contenido del archivo e inserta sus filas en commerce.
    /// </summary>
    /// <exception cref="Exceptions.ValidationException">Nombre inválido, archivo vacío o mal formado.</exception>
    Task<UploadResultDto> UploadAsync(string fileName, Stream content, CancellationToken cancellationToken);

    /// <summary>
    /// Procesa los comercios de un día: los inválidos pasan a cuarentena.
    /// </summary>
    Task<ProcessResultDto> ProcessAsync(DateOnly processDate, CancellationToken cancellationToken);

    /// <summary>
    /// Lista los comercios en cuarentena con su motivo.
    /// </summary>
    Task<IReadOnlyList<QuarantineRecordDto>> GetQuarantineAsync(CancellationToken cancellationToken);
}
