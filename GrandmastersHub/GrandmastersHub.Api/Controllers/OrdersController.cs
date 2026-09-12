using System.IdentityModel.Tokens.Jwt;
using GrandmastersHub.Application.DTOs.Orders;
using GrandmastersHub.Application.Exceptions;
using GrandmastersHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrandmastersHub.Api.Controllers;

[Authorize]
public sealed class OrdersController(IOrderService orders) : BaseApiController
{
    [HttpGet("checkout")]
    public Task<IActionResult> Preview(CancellationToken cancellationToken) =>
        Run(async userId => Ok(await orders.PreviewAsync(userId, cancellationToken)));

    [HttpPost]
    public Task<IActionResult> Place(PlaceOrderRequest request, CancellationToken cancellationToken) =>
        Run(async userId =>
        {
            var result = await orders.PlaceAsync(userId, request, cancellationToken);
            return result.Created
                ? CreatedAtAction(nameof(Get), new { id = result.Order.OrderId }, result.Order)
                : Ok(result.Order);
        });

    [HttpGet]
    public Task<IActionResult> List(CancellationToken cancellationToken, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10) =>
        Run(async userId => Ok(await orders.ListAsync(userId, page, pageSize, cancellationToken)));

    [HttpGet("{id:int}")]
    public Task<IActionResult> Get(int id, CancellationToken cancellationToken) =>
        Run(async userId => Ok(await orders.GetAsync(userId, id, cancellationToken)));

    [HttpGet("by-checkout/{key:guid}")]
    public Task<IActionResult> GetByCheckout(Guid key, CancellationToken cancellationToken) =>
        Run(async userId => Ok(await orders.GetByKeyAsync(userId, key, cancellationToken)));

    private async Task<IActionResult> Run(Func<int, Task<IActionResult>> action)
    {
        if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var userId) || userId <= 0)
            return Unauthorized();
        try { return await action(userId); }
        catch (OrderRequestException exception)
        {
            var status = exception.Failure switch
            {
                OrderFailure.Unauthenticated => 401, OrderFailure.NotFound => 404,
                OrderFailure.Conflict => 409, _ => 400
            };
            return StatusCode(status, new { message = exception.Message });
        }
    }
}
