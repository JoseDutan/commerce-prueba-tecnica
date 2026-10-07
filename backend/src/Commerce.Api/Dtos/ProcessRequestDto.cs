using System.ComponentModel.DataAnnotations;

namespace Commerce.Api.Dtos;

/// <summary>
/// Petición para procesar los comercios de un día.
/// </summary>
public sealed class ProcessRequestDto
{
    /// <summary>Fecha de proceso (pc_processdate) en formato yyyy-MM-dd.</summary>
    /// <example>2026-10-06</example>
    [Required(ErrorMessage = "La fecha de proceso es obligatoria.")]
    public DateOnly? ProcessDate { get; init; }
}
