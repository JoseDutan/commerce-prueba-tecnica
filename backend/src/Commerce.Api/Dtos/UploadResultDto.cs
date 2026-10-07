namespace Commerce.Api.Dtos;

/// <summary>
/// Resultado de la carga de un archivo.
/// </summary>
/// <param name="FileName">Nombre del archivo recibido.</param>
/// <param name="InsertedCount">Filas insertadas en la tabla commerce.</param>
public sealed record UploadResultDto(string FileName, int InsertedCount);
