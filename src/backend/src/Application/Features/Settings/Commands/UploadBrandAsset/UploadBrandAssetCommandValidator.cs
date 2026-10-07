namespace AutoHub.Application.Features.Settings.Commands.UploadBrandAsset;

public class UploadBrandAssetCommandValidator : AbstractValidator<UploadBrandAssetCommand>
{
    public static readonly string[] AllowedContentTypes =
    [
        "image/png", "image/jpeg", "image/svg+xml", "image/webp", "image/x-icon", "image/vnd.microsoft.icon"
    ];

    public const int MaxBytes = 5 * 1024 * 1024;

    public UploadBrandAssetCommandValidator()
    {
        RuleFor(x => x.Slot)
            .Must(s => BrandAssetSlots.Allowed.Contains(s))
            .WithMessage($"Slot inválido. Permitidos: {string.Join(", ", BrandAssetSlots.Allowed)}.");

        RuleFor(x => x.ContentType)
            .Must(ct => AllowedContentTypes.Contains(ct))
            .WithMessage("Tipo de imagen no permitido (png, jpeg, svg, webp, ico).");

        RuleFor(x => x.Data)
            .NotEmpty().WithMessage("El archivo está vacío.")
            .Must(d => d.Length <= MaxBytes).WithMessage("El archivo supera los 5 MB.");
    }
}
