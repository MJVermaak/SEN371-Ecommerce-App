using System.ComponentModel.DataAnnotations;

namespace GrandmastersHub.Application.DTOs.Cart;

public sealed record CartDto(IReadOnlyList<CartItemDto> Items)
{
    public int TotalQuantity => Items.Sum(item => item.Quantity);
    public decimal Subtotal => Items.Sum(item => item.LineTotal);
}

public sealed class AddCartItemRequest
{
    [Range(1, int.MaxValue)] public int ProductId { get; set; }
    [Range(1, int.MaxValue)] public int ProductVariantId { get; set; }
    [Range(1, 99)] public int Quantity { get; set; } = 1;
}

public sealed class UpdateCartItemRequest
{
    [Range(1, 99)] public int Quantity { get; set; }
}
