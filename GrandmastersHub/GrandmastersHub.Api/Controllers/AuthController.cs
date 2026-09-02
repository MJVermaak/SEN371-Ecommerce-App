using System.IdentityModel.Tokens.Jwt;
using GrandmastersHub.Application.DTOs.Auth;
using GrandmastersHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrandmastersHub.Api.Controllers;

public sealed class AuthController(IAuthService authService) : BaseApiController
{
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType<AuthResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponseDto>> Register(
        RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        var response = await authService.RegisterAsync(request, cancellationToken);
        return response is null
            ? Conflict(new { message = "An account with this email already exists." })
            : StatusCode(StatusCodes.Status201Created, response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<AuthResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login(
        LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var response = await authService.LoginAsync(request, cancellationToken);
        return response is null
            ? Unauthorized(new { message = "Invalid email or password." })
            : Ok(response);
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var subject = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (!Guid.TryParse(subject, out var userId))
        {
            return Unauthorized();
        }

        var user = await authService.GetUserAsync(userId, cancellationToken);
        return user is null
            ? Unauthorized()
            : Ok(new { user.Id, user.Email, Role = user.Role.ToString() });
    }
}
