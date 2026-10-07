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
}
