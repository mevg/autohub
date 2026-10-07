namespace AutoHub.Application.Features.Settings.Commands.UpdateBranding;

public class UpdateBrandingCommandValidator : AbstractValidator<UpdateBrandingCommand>
{
    public UpdateBrandingCommandValidator()
    {
        RuleFor(x => x.AppName)
            .NotEmpty().WithMessage("El nombre de la aplicación es requerido.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");

        RuleFor(x => x.PrimaryLight).IsHexColor();
        RuleFor(x => x.Primary).IsHexColor();
        RuleFor(x => x.PrimaryDark).IsHexColor();
    }
}

internal static class HexColorRuleExtensions
{
    public static IRuleBuilderOptions<T, string> IsHexColor<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty().WithMessage("El color es requerido.")
            .Matches("^#[0-9A-Fa-f]{6}$").WithMessage("El color debe ser hexadecimal (#RRGGBB).");
    }
}
