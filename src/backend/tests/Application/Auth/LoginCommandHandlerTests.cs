using AutoHub.Application.Common.Interfaces;
using AutoHub.Application.Features.Auth.Commands.Login;
using AutoHub.Domain.Entities;
using AutoHub.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace AutoHub.Application.UnitTests.Auth;

public class LoginCommandHandlerTests
{
    private readonly IUserQueries _queries = Substitute.For<IUserQueries>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _tokens = Substitute.For<IJwtTokenGenerator>();
    private LoginCommandHandler Sut => new(_queries, _hasher, _tokens);

    private static User ActiveUser() => new()
    {
        Id = Guid.NewGuid(),
        Email = "admin@autohub.local",
        PasswordHash = "hash",
        FullName = "Administrator",
        Role = UserRole.Admin,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task Handle_valid_credentials_returns_token()
    {
        var user = ActiveUser();
        _queries.FindByEmailAsync(user.Email).Returns(user);
        _hasher.VerifyPassword("Admin123!", user.PasswordHash).Returns(true);
        _tokens.GenerateToken(user).Returns("jwt-token");

        var result = await Sut.Handle(new LoginCommand(user.Email, "Admin123!"), CancellationToken.None);

        result.Token.Should().Be("jwt-token");
        result.Email.Should().Be(user.Email);
        result.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Handle_unknown_email_throws_unauthorized()
    {
        _queries.FindByEmailAsync(Arg.Any<string>()).Returns((User?)null);

        var act = () => Sut.Handle(new LoginCommand("nadie@autohub.local", "x"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Credenciales inválidas.");
    }

    [Fact]
    public async Task Handle_wrong_password_throws_unauthorized()
    {
        var user = ActiveUser();
        _queries.FindByEmailAsync(user.Email).Returns(user);
        _hasher.VerifyPassword(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var act = () => Sut.Handle(new LoginCommand(user.Email, "wrong"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Credenciales inválidas.");
    }

    [Fact]
    public async Task Handle_inactive_user_throws_same_message_without_verifying_password()
    {
        var user = ActiveUser();
        user.IsActive = false;
        _queries.FindByEmailAsync(user.Email).Returns(user);

        var act = () => Sut.Handle(new LoginCommand(user.Email, "Admin123!"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Credenciales inválidas.");
        _hasher.DidNotReceive().VerifyPassword(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_normalizes_email_before_query()
    {
        _queries.FindByEmailAsync(Arg.Any<string>()).Returns((User?)null);

        try { await Sut.Handle(new LoginCommand("  ADMIN@autohub.local ", "x"), CancellationToken.None); }
        catch (UnauthorizedAccessException) { }

        await _queries.Received(1).FindByEmailAsync("admin@autohub.local", Arg.Any<CancellationToken>());
    }
}
