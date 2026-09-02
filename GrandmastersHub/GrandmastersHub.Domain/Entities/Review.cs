using System;
using System.Collections.Generic;
using System.Text;

namespace GrandmastersHub.Domain.Entities
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
