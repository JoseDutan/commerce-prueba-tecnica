using System.Globalization;
using System.Text;
using Commerce.Api.Exceptions;
using Commerce.Api.Models;
using Commerce.Api.Options;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;
// CsvHelper también define un ValidationException: usamos siempre el de la aplicación
using ValidationException = Commerce.Api.Exceptions.ValidationException;

namespace Commerce.Api.Services;

/// <inheritdoc />
public sealed class CsvCommerceParser : ICsvCommerceParser
{
    /// <summary>Máximo de errores devueltos al cliente para no generar respuestas enormes.</summary>
    private const int MaxReportedErrors = 20;

    /// <summary>Columnas esperadas, en orden, con la longitud máxima de la tabla commerce.</summary>
    private static readonly (string Name, int MaxLength)[] Columns =
    [
        ("pc_processdate", 10),
        ("pc_tipdoc", 10),
        ("pc_numdoc", 50),
        ("pc_nomcomred", 200),
        ("pc_nomcomlegal", 200),
        ("pc_direccion", 300),
        ("pc_telefono", 30),
        ("pc_email", 150),
        ("pc_estado", 20),
    ];

    private readonly CsvOptions _options;

    public CsvCommerceParser(IOptions<CsvOptions> options)
    {
        _options = options.Value;
    }

    public async Task<IReadOnlyList<CommerceRecord>> ParseAsync(Stream content, CancellationToken cancellationToken)
    {
        var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = _options.Delimiter,
            BadDataFound = null,
        };

        // UTF-8 estricto: un archivo en otra codificación falla en lugar de guardar acentos corruptos
        var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
        using var reader = new StreamReader(content, utf8, detectEncodingFromByteOrderMarks: true);
        using var csv = new CsvParser(reader, configuration);

        var records = new List<CommerceRecord>();
        var errors = new List<string>();

        try
        {
            if (!await csv.ReadAsync())
            {
                throw new ValidationException("El archivo está vacío.");
            }

            ValidateHeader(csv.Record!);

            while (await csv.ReadAsync())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var record = ParseRow(csv.Record!, csv.Row, errors);
                if (record is not null)
                {
                    records.Add(record);
                }
            }
        }
        catch (DecoderFallbackException)
        {
            throw new ValidationException("El archivo debe estar codificado en UTF-8.");
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(
                $"El archivo tiene {errors.Count} error(es) de formato.",
                errors.Take(MaxReportedErrors).ToList());
        }

        if (records.Count == 0)
        {
            throw new ValidationException("El archivo está vacío: no contiene registros.");
        }

        return records;
    }

    private static void ValidateHeader(string[] header)
    {
        var expected = Columns.Select(c => c.Name);
        var actual = header.Select(h => h.Trim());

        if (!expected.SequenceEqual(actual, StringComparer.OrdinalIgnoreCase))
        {
            throw new ValidationException(
                "La cabecera del archivo no es válida.",
                [$"Se esperaba: {string.Join(",", expected)}"]);
        }
    }

    /// <summary>
    /// Convierte una fila. Si es inválida agrega el motivo a <paramref name="errors"/> y devuelve null.
    /// Las reglas de negocio (nomcomred, numdoc) no se validan aquí: las aplica el proceso en SQL.
    /// </summary>
    private CommerceRecord? ParseRow(string[] fields, int line, List<string> errors)
    {
        if (fields.Length != Columns.Length)
        {
            errors.Add($"Línea {line}: se esperaban {Columns.Length} columnas y hay {fields.Length}.");
            return null;
        }

        var isValid = true;
        for (var i = 0; i < Columns.Length; i++)
        {
            if (fields[i].Length > Columns[i].MaxLength)
            {
                errors.Add($"Línea {line}: {Columns[i].Name} supera los {Columns[i].MaxLength} caracteres.");
                isValid = false;
            }
        }

        if (!DateOnly.TryParseExact(fields[0].Trim(), _options.DateFormats,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var processDate))
        {
            errors.Add($"Línea {line}: pc_processdate '{fields[0]}' no es una fecha válida.");
            isValid = false;
        }

        return isValid
            ? new CommerceRecord(processDate, fields[1], fields[2], fields[3], fields[4],
                fields[5], fields[6], fields[7], fields[8])
            : null;
    }
}
