namespace GrandmastersHub.Application.DTOs.Auth;

public sealed record AuthResponseDto(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    Guid UserId,
    string Email,
    string Role);
