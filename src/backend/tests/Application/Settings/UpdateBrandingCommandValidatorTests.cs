using AutoHub.Application.Features.Settings.Commands.UpdateBranding;
using FluentAssertions;

namespace AutoHub.Application.UnitTests.Settings;

public class UpdateBrandingCommandValidatorTests
{
    private readonly UpdateBrandingCommandValidator _validator = new();

    [Theory]
    [InlineData("AutoHub", "#71E2DD", "#4DDBC4", "#45C5B0", true)]
    [InlineData("", "#71E2DD", "#4DDBC4", "#45C5B0", false)]
    [InlineData("AutoHub", "red", "#4DDBC4", "#45C5B0", false)]
    [InlineData("AutoHub", "#71E2DD", "#4DDBC", "#45C5B0", false)]
    [InlineData("AutoHub", "#71E2DD", "#4DDBC4", "", false)]
    public void Validate_inputs(string name, string light, string primary, string dark, bool expectedValid)
    {
        var result = _validator.Validate(new UpdateBrandingCommand(name, light, primary, dark));

        result.IsValid.Should().Be(expectedValid);
    }
}
