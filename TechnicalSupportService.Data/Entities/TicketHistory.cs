using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechnicalSupportService.Core.Enums;

namespace TechnicalSupportService.Data.Entities;

public class TicketHistory
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TicketId { get; set; }

    public Guid ChangedByUserId { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string? FieldName { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public ChangeType ChangeType { get; set; }

    public Guid? CommentId { get; set; }

    public Guid? AttachmentId { get; set; }

    [ForeignKey(nameof(TicketId))]
    public virtual Ticket Ticket { get; set; } = null!;

    [ForeignKey(nameof(ChangedByUserId))]
    public virtual ApplicationUser ChangedByUser { get; set; } = null!;

    [ForeignKey(nameof(CommentId))]
    public virtual Comment? Comment { get; set; }

    [ForeignKey(nameof(AttachmentId))]
    public virtual Attachment? Attachment { get; set; }
}
