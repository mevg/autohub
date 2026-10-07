namespace AutoHub.Application.Features.Settings.Commands.UpdateBranding;

public class UpdateBrandingCommandHandler(IBrandingStore store) : IRequestHandler<UpdateBrandingCommand, Unit>
{
    public async Task<Unit> Handle(UpdateBrandingCommand request, CancellationToken cancellationToken)
    {
        var branding = await store.GetAsync(cancellationToken)
            ?? throw new InvalidOperationException("Branding no inicializado.");

        branding.AppName = request.AppName.Trim();
        branding.PrimaryLight = request.PrimaryLight;
        branding.Primary = request.Primary;
        branding.PrimaryDark = request.PrimaryDark;
        branding.UpdatedAt = DateTime.UtcNow;

        await store.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
