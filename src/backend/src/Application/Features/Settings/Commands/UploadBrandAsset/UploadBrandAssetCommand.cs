using AutoHub.Application.Features.Settings.Dtos;

namespace AutoHub.Application.Features.Settings.Commands.UploadBrandAsset;

public record UploadBrandAssetCommand(
    string Slot,
    string ContentType,
    byte[] Data
) : IRequest<BrandingDto>;
