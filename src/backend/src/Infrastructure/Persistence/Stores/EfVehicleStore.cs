using AutoHub.Application.Common.Exceptions;

namespace AutoHub.Infrastructure.Persistence.Stores;

public sealed class EfVehicleStore(AppDbContext db) : IVehicleStore
{
    public Task<Vehicle?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
        => db.Vehicles.Include(v => v.Images).FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public void Add(Vehicle vehicle)
        => db.Vehicles.Add(vehicle);

    public void AddImage(VehicleImage image)
        => db.VehicleImages.Add(image);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            var vin = ex.Entries.Select(e => e.Entity).OfType<Vehicle>().FirstOrDefault()?.Vin ?? "?";
            throw new DuplicateVinException(vin);
        }
    }
}
