using System.ComponentModel.DataAnnotations;

namespace GrandmastersHub.Domain.Entities;

public class Order
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    [Required, MaxLength(50)]
    public string Status { get; set; } = "Pending";

    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Null on orders created before the checkout migration.
    [MaxLength(36)]
    public string? CheckoutKey { get; set; }

    // Immutable checkout receipt: delivery address, item descriptions, prices and request hash.
    // OrderItems remain the relational record of purchased variants and quantities.
    public string? ReceiptJson { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public Payment? Payment { get; set; }
    public Shipment? Shipment { get; set; }
}
