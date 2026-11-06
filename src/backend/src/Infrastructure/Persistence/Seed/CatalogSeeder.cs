using System.Net;
using AutoHub.Application.Common.Interfaces;
using AutoHub.Domain.Entities;
using AutoHub.Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AutoHub.Infrastructure.Persistence.Seed;

/// <summary>
/// Datos demo del catálogo público. Solo se ejecuta si no hay vehículos.
/// Las imágenes son SVG generados en runtime para no versionar binarios.
/// </summary>
public static class CatalogSeeder
{
    private static readonly (string Make, string Model, int Year, decimal Price, int Mileage,
        string Transmission, string FuelType, string Description, string Color)[] DemoVehicles =
    [
        ("Toyota", "Corolla", 2023, 389000m, 18000, "Automática", "Gasolina",
            "Seminuevo certificado, único dueño, servicio completo.", "#4DDBC4"),
        ("Honda", "Civic", 2022, 425000m, 24000, "Automática", "Gasolina",
            "Versión Touring con techo panorámico y cámara 360.", "#4D9BDB"),
        ("Mazda", "CX-5", 2023, 498000m, 15000, "Automática", "Gasolina",
            "SUV familiar con asientos de piel y Apple CarPlay.", "#DB624D"),
        ("Nissan", "Versa", 2024, 289000m, 8000, "Manual", "Gasolina",
            "Económico y práctico, ideal para ciudad.", "#B04DDB"),
        ("Kia", "K3", 2023, 319000m, 21000, "Automática", "Gasolina",
            "Garantía de agencia vigente y mantenimientos al día.", "#DBB44D"),
        ("Hyundai", "Tucson", 2022, 459000m, 32000, "Automática", "Híbrido",
            "SUV híbrida con excelente rendimiento y espacio.", "#4DDB7A"),
        ("Volkswagen", "Jetta", 2021, 279000m, 41000, "Automática", "Gasolina",
            "Versión Highline, interiores impecables.", "#7A4DDB"),
        ("Ford", "Ranger", 2022, 579000m, 38000, "Automática", "Diésel",
            "Pickup 4x4 lista para trabajo y aventura.", "#DB4D9B")
    ];

    public static async Task SeedAsync(
        AppDbContext db,
        IServiceProvider services,
        IConfiguration configuration,
        Guid adminUserId,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        if (!configuration.GetValue("SeedCatalog:Enabled", false))
        {
            return;
        }

        if (await db.Vehicles.AnyAsync(cancellationToken))
        {
            return;
        }

        var webRoot = services.GetRequiredService<IWebHostEnvironment>().WebRootPath
            ?? Path.Combine(services.GetRequiredService<IWebHostEnvironment>().ContentRootPath, "wwwroot");
        var imageDirectory = Path.Combine(webRoot, "seed", "vehicles");
        Directory.CreateDirectory(imageDirectory);

        var references = services.GetRequiredService<IPublicReferenceGenerator>();
        var now = DateTime.UtcNow;

        for (var index = 0; index < DemoVehicles.Length; index++)
        {
            var demo = DemoVehicles[index];
            var vehicleId = Guid.NewGuid();
            var vehicle = new Vehicle
            {
                Id = vehicleId,
                Vin = $"DEMO{index + 1:000000000000}",
                PublicReference = await references.NextAsync(cancellationToken),
                Make = demo.Make,
                Model = demo.Model,
                Year = demo.Year,
                Price = demo.Price,
                Mileage = demo.Mileage,
                Transmission = demo.Transmission,
                FuelType = demo.FuelType,
                Status = VehicleStatus.Available,
                Description = demo.Description,
                CreatedBy = adminUserId,
                CreatedAt = now.AddMinutes(-index * 7)
            };

            for (var photo = 0; photo < 3; photo++)
            {
                var fileName = $"{vehicleId:N}-{photo + 1}.svg";
                var filePath = Path.Combine(imageDirectory, fileName);
                if (!File.Exists(filePath))
                {
                    await File.WriteAllTextAsync(
                        filePath,
                        BuildPlaceholderSvg(demo.Make, demo.Model, demo.Year, demo.Color, photo + 1),
                        cancellationToken);
                }

                vehicle.Images.Add(new VehicleImage
                {
                    Id = Guid.NewGuid(),
                    VehicleId = vehicleId,
                    ImageUrl = $"/seed/vehicles/{fileName}",
                    AltText = $"{demo.Make} {demo.Model} {demo.Year} - foto {photo + 1}",
                    DisplayOrder = photo,
                    CreatedAt = now
                });
            }

            db.Vehicles.Add(vehicle);
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Catálogo demo creado con {Count} vehículos.", DemoVehicles.Length);
    }

    private static string BuildPlaceholderSvg(
        string make,
        string model,
        int year,
        string color,
        int photoNumber)
    {
        var title = WebUtility.HtmlEncode($"{make} {model}");
        var subtitle = WebUtility.HtmlEncode($"{year} · Foto de ejemplo {photoNumber}");
        var initials = WebUtility.HtmlEncode($"{make[0]}{model[0]}");

        return $"""
            <svg xmlns="http://www.w3.org/2000/svg" width="1200" height="800" viewBox="0 0 1200 800" role="img">
              <defs>
                <linearGradient id="bg" x1="0" y1="0" x2="1" y2="1">
                  <stop offset="0%" stop-color="{color}" stop-opacity="0.92"/>
                  <stop offset="100%" stop-color="#111827" stop-opacity="0.96"/>
                </linearGradient>
              </defs>
              <rect width="1200" height="800" fill="url(#bg)"/>
              <circle cx="980" cy="160" r="220" fill="#ffffff" fill-opacity="0.08"/>
              <circle cx="180" cy="700" r="260" fill="#ffffff" fill-opacity="0.06"/>
              <text x="80" y="300" fill="#ffffff" fill-opacity="0.16" font-family="Arial, sans-serif"
                    font-size="230" font-weight="700">{initials}</text>
              <text x="80" y="560" fill="#ffffff" font-family="Arial, sans-serif"
                    font-size="74" font-weight="700">{title}</text>
              <text x="80" y="630" fill="#ffffff" fill-opacity="0.75" font-family="Arial, sans-serif"
                    font-size="34">{subtitle}</text>
            </svg>
            """;
    }
}
