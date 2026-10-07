using System.Data;
using Commerce.Api.Models;
using Dapper;

namespace Commerce.Api.Repositories;

/// <inheritdoc />
public sealed class CommerceRepository : ICommerceRepository
{
    private const string CommerceTableType = "dbo.CommerceTableType";

    private readonly IDbConnectionFactory _connectionFactory;

    public CommerceRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateAsync(IReadOnlyCollection<CommerceRecord> records, CancellationToken cancellationToken)
    {
        // Table-Valued Parameter: todas las filas viajan en una sola llamada al stored procedure
        using var rows = ToDataTable(records);
        var parameters = new DynamicParameters();
        parameters.Add("@rows", rows.AsTableValuedParameter(CommerceTableType));

        await using var connection = _connectionFactory.Create();
        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            "dbo.sp_create_commerce",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Construye el DataTable con las columnas en el mismo orden que dbo.CommerceTableType.
    /// </summary>
    private static DataTable ToDataTable(IReadOnlyCollection<CommerceRecord> records)
    {
        var table = new DataTable();
        table.Columns.Add("pc_processdate", typeof(DateTime));
        table.Columns.Add("pc_tipdoc", typeof(string));
        table.Columns.Add("pc_numdoc", typeof(string));
        table.Columns.Add("pc_nomcomred", typeof(string));
        table.Columns.Add("pc_nomcomlegal", typeof(string));
        table.Columns.Add("pc_direccion", typeof(string));
        table.Columns.Add("pc_telefono", typeof(string));
        table.Columns.Add("pc_email", typeof(string));
        table.Columns.Add("pc_estado", typeof(string));

        foreach (var record in records)
        {
            table.Rows.Add(
                record.PcProcessDate.ToDateTime(TimeOnly.MinValue),
                record.PcTipDoc,
                record.PcNumDoc,
                record.PcNomComRed,
                record.PcNomComLegal,
                record.PcDireccion,
                record.PcTelefono,
                record.PcEmail,
                record.PcEstado);
        }

        return table;
    }
}
