using System.ComponentModel.DataAnnotations;

namespace GrandmastersHub.Domain.Entities;

public class User
{
    public int UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Role { get; set; } = "Customer";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Address> Addresses { get; set; } = new List<Address>();

    public ICollection<Order> Orders { get; set; } = new List<Order>();

    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    public Cart? Cart { get; set; }
}