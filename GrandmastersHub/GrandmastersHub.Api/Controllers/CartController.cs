using System.IdentityModel.Tokens.Jwt;
using GrandmastersHub.Application.DTOs.Cart;
using GrandmastersHub.Application.Exceptions;
using GrandmastersHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrandmastersHub.Api.Controllers;

[Authorize]
public sealed class CartController(ICartService carts) : BaseApiController
{
    [HttpGet]
    public Task<ActionResult<CartDto>> Get() => ForUser(carts.GetAsync);

    [HttpPost("items")]
    public Task<ActionResult<CartDto>> Add(AddCartItemRequest request) =>
        ForUser(userId => carts.AddAsync(userId, request));

    [HttpPut("items/{itemId:int:min(1)}")]
    public Task<ActionResult<CartDto>> Update(int itemId, UpdateCartItemRequest request) =>
        ForUser(userId => carts.UpdateAsync(userId, itemId, request.Quantity));

    [HttpDelete("items/{itemId:int:min(1)}")]
    public Task<ActionResult<CartDto>> Remove(int itemId) =>
        ForUser(userId => carts.RemoveAsync(userId, itemId));

    private async Task<ActionResult<CartDto>> ForUser(Func<int, Task<CartDto>> action)
    {
        if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var userId) || userId < 1)
            return Unauthorized(new { message = "Please sign in again." });
        try { return Ok(await action(userId)); }
        catch (CartRequestException exception)
        {
            var status = exception.Error switch
            {
                CartError.NotFound => StatusCodes.Status404NotFound,
                CartError.Conflict => StatusCodes.Status409Conflict,
                CartError.Unauthenticated => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status400BadRequest
            };
            return StatusCode(status, new { message = exception.Message });
        }
    }
}
