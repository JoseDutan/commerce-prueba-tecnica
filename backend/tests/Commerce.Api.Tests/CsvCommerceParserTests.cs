using System.Text;
using Commerce.Api.Exceptions;
using Commerce.Api.Options;
using Commerce.Api.Services;

namespace Commerce.Api.Tests;

public class CsvCommerceParserTests
{
    private const string Header =
        "pc_processdate,pc_tipdoc,pc_numdoc,pc_nomcomred,pc_nomcomlegal,pc_direccion,pc_telefono,pc_email,pc_estado";

    private static CsvCommerceParser CreateParser(CsvOptions? options = null)
        => new(Microsoft.Extensions.Options.Options.Create(options ?? new CsvOptions()));

    private static MemoryStream Utf8(string content) => new(Encoding.UTF8.GetBytes(content));

    [Fact]
    public async Task ParseAsync_EmptyFile_ThrowsValidationException()
    {
        var parser = CreateParser();

        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => parser.ParseAsync(new MemoryStream(), CancellationToken.None));

        Assert.Contains("vacío", ex.Message);
    }

    [Fact]
    public async Task ParseAsync_HeaderWithoutRows_ThrowsValidationException()
    {
        var parser = CreateParser();

        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => parser.ParseAsync(Utf8(Header + "\n"), CancellationToken.None));

        Assert.Contains("vacío", ex.Message);
    }

    [Fact]
    public async Task ParseAsync_ValidFile_ReturnsRecordsKeepingAccentsAndLeadingZeros()
    {
        var parser = CreateParser();
        var csv = Header + "\n" +
            "2026-10-06,CED,0912345678,Panadería El Trigal,Juan Pérez,Cdla. Alborada,0991112233,a@mail.com,ACTIVO\n" +
            "2026-10-07,RUC,,   ,Sin Datos S.A.,Calle 1,042000000,b@mail.com,ACTIVO\n";

        var records = await parser.ParseAsync(Utf8(csv), CancellationToken.None);

        Assert.Equal(2, records.Count);
        Assert.Equal(new DateOnly(2026, 10, 6), records[0].PcProcessDate);
        Assert.Equal("0912345678", records[0].PcNumDoc);
        Assert.Equal("Panadería El Trigal", records[0].PcNomComRed);
        Assert.Equal("Juan Pérez", records[0].PcNomComLegal);
        // Las reglas de negocio no se aplican al parsear: los vacíos se cargan tal cual
        Assert.Equal(string.Empty, records[1].PcNumDoc);
        Assert.Equal("   ", records[1].PcNomComRed);
    }

    [Fact]
    public async Task ParseAsync_InvalidHeader_ThrowsValidationException()
    {
        var parser = CreateParser();

        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => parser.ParseAsync(Utf8("fecha,documento\n2026-10-06,123\n"), CancellationToken.None));

        Assert.Contains("cabecera", ex.Message);
    }

    [Fact]
    public async Task ParseAsync_MalformedRows_ReportsEveryLine()
    {
        var parser = CreateParser();
        var csv = Header + "\n" +
            "06-10-2026,RUC,1,a,b,c,d,e,f\n" + // fecha con formato no admitido
            "2026-10-06,RUC,1\n";               // faltan columnas

        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => parser.ParseAsync(Utf8(csv), CancellationToken.None));

        Assert.Equal(2, ex.Errors.Count);
        Assert.Contains("Línea 2", ex.Errors[0]);
        Assert.Contains("Línea 3", ex.Errors[1]);
    }

    [Fact]
    public async Task ParseAsync_ConfiguredDelimiterAndDateFormat_AreApplied()
    {
        var parser = CreateParser(new CsvOptions { Delimiter = ";", DateFormats = ["dd/MM/yyyy"] });
        var csv = Header.Replace(',', ';') + "\n" +
            "06/10/2026;RUC;0991234567001;Tienda;Tienda S.A.;Av. 1;042345678;t@mail.com;ACTIVO\n";

        var records = await parser.ParseAsync(Utf8(csv), CancellationToken.None);

        var record = Assert.Single(records);
        Assert.Equal(new DateOnly(2026, 10, 6), record.PcProcessDate);
    }

    [Fact]
    public async Task ParseAsync_NotUtf8_ThrowsValidationException()
    {
        var parser = CreateParser();
        // "Panadería" en Latin-1: el byte 0xED suelto no es UTF-8 válido
        var bytes = Encoding.Latin1.GetBytes(Header + "\n2026-10-06,CED,1,Panadería,b,c,d,e,f\n");

        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => parser.ParseAsync(new MemoryStream(bytes), CancellationToken.None));

        Assert.Contains("UTF-8", ex.Message);
    }
}
