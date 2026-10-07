namespace Commerce.Api.Models;

/// <summary>
/// Fila del archivo commerce_DDMMYYYY.csv. Tiene la misma estructura que la tabla commerce.
/// </summary>
public sealed record CommerceRecord(
    DateOnly PcProcessDate,
    string? PcTipDoc,
    string? PcNumDoc,
    string? PcNomComRed,
    string? PcNomComLegal,
    string? PcDireccion,
    string? PcTelefono,
    string? PcEmail,
    string? PcEstado);
