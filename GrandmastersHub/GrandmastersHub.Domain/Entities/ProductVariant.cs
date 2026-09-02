using System.ComponentModel.DataAnnotations;

namespace GrandmastersHub.Domain.Entities;

public class ProductVariant
{
    public int ProductVariantId { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public Inventory? Inventory { get; set; }

    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}