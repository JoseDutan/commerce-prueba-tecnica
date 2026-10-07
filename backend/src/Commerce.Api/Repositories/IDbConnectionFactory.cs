using System.Data.Common;

namespace Commerce.Api.Repositories;

/// <summary>
/// Crea conexiones a la base de datos. Aísla a los repositorios de la configuración.
/// </summary>
public interface IDbConnectionFactory
{
    DbConnection Create();
}
