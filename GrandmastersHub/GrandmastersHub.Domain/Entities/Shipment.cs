using System.ComponentModel.DataAnnotations;

namespace GrandmastersHub.Domain.Entities;

public class Shipment
{
    public int ShipmentId { get; set; }

    public int OrderId { get; set; }

    public Order? Order { get; set; }

    [MaxLength(100)]
    public string? TrackingNumber { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Pending";

    public DateTime? ShippedAt { get; set; }

    public DateTime? DeliveredAt { get; set; }
}