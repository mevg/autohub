using AutoHub.Application.Features.Vehicles.Commands.CreateVehicle;
using AutoHub.Domain.Enums;

namespace AutoHub.Application.Features.Vehicles.Commands.UpdateVehicle;

public class UpdateVehicleCommandValidator : AbstractValidator<UpdateVehicleCommand>
{
    public UpdateVehicleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Vin).IsVin();
        RuleFor(x => x.Make).IsVehicleText("La marca", 50);
        RuleFor(x => x.Model).IsVehicleText("El modelo", 50);
        RuleFor(x => x.Year)
            .InclusiveBetween(1900, DateTime.UtcNow.Year + 1)
            .WithMessage("El año es inválido.");
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("El precio debe ser mayor que cero.");
        RuleFor(x => x.Mileage)
            .GreaterThanOrEqualTo(0).WithMessage("El kilometraje no puede ser negativo.");
        RuleFor(x => x.Transmission).IsVehicleText("La transmisión", 30);
        RuleFor(x => x.FuelType).IsVehicleText("El combustible", 30);
        RuleFor(x => x.Status)
            .Must(s => Enum.TryParse<VehicleStatus>(s, true, out _))
            .WithMessage("El estado es inválido.");
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("La descripción no puede superar 1000 caracteres.");
    }
}
