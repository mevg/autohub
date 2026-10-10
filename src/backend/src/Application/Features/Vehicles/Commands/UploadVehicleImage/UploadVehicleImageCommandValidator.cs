namespace AutoHub.Application.Features.Vehicles.Commands.UploadVehicleImage;

public class UploadVehicleImageCommandValidator : AbstractValidator<UploadVehicleImageCommand>
{
    public UploadVehicleImageCommandValidator()
    {
        RuleFor(x => x.VehicleId).NotEmpty();
        RuleFor(x => x.ContentType)
            .Must(ct => VehicleImageRules.AllowedContentTypes.Contains(ct))
            .WithMessage("Tipo de imagen no permitido (png, jpeg, webp).");
        RuleFor(x => x.Data)
            .NotEmpty().WithMessage("El archivo está vacío.")
            .Must(d => d.Length <= VehicleImageRules.MaxBytes).WithMessage("El archivo supera los 5 MB.");
    }
}
