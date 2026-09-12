namespace GrandmastersHub.Application.DTOs.Cart;

public sealed record CartItemDto(
    int CartItemId, int ProductId, int ProductVariantId, string Name, string VariantName,
    string CategoryName, string? ImageUrl, int Quantity, decimal UnitPrice, int StockQuantity)
{
    public decimal LineTotal => UnitPrice * Quantity;
    public bool IsAvailable => Quantity <= StockQuantity;
}

