using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechnicalSupportService.Data.Entities;

public class Attachment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TicketId { get; set; }

    [Required, MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string StoredFileName { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    [Required, MaxLength(100)]
    public string MimeType { get; set; } = string.Empty;

    public Guid UploadedByUserId { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    [ForeignKey(nameof(TicketId))]
    public virtual Ticket Ticket { get; set; } = null!;

    [ForeignKey(nameof(UploadedByUserId))]
    public virtual ApplicationUser UploadedByUser { get; set; } = null!;
}
