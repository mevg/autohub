namespace AutoHub.Infrastructure.Persistence.Stores;

public sealed class EfBrandingStore(AppDbContext db) : IBrandingStore
{
    public Task<Branding?> GetAsync(CancellationToken cancellationToken = default)
        => db.Branding.FirstOrDefaultAsync(b => b.Id == 1, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => db.SaveChangesAsync(cancellationToken);
}
