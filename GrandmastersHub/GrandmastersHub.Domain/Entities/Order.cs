using System;
using System.Collections.Generic;
using System.Text;

namespace GrandmastersHub.Domain.Entities
{
    public int OrderId { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public User? User { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
