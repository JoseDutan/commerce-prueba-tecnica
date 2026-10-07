namespace Commerce.Api.Options;

/// <summary>
/// Configuración del parseo del CSV (sección "Csv" de appsettings.json).
/// </summary>
public sealed class CsvOptions
{
    public const string SectionName = "Csv";

    /// <summary>Separador de columnas.</summary>
    public string Delimiter { get; set; } = ",";

    /// <summary>Formatos aceptados para pc_processdate.</summary>
    public string[] DateFormats { get; set; } = ["yyyy-MM-dd"];
}
