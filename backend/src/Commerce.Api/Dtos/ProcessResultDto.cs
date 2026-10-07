namespace Commerce.Api.Dtos;

/// <summary>
/// Resultado de procesar un día.
/// </summary>
/// <param name="ProcessDate">Fecha de proceso solicitada.</param>
/// <param name="QuarantinedCount">Registros insertados en commerce_quarantine.</param>
public sealed record ProcessResultDto(DateOnly ProcessDate, int QuarantinedCount);
