using System;
using System.Collections.Generic;
using System.Text;

namespace GrandmastersHub.Domain.Entities
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public required string Email { get; set; }
    public required string NormalizedEmail { get; set; }
    public required string PasswordHash { get; set; }
    public UserRole Role { get; set; } = UserRole.Customer;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public Cart? Cart { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
