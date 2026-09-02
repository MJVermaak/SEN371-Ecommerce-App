using System.ComponentModel.DataAnnotations;

namespace GrandmastersHub.Domain.Entities;

public class Category
{
    public int CategoryId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}