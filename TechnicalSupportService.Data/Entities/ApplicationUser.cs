using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechnicalSupportService.Data.Entities;

public class ApplicationUser : Microsoft.AspNetCore.Identity.IdentityUser<Guid>
{
    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    public Guid? DepartmentId { get; set; }

    [MaxLength(100)]
    public string? Position { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(DepartmentId))]
    public virtual Department? Department { get; set; }
}
