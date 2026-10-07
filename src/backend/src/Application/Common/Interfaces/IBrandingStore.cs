namespace AutoHub.Application.Common.Interfaces;

/// <summary>
/// Escrituras del branding con EF Core (la entidad queda trackeada).
/// </summary>
public interface IBrandingStore
{
    Task<Branding?> GetAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
