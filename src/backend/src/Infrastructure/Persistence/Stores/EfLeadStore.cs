using AutoHub.Application.Common.Exceptions;
using AutoHub.Domain.Enums;

namespace AutoHub.Infrastructure.Persistence.Stores;

public sealed class EfLeadStore(AppDbContext db) : ILeadStore
{
    public Task<bool> IsVehicleAvailableAsync(Guid vehicleId, CancellationToken cancellationToken = default)
        => db.Vehicles
            .AsNoTracking()
            .AnyAsync(v => v.Id == vehicleId && v.Status == VehicleStatus.Available, cancellationToken);

    public Task<Lead?> FindByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default)
        => db.Leads
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.IdempotencyKey == idempotencyKey, cancellationToken);

    public Task<Lead?> FindRecentDuplicateAsync(
        Guid vehicleId,
        string email,
        string phone,
        DateTime sinceUtc,
        CancellationToken cancellationToken = default)
        => db.Leads
            .AsNoTracking()
            .FirstOrDefaultAsync(
                l => l.VehicleId == vehicleId
                    && l.Email == email
                    && l.Phone == phone
                    && l.CreatedAt >= sinceUtc,
                cancellationToken);

    public void Add(Lead lead) => db.Leads.Add(lead);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException pg
                && pg.SqlState == PostgresErrorCodes.UniqueViolation
                && pg.ConstraintName?.Contains("IdempotencyKey", StringComparison.OrdinalIgnoreCase) == true)
        {
            var key = ex.Entries.Select(e => e.Entity).OfType<Lead>().FirstOrDefault()?.IdempotencyKey ?? "?";
            throw new DuplicateIdempotencyKeyException(key);
        }
    }
}
