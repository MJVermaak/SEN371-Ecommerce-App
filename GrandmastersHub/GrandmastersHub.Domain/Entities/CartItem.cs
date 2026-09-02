namespace GrandmastersHub.Domain.Entities;

public class CartItem
{
    public int CartItemId { get; set; }

    public int CartId { get; set; }

    public Cart? Cart { get; set; }

    public int ProductVariantId { get; set; }

    public ProductVariant? ProductVariant { get; set; }

    public int Quantity { get; set; }
}