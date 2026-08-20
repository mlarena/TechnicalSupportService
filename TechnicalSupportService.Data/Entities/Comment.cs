using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechnicalSupportService.Data.Entities;

public class Comment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TicketId { get; set; }

    public Guid AuthorUserId { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    public bool IsInternal { get; set; } = false;

    public bool IsEdited { get; set; } = false;

    public DateTime? EditedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;

    [ForeignKey(nameof(TicketId))]
    public virtual Ticket Ticket { get; set; } = null!;

    [ForeignKey(nameof(AuthorUserId))]
    public virtual ApplicationUser AuthorUser { get; set; } = null!;
}
