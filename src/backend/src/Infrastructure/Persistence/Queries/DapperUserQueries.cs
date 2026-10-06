namespace AutoHub.Infrastructure.Persistence.Queries;

/// <summary>
/// Lecturas de usuarios con Dapper. Debe mantenerse en sync con
/// <see cref="Configurations.UserConfiguration"/> (tabla y columnas).
/// </summary>
public sealed class DapperUserQueries(ISqlConnectionFactory connectionFactory) : IUserQueries
{
    public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id AS "Id",
                   email AS "Email",
                   password_hash AS "PasswordHash",
                   full_name AS "FullName",
                   role AS "Role",
                   is_active AS "IsActive",
                   created_at AS "CreatedAt"
            FROM users
            WHERE email = @Email
            LIMIT 1;
            """;

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(sql, new { Email = email }, cancellationToken: cancellationToken);
        return await connection.QueryFirstOrDefaultAsync<User?>(command);
    }
}
