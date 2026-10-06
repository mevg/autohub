namespace AutoHub.Application.Features.Auth.Dtos;

public record AuthResponseDto(
    Guid Id,
    string Email,
    string FullName,
    string Role,
    string Token
);