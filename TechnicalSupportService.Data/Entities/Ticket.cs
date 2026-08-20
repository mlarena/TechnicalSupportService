using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechnicalSupportService.Core.Enums;

namespace TechnicalSupportService.Data.Entities;

public class Ticket
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)]
    public string Number { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public Guid ProductId { get; set; }

    [MaxLength(20)]
    public string? Version { get; set; }

    public Priority Priority { get; set; }

    public TicketStatus Status { get; set; } = TicketStatus.New;

    public Category Category { get; set; }

    public Impact? Impact { get; set; }

    public Source Source { get; set; }

    public Guid? AssignedToUserId { get; set; }

    public Guid CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid UpdatedByUserId { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ClosedAt { get; set; }

    public string? Resolution { get; set; }

    public int? TimeSpentMinutes { get; set; }

    public Guid? ParentTicketId { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedByUserId { get; set; }

    [ForeignKey(nameof(ProductId))]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey(nameof(AssignedToUserId))]
    public virtual ApplicationUser? AssignedToUser { get; set; }

    [ForeignKey(nameof(CreatedByUserId))]
    public virtual ApplicationUser CreatedByUser { get; set; } = null!;

    [ForeignKey(nameof(UpdatedByUserId))]
    public virtual ApplicationUser UpdatedByUser { get; set; } = null!;

    [ForeignKey(nameof(ParentTicketId))]
    public virtual Ticket? ParentTicket { get; set; }

    [ForeignKey(nameof(DeletedByUserId))]
    public virtual ApplicationUser? DeletedByUser { get; set; }

    public virtual ICollection<Ticket> ChildTickets { get; set; } = new List<Ticket>();
    public virtual ICollection<TicketHistory> History { get; set; } = new List<TicketHistory>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}
