using AutoHub.Infrastructure.Authentication;
using FluentAssertions;

namespace AutoHub.IntegrationTests.Auth;

public class PasswordHasherTests
{
    private readonly PasswordHasher _sut = new();

    [Fact]
    public void Hash_and_verify_roundtrip()
    {
        var hash = _sut.HashPassword("Admin123!");

        hash.Should().NotBe("Admin123!");
        _sut.VerifyPassword("Admin123!", hash).Should().BeTrue();
        _sut.VerifyPassword("Wrong123!", hash).Should().BeFalse();
    }
}
