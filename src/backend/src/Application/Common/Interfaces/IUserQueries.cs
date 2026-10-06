namespace AutoHub.Application.Common.Interfaces;

/// <summary>
/// Lecturas de usuarios con Dapper (CQRS: queries directas con SQL,
/// sin change tracking de EF Core).
/// </summary>
public interface IUserQueries
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
}
