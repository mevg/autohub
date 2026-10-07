namespace AutoHub.Application.Features.Settings.Commands.UpdateBranding;

public record UpdateBrandingCommand(
    string AppName,
    string PrimaryLight,
    string Primary,
    string PrimaryDark
) : IRequest<Unit>;
