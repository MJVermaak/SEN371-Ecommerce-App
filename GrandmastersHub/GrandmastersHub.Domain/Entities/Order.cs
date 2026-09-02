using System.ComponentModel.DataAnnotations;

namespace GrandmastersHub.Domain.Entities;

public class Order
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public User? User { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Pending";

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    public Payment? Payment { get; set; }

    public Shipment? Shipment { get; set; }
}