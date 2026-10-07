using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace Commerce.Api.Repositories;

/// <summary>
/// Fábrica de conexiones a SQL Server.
/// La cadena de conexión versionada no lleva contraseña: se toma de la clave de
/// configuración SQL_SA_PASSWORD (variable de entorno o user-secrets).
/// </summary>
public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private const string ConnectionName = "CommerceDb";
    private const string PasswordKey = "SQL_SA_PASSWORD";

    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        var baseConnectionString = configuration.GetConnectionString(ConnectionName)
            ?? throw new InvalidOperationException($"Falta la cadena de conexión '{ConnectionName}'.");

        var builder = new SqlConnectionStringBuilder(baseConnectionString);

        var password = configuration[PasswordKey];
        if (!string.IsNullOrEmpty(password))
        {
            builder.Password = password;
        }

        _connectionString = builder.ConnectionString;
    }

    // ADO.NET mantiene un pool por cadena de conexión: abrir y cerrar por operación es barato
    public DbConnection Create() => new SqlConnection(_connectionString);
}
