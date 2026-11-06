using AutoHub.Application.Features.Vehicles;
using FluentAssertions;

namespace AutoHub.Application.UnitTests.Vehicles;

public class ImageContentTypeSnifferTests
{
    [Fact]
    public void Keeps_explicit_content_type()
    {
        ImageContentTypeSniffer.Sniff("image/webp", [1, 2, 3]).Should().Be("image/webp");
    }

    [Fact]
    public void Sniffs_webp_from_signature()
    {
        var webp = new byte[] { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50 };

        ImageContentTypeSniffer.Sniff("application/octet-stream", webp).Should().Be("image/webp");
    }

    [Fact]
    public void Sniffs_jpeg_from_signature()
    {
        var jpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };

        ImageContentTypeSniffer.Sniff("", jpeg).Should().Be("image/jpeg");
    }

    [Fact]
    public void Sniffs_png_from_signature()
    {
        var png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        ImageContentTypeSniffer.Sniff("application/octet-stream", png).Should().Be("image/png");
    }

    [Fact]
    public void Unknown_signature_keeps_original()
    {
        ImageContentTypeSniffer.Sniff("application/octet-stream", [1, 2]).Should().Be("application/octet-stream");
    }
}
