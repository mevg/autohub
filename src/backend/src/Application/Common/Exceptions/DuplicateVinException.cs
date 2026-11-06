namespace AutoHub.Application.Common.Exceptions;

public class DuplicateVinException(string vin) : Exception($"Ya existe un vehículo con VIN '{vin}'.")
{
    public string Vin { get; } = vin;
}
