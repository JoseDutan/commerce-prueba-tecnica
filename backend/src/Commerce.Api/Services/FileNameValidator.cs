using System.Globalization;
using System.Text.RegularExpressions;

namespace Commerce.Api.Services;

/// <summary>
/// Valida el nombre del archivo de carga: commerce_DDMMYYYY.csv con una fecha real.
/// </summary>
public static partial class FileNameValidator
{
    [GeneratedRegex(@"^commerce_(\d{8})\.csv$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex FileNamePattern();

    /// <summary>
    /// Devuelve true si el nombre cumple el patrón y la fecha existe en el calendario.
    /// </summary>
    public static bool TryGetFileDate(string? fileName, out DateOnly fileDate)
    {
        fileDate = default;
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        var match = FileNamePattern().Match(fileName);
        return match.Success
            && DateOnly.TryParseExact(match.Groups[1].Value, "ddMMyyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out fileDate);
    }
}
