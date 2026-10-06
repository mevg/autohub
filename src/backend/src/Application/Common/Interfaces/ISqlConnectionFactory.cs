namespace AutoHub.Application.Common.Interfaces;

/// <summary>
/// Crea conexiones SQL abiertas. La implementación vive en Infrastructure;
/// los handlers de lectura (Dapper) dependen solo de esta abstracción.
/// </summary>
public interface ISqlConnectionFactory
{
    Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}
