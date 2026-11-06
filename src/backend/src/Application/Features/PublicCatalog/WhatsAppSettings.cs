namespace AutoHub.Application.Features.PublicCatalog;

public sealed record WhatsAppSettings(
    bool Enabled,
    string PhoneNumber,
    string MessageTemplate
)
{
    public const string DefaultMessageTemplate =
        "Hola, me interesa el {make} {model} {year} con referencia {reference}. ¿Podrían darme más información?";
}
