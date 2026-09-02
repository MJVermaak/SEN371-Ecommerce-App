using System.ComponentModel.DataAnnotations;

namespace GrandmastersHub.Domain.Entities;

public class ProductImage
{
    public int ProductImageId { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;
}