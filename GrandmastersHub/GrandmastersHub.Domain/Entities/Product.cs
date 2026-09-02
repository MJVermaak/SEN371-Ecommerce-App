using System.ComponentModel.DataAnnotations;

namespace GrandmastersHub.Domain.Entities;

public class Product
{
    public int ProductId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();

    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}