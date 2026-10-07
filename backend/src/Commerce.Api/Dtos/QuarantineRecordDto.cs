namespace Commerce.Api.Dtos;

/// <summary>
/// Comercio en cuarentena con el motivo del rechazo.
/// Los nombres coinciden con las columnas de commerce_quarantine (sin guiones bajos).
/// </summary>
public sealed class QuarantineRecordDto
{
    public int Id { get; init; }
    public DateOnly PcProcessDate { get; init; }
    public string? PcTipDoc { get; init; }
    public string? PcNumDoc { get; init; }
    public string? PcNomComRed { get; init; }
    public string? PcNomComLegal { get; init; }
    public string? PcDireccion { get; init; }
    public string? PcTelefono { get; init; }
    public string? PcEmail { get; init; }
    public string? PcEstado { get; init; }

    /// <summary>Motivos del rechazo, unidos por " | " si son varios.</summary>
    public string Motivo { get; init; } = string.Empty;
}
