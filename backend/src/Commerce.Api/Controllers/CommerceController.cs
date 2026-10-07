using Commerce.Api.Dtos;
using Commerce.Api.Exceptions;
using Commerce.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Commerce.Api.Controllers;

/// <summary>
/// Carga, procesamiento y consulta de comercios.
/// </summary>
[ApiController]
[Route("api/commerce")]
[Produces("application/json")]
public sealed class CommerceController : ControllerBase
{
    /// <summary>Tamaño máximo aceptado para el archivo de carga (10 MB).</summary>
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    private readonly ICommerceService _service;

    public CommerceController(ICommerceService service)
    {
        _service = service;
    }

    /// <summary>
    /// Carga un archivo commerce_DDMMYYYY.csv e inserta sus filas en la tabla commerce.
    /// </summary>
    /// <param name="file">Archivo CSV en UTF-8 (campo "file" del formulario).</param>
    /// <param name="cancellationToken">Se cancela si el cliente aborta la petición.</param>
    /// <response code="200">Archivo cargado; devuelve la cantidad de filas insertadas.</response>
    /// <response code="400">Archivo ausente, vacío, con nombre inválido o mal formado.</response>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxFileSizeBytes)]
    [ProducesResponseType(typeof(UploadResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UploadResultDto>> Upload(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            throw new ValidationException("El archivo está vacío.");
        }

        await using var stream = file.OpenReadStream();
        var result = await _service.UploadAsync(file.FileName, stream, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Procesa los comercios de una fecha: los que incumplen las validaciones se
    /// eliminan de commerce y se insertan en commerce_quarantine con su motivo.
    /// </summary>
    /// <param name="request">Fecha de proceso (pc_processdate).</param>
    /// <param name="cancellationToken">Se cancela si el cliente aborta la petición.</param>
    /// <response code="200">Cantidad de registros insertados en commerce_quarantine.</response>
    /// <response code="400">Fecha ausente o con formato inválido.</response>
    [HttpPost("process")]
    [ProducesResponseType(typeof(ProcessResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProcessResultDto>> Process(ProcessRequestDto request, CancellationToken cancellationToken)
    {
        // [Required] garantiza que la fecha no es null al llegar aquí
        var result = await _service.ProcessAsync(request.ProcessDate!.Value, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Lista los comercios en cuarentena con el motivo del rechazo.
    /// </summary>
    /// <param name="cancellationToken">Se cancela si el cliente aborta la petición.</param>
    /// <response code="200">Registros de commerce_quarantine.</response>
    [HttpGet("quarantine")]
    [ProducesResponseType(typeof(IReadOnlyList<QuarantineRecordDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<QuarantineRecordDto>>> GetQuarantine(CancellationToken cancellationToken)
    {
        var records = await _service.GetQuarantineAsync(cancellationToken);

        return Ok(records);
    }
}
