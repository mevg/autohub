namespace AutoHub.Infrastructure.Persistence.Queries;

public sealed class NpgsqlPublicReferenceGenerator(ISqlConnectionFactory connectionFactory) : IPublicReferenceGenerator
{
    public async Task<string> NextAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT 'AH-' || lpad(nextval('"VehiclePublicReferenceSequence"')::text, 6, '0');
            """;

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        return await connection.QueryFirstAsync<string>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
    }
}
