namespace GrandmastersHub.Application.DTOs.Auth;

public sealed record AuthResponseDto(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    int UserId,
    string Email,
    string Role);
