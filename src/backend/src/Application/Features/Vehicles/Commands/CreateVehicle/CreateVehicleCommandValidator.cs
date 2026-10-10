namespace AutoHub.Application.Features.Vehicles.Commands.CreateVehicle;

public class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
    public CreateVehicleCommandValidator()
    {
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
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("La descripción no puede superar 1000 caracteres.");
    }
}

internal static class VehicleValidationExtensions
{
    public static IRuleBuilderOptions<T, string> IsVin<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty().WithMessage("El VIN es requerido.")
            .Matches("^[A-HJ-NPR-Z0-9]{17}$").WithMessage("El VIN debe tener 17 caracteres alfanuméricos (sin I, O, Q).");
    }

    public static IRuleBuilderOptions<T, string> IsVehicleText<T>(this IRuleBuilder<T, string> rule, string name, int max)
    {
        return rule
            .NotEmpty().WithMessage($"{name} es requerido.")
            .MaximumLength(max).WithMessage($"{name} no puede superar {max} caracteres.");
    }
}
