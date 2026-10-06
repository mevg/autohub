using AutoHub.Application.Features.Auth.Commands.Login;
using FluentAssertions;

namespace AutoHub.Application.UnitTests.Auth;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Theory]
    [InlineData("admin@autohub.local", "Admin123!", true)]
    [InlineData("no-es-email", "Admin123!", false)]
    [InlineData("", "Admin123!", false)]
    [InlineData("admin@autohub.local", "", false)]
    public void Validate_inputs(string email, string password, bool expectedValid)
    {
        var result = _validator.Validate(new LoginCommand(email, password));

        result.IsValid.Should().Be(expectedValid);
    }
}
