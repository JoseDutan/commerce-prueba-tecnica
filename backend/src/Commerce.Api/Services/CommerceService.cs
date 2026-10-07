using Commerce.Api.Dtos;
using Commerce.Api.Exceptions;
using Commerce.Api.Repositories;

namespace Commerce.Api.Services;

/// <inheritdoc />
public sealed class CommerceService : ICommerceService
{
    private readonly ICsvCommerceParser _parser;
    private readonly ICommerceRepository _repository;
    private readonly ILogger<CommerceService> _logger;

    public CommerceService(
        ICsvCommerceParser parser,
        ICommerceRepository repository,
        ILogger<CommerceService> logger)
    {
        _parser = parser;
        _repository = repository;
        _logger = logger;
    }

    public async Task<UploadResultDto> UploadAsync(string fileName, Stream content, CancellationToken cancellationToken)
    {
        // Algunos navegadores envían la ruta completa: nos quedamos solo con el nombre
        var name = Path.GetFileName(fileName);

        if (!FileNameValidator.TryGetFileDate(name, out _))
        {
            throw new ValidationException(
                "El nombre del archivo debe tener el formato commerce_DDMMYYYY.csv con una fecha válida.");
        }

        var records = await _parser.ParseAsync(content, cancellationToken);
        var inserted = await _repository.CreateAsync(records, cancellationToken);

        _logger.LogInformation("Archivo {FileName} cargado: {Inserted} registros insertados", name, inserted);

        return new UploadResultDto(name, inserted);
    }

    public async Task<ProcessResultDto> ProcessAsync(DateOnly processDate, CancellationToken cancellationToken)
    {
        var quarantined = await _repository.ProcessAsync(processDate, cancellationToken);

        _logger.LogInformation("Fecha {ProcessDate:yyyy-MM-dd} procesada: {Quarantined} registros a cuarentena",
            processDate, quarantined);

        return new ProcessResultDto(processDate, quarantined);
    }

    public Task<IReadOnlyList<QuarantineRecordDto>> GetQuarantineAsync(CancellationToken cancellationToken)
        => _repository.GetQuarantineAsync(cancellationToken);
}
