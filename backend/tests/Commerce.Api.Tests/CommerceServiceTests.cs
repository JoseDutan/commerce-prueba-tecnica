using Commerce.Api.Dtos;
using Commerce.Api.Exceptions;
using Commerce.Api.Models;
using Commerce.Api.Repositories;
using Commerce.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Commerce.Api.Tests;

public class CommerceServiceTests
{
    private readonly Mock<ICsvCommerceParser> _parser = new();
    private readonly Mock<ICommerceRepository> _repository = new();
    private readonly CommerceService _service;

    public CommerceServiceTests()
    {
        _service = new CommerceService(_parser.Object, _repository.Object, NullLogger<CommerceService>.Instance);
    }

    [Fact]
    public async Task UploadAsync_InvalidFileName_ThrowsAndDoesNotTouchDatabase()
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.UploadAsync("datos.csv", new MemoryStream(), CancellationToken.None));

        _parser.VerifyNoOtherCalls();
        _repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UploadAsync_EmptyFile_ThrowsAndDoesNotTouchDatabase()
    {
        _parser
            .Setup(p => p.ParseAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("El archivo está vacío."));

        await Assert.ThrowsAsync<ValidationException>(
            () => _service.UploadAsync("commerce_06102026.csv", new MemoryStream(), CancellationToken.None));

        _repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UploadAsync_ValidFile_InsertsAllRecordsInASingleCall()
    {
        IReadOnlyList<CommerceRecord> records =
        [
            new(new DateOnly(2026, 10, 6), "RUC", "0991234567001", "Tienda", "Tienda S.A.", null, null, null, "ACTIVO"),
            new(new DateOnly(2026, 10, 6), "CED", "0912345678", "Panadería", "Juan Pérez", null, null, null, "ACTIVO"),
        ];
        _parser
            .Setup(p => p.ParseAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);
        _repository
            .Setup(r => r.CreateAsync(records, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        // El navegador puede enviar la ruta completa: solo cuenta el nombre
        var result = await _service.UploadAsync("/datos/commerce_06102026.csv", new MemoryStream(), CancellationToken.None);

        Assert.Equal("commerce_06102026.csv", result.FileName);
        Assert.Equal(2, result.InsertedCount);
        _repository.Verify(r => r.CreateAsync(records, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsQuarantinedCountFromRepository()
    {
        var processDate = new DateOnly(2026, 10, 6);
        _repository
            .Setup(r => r.ProcessAsync(processDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(9);

        var result = await _service.ProcessAsync(processDate, CancellationToken.None);

        Assert.Equal(new ProcessResultDto(processDate, 9), result);
        _repository.Verify(r => r.ProcessAsync(processDate, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetQuarantineAsync_ReturnsRecordsFromRepository()
    {
        IReadOnlyList<QuarantineRecordDto> quarantine =
        [
            new() { Id = 1, PcProcessDate = new DateOnly(2026, 10, 6), Motivo = "El número (numdoc) contiene letras" },
        ];
        _repository
            .Setup(r => r.GetQuarantineAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(quarantine);

        var result = await _service.GetQuarantineAsync(CancellationToken.None);

        Assert.Same(quarantine, result);
    }
}
