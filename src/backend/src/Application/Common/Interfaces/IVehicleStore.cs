namespace AutoHub.Application.Common.Interfaces;

/// <summary>
/// Escrituras de vehículos con EF Core (entidades trackeadas).
/// Traduce violación de VIN único a <see cref="Exceptions.DuplicateVinException"/>.
/// </summary>
public interface IVehicleStore
{
    Task<Vehicle?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Vehicle vehicle);
    void AddImage(VehicleImage image);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
