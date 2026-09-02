using System.ComponentModel.DataAnnotations;

namespace GrandmastersHub.Domain.Entities;

public class Payment
{
    public int PaymentId { get; set; }

    public int OrderId { get; set; }

    public Order? Order { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Pending";

    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}