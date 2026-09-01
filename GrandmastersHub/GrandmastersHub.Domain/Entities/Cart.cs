namespace GrandmastersHub.Domain.Entities;

public class Cart
{
    public int CartId { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
