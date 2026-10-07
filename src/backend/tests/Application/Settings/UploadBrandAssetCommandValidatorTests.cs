using AutoHub.Application.Common.Interfaces;
using AutoHub.Application.Features.Settings.Commands.UploadBrandAsset;
using FluentAssertions;

namespace AutoHub.Application.UnitTests.Settings;

public class UploadBrandAssetCommandValidatorTests
{
    private readonly UploadBrandAssetCommandValidator _validator = new();
    private static readonly byte[] Png = new byte[100];

    [Fact]
    public void Valid_command_passes()
    {
        var result = _validator.Validate(new UploadBrandAssetCommand(BrandAssetSlots.Logo, "image/png", Png));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("avatar")]
    [InlineData("")]
    public void Invalid_slot_fails(string slot)
    {
        var result = _validator.Validate(new UploadBrandAssetCommand(slot, "image/png", Png));

        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("text/html")]
    public void Disallowed_mime_fails(string contentType)
    {
        var result = _validator.Validate(new UploadBrandAssetCommand(BrandAssetSlots.Logo, contentType, Png));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Oversized_file_fails()
    {
        var big = new byte[UploadBrandAssetCommandValidator.MaxBytes + 1];

        var result = _validator.Validate(new UploadBrandAssetCommand(BrandAssetSlots.Banner, "image/jpeg", big));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_file_fails()
    {
        var result = _validator.Validate(
            new UploadBrandAssetCommand(BrandAssetSlots.Favicon, "image/x-icon", []));

        result.IsValid.Should().BeFalse();
    }
}
