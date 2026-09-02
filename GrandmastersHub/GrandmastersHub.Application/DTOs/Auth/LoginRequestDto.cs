using System.ComponentModel.DataAnnotations;

namespace GrandmastersHub.Application.DTOs.Auth;

public sealed class LoginRequestDto
{
    [Required, EmailAddress, MaxLength(254)]
    public string Email { get; init; } = string.Empty;

    [Required, MaxLength(128)]
    public string Password { get; init; } = string.Empty;
}
