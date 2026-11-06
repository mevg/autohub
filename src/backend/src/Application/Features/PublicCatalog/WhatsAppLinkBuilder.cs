using System.Text;

namespace AutoHub.Application.Features.PublicCatalog;

public static class WhatsAppLinkBuilder
{
    public static string? Build(
        WhatsAppSettings settings,
        string make,
        string model,
        int year,
        string publicReference)
    {
        if (!settings.Enabled || string.IsNullOrWhiteSpace(settings.PhoneNumber))
        {
            return null;
        }

        var phone = NormalizePhone(settings.PhoneNumber);
        if (phone.Length < 8)
        {
            return null;
        }

        var template = string.IsNullOrWhiteSpace(settings.MessageTemplate)
            ? WhatsAppSettings.DefaultMessageTemplate
            : settings.MessageTemplate;

        var message = template
            .Replace("{make}", make, StringComparison.OrdinalIgnoreCase)
            .Replace("{model}", model, StringComparison.OrdinalIgnoreCase)
            .Replace("{year}", year.ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
            .Replace("{reference}", publicReference, StringComparison.OrdinalIgnoreCase);

        return $"https://wa.me/{phone}?text={Uri.EscapeDataString(message)}";
    }

    public static string NormalizePhone(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            if (char.IsDigit(character))
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }
}
