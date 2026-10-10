namespace AutoHub.Application.Features.Vehicles;

public static class VehicleImageRules
{
    public static readonly string[] AllowedContentTypes = ["image/png", "image/jpeg", "image/webp"];
    public const int MaxBytes = 5 * 1024 * 1024;
    public const int MaxPerVehicle = 10;
    public const string StorageFolder = "vehicles";
}
