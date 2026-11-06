namespace AutoHub.Application.Features.Leads.CreateLead;

public class CreateLeadCommandValidator : AbstractValidator<CreateLeadCommand>
{
    public CreateLeadCommandValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty().WithMessage("El vehículo es requerido.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .Length(2, 100).WithMessage("El nombre debe tener entre 2 y 100 caracteres.")
            .Must(name => !name.Any(char.IsControl)).WithMessage("El nombre contiene caracteres inválidos.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("El teléfono es requerido.")
            .MaximumLength(25).WithMessage("El teléfono es demasiado largo.")
            .Matches(@"^[0-9+()\-\s.]+$").WithMessage("El teléfono contiene caracteres inválidos.")
            .Must(phone => CountDigits(phone) is >= 10 and <= 15)
            .WithMessage("El teléfono debe tener entre 10 y 15 dígitos.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo es requerido.")
            .MaximumLength(254).WithMessage("El correo es demasiado largo.")
            .EmailAddress().WithMessage("El correo no tiene un formato válido.");

        RuleFor(x => x.Message)
            .MaximumLength(1000).WithMessage("El mensaje no puede superar 1000 caracteres.");

        RuleFor(x => x.Website)
            .MaximumLength(200).WithMessage("El campo auxiliar es inválido.");

        RuleFor(x => x.IdempotencyKey)
            .MaximumLength(64).WithMessage("La clave de idempotencia es inválida.");

        RuleFor(x => x.IpHash)
            .MaximumLength(64);

        RuleFor(x => x.UserAgent)
            .MaximumLength(512);
    }

    private static int CountDigits(string value) => value.Count(char.IsDigit);
}
