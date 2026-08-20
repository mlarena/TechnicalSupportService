using System.ComponentModel.DataAnnotations;
using TechnicalSupportService.Core.Enums;

namespace TechnicalSupportService.Data.Entities;

public class Product
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public ProductType ProductType { get; set; }

    [MaxLength(20)]
    public string? CurrentVersion { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
