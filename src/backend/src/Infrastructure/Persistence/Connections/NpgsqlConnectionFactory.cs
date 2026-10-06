namespace AutoHub.Infrastructure.Persistence.Connections;

public sealed class NpgsqlConnectionFactory(string connectionString) : ISqlConnectionFactory
{
    public async Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
