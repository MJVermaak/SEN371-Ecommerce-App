using System.ComponentModel.DataAnnotations;

namespace GrandmastersHub.Domain.Entities;

public class Address
{
    public int AddressId { get; set; }

    public int UserId { get; set; }

    public User? User { get; set; }

    [Required]
    [MaxLength(100)]
    public string StreetAddress { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Province { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string PostalCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;
}