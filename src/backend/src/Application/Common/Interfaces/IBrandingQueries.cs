namespace AutoHub.Application.Common.Interfaces;

/// <summary>
/// Lectura del branding con Dapper (fila única).
/// </summary>
public interface IBrandingQueries
{
    Task<Branding?> GetAsync(CancellationToken cancellationToken = default);
}
