using Commerce.Api.Services;

namespace Commerce.Api.Tests;

public class FileNameValidatorTests
{
    [Theory]
    [InlineData("commerce_06102026.csv", 2026, 10, 6)]
    [InlineData("commerce_29022024.csv", 2024, 2, 29)] // año bisiesto
    [InlineData("COMMERCE_01012025.CSV", 2025, 1, 1)]
    public void TryGetFileDate_ValidName_ReturnsTrueAndDate(string fileName, int year, int month, int day)
    {
        var isValid = FileNameValidator.TryGetFileDate(fileName, out var fileDate);

        Assert.True(isValid);
        Assert.Equal(new DateOnly(year, month, day), fileDate);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("datos.csv")]
    [InlineData("commerce_06102026.txt")]
    [InlineData("commerce_0610206.csv")]        // 7 dígitos
    [InlineData("commerce_061020266.csv")]      // 9 dígitos
    [InlineData("commerce_2026-10-06.csv")]     // formato de fecha distinto
    [InlineData("commerce_31022026.csv")]       // 31 de febrero no existe
    [InlineData("commerce_29022025.csv")]       // 2025 no es bisiesto
    [InlineData("commerce_06132026.csv")]       // mes 13
    [InlineData("x_commerce_06102026.csv")]     // prefijo extra
    [InlineData("commerce_06102026.csv.exe")]   // sufijo extra
    public void TryGetFileDate_InvalidName_ReturnsFalse(string? fileName)
    {
        var isValid = FileNameValidator.TryGetFileDate(fileName, out _);

        Assert.False(isValid);
    }
}
