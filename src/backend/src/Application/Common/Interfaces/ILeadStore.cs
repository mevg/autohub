namespace AutoHub.Application.Common.Interfaces;

public interface ILeadStore
{
    Task<bool> IsVehicleAvailableAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<Lead?> FindByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    Task<Lead?> FindRecentDuplicateAsync(
        Guid vehicleId,
        string email,
        string phone,
        DateTime sinceUtc,
        CancellationToken cancellationToken = default);
    void Add(Lead lead);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
