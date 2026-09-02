using System.ComponentModel.DataAnnotations;

namespace GrandmastersHub.Domain.Entities;

public class Review
{
    public int ReviewId { get; set; }

    public int UserId { get; set; }

    public User? User { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}