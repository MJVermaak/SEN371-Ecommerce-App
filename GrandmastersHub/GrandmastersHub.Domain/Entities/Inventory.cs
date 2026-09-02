namespace GrandmastersHub.Domain.Entities;

public class Inventory
{
    public int InventoryId { get; set; }

    public int ProductVariantId { get; set; }

    public ProductVariant? ProductVariant { get; set; }

    public int Quantity { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}