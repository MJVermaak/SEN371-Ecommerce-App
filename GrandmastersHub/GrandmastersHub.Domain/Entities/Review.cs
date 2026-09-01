namespace GrandmastersHub.Domain.Entities;

public class Review
{
    public int ReviewId { get; set; }
    public int ProductId { get; set; }
    public Guid UserId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Product? Product { get; set; }
    public User? User { get; set; }
}
